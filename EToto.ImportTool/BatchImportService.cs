using EToto.Application.Interfaces;
using EToto.Domain.Entities;
using EToto.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace EToto.ImportTool;

public class BatchImportService : IBatchImportService
{
    private readonly IEquipamentoRepository _equipmentRepository;
    private readonly IBlobStorageService _blobStorage;
    private readonly IEquipamentoExcelParser _excelParser;
    private readonly IImageOcrService _ocrService;
    private readonly ILogger<BatchImportService> _logger;

    private const string ExcelContainer = "excel";
    private const string ImagesContainer = "imagens";

    public BatchImportService(
        IEquipamentoRepository equipmentRepository,
        IBlobStorageService blobStorage,
        IEquipamentoExcelParser excelParser,
        IImageOcrService ocrService,
        ILogger<BatchImportService> logger)
    {
        _equipmentRepository = equipmentRepository;
        _blobStorage = blobStorage;
        _excelParser = excelParser;
        _ocrService = ocrService;
        _logger = logger;
    }

    public async Task ImportBatchAsync(int plantaId, string directoryPath, CancellationToken ct = default)
    {
        // Criar estrutura de pastas no diretório raiz
        var successPath = Path.Combine(directoryPath, "_success");
        var errorPath = Path.Combine(directoryPath, "_error");
        Directory.CreateDirectory(successPath);
        Directory.CreateDirectory(errorPath);

        // Buscar todos os arquivos Excel recursivamente, excluindo pastas de controle
        var excelFiles = GetExcelFilesRecursive(directoryPath);

        if (!excelFiles.Any())
        {
            _logger.LogWarning("Nenhum arquivo Excel encontrado no diretório: {Path}", directoryPath);
            return;
        }

        _logger.LogInformation("Encontrados {Count} arquivo(s) para importação (incluindo subpastas)", excelFiles.Count);

        // Mostrar estrutura de pastas encontradas
        var folders = excelFiles
            .Select(f => Path.GetDirectoryName(f))
            .Distinct()
            .OrderBy(f => f)
            .ToList();

        _logger.LogInformation("Pastas encontradas:");
        foreach (var folder in folders)
        {
            var relativePath = Path.GetRelativePath(directoryPath, folder!);
            var filesInFolder = excelFiles.Count(f => Path.GetDirectoryName(f) == folder);
            _logger.LogInformation("  • {Path} ({Count} arquivo(s))",
                relativePath == "." ? "[raiz]" : relativePath,
                filesInFolder);
        }

        var successCount = 0;
        var errorCount = 0;

        foreach (var filePath in excelFiles)
        {
            var fileName = Path.GetFileName(filePath);
            var relativeFilePath = Path.GetRelativePath(directoryPath, filePath);

            _logger.LogInformation("\n--- Processando: {RelativePath} ---", relativeFilePath);

            try
            {
                await ImportFileTransactionalAsync(plantaId, filePath, ct);

                // Manter estrutura de pastas no _success
                var relativeDir = Path.GetDirectoryName(relativeFilePath) ?? string.Empty;
                var successSubPath = Path.Combine(successPath, relativeDir);
                Directory.CreateDirectory(successSubPath);

                var destPath = Path.Combine(successSubPath, fileName);

                // Se o arquivo já existe no destino, adicionar timestamp
                if (File.Exists(destPath))
                {
                    var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                    var extension = Path.GetExtension(fileName);
                    var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    destPath = Path.Combine(successSubPath, $"{fileNameWithoutExt}_{timestamp}{extension}");
                }

                File.Move(filePath, destPath, overwrite: false);

                successCount++;
                _logger.LogInformation("✓ {FileName} importado com sucesso", relativeFilePath);
            }
            catch (Exception ex)
            {
                // Manter estrutura de pastas no _error
                var relativeDir = Path.GetDirectoryName(relativeFilePath) ?? string.Empty;
                var errorSubPath = Path.Combine(errorPath, relativeDir);
                Directory.CreateDirectory(errorSubPath);

                var destPath = Path.Combine(errorSubPath, fileName);

                // Se o arquivo já existe no destino, adicionar timestamp
                if (File.Exists(destPath))
                {
                    var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                    var extension = Path.GetExtension(fileName);
                    var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    destPath = Path.Combine(errorSubPath, $"{fileNameWithoutExt}_{timestamp}{extension}");
                }

                File.Move(filePath, destPath, overwrite: false);

                // Criar arquivo de log do erro mantendo a estrutura
                var errorLogFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_error.txt";
                var errorLogPath = Path.Combine(errorSubPath, errorLogFileName);

                await File.WriteAllTextAsync(errorLogPath,
                    $"Data: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n" +
                    $"Arquivo: {relativeFilePath}\n" +
                    $"Erro: {ex.Message}\n" +
                    $"Stack: {ex.StackTrace}", ct);

                errorCount++;
                _logger.LogError(ex, "✗ Erro ao importar {FileName}", relativeFilePath);
            }
        }

        // Limpar pastas vazias no diretório raiz (exceto _success e _error)
        CleanEmptyDirectories(directoryPath);

        _logger.LogInformation("\n=== Resumo ===");
        _logger.LogInformation("Total processado: {Total}", excelFiles.Count);
        _logger.LogInformation("Sucesso: {Success}", successCount);
        _logger.LogInformation("Erro: {Error}", errorCount);
        _logger.LogInformation("\nArquivos organizados em:");
        _logger.LogInformation("  • Sucesso: {Path}", successPath);
        _logger.LogInformation("  • Erro: {Path}", errorPath);
    }

    /// <summary>
    /// Busca recursivamente todos os arquivos Excel, excluindo pastas de controle
    /// </summary>
    private List<string> GetExcelFilesRecursive(string rootPath)
    {
        var excelFiles = new List<string>();
        var excludedFolders = new[] { "_success", "_error", "_backup", "_temp" };

        try
        {
            // Buscar arquivos no diretório raiz
            var filesInRoot = Directory.GetFiles(rootPath, "*.xlsx")
                .Where(f => !Path.GetFileName(f).StartsWith("~$")); // Ignorar arquivos temporários do Excel

            excelFiles.AddRange(filesInRoot);

            // Buscar em subdiretórios
            var subdirectories = Directory.GetDirectories(rootPath);

            foreach (var subdirectory in subdirectories)
            {
                var folderName = Path.GetFileName(subdirectory);

                // Ignorar pastas de controle
                if (excludedFolders.Contains(folderName, StringComparer.OrdinalIgnoreCase))
                {
                    _logger.LogDebug("Ignorando pasta: {Folder}", folderName);
                    continue;
                }

                // Recursão para subpastas
                var filesInSubdirectory = GetExcelFilesRecursive(subdirectory);
                excelFiles.AddRange(filesInSubdirectory);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Sem permissão para acessar: {Path}. Erro: {Message}", rootPath, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar arquivos em: {Path}", rootPath);
        }

        return excelFiles;
    }

    /// <summary>
    /// Remove pastas vazias recursivamente (exceto pastas de controle)
    /// </summary>
    private void CleanEmptyDirectories(string rootPath)
    {
        var excludedFolders = new[] { "_success", "_error", "_backup", "_temp" };

        try
        {
            var subdirectories = Directory.GetDirectories(rootPath);

            foreach (var subdirectory in subdirectories)
            {
                var folderName = Path.GetFileName(subdirectory);

                // Não limpar pastas de controle
                if (excludedFolders.Contains(folderName, StringComparer.OrdinalIgnoreCase))
                    continue;

                // Recursão para subpastas
                CleanEmptyDirectories(subdirectory);

                // Se a pasta está vazia após limpar subpastas, remove
                if (!Directory.EnumerateFileSystemEntries(subdirectory).Any())
                {
                    Directory.Delete(subdirectory);
                    _logger.LogDebug("Pasta vazia removida: {Path}", Path.GetRelativePath(rootPath, subdirectory));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao limpar diretórios vazios em: {Path}", rootPath);
        }
    }

    private async Task ImportFileTransactionalAsync(
        int plantaId,
        string filePath,
        CancellationToken ct)
    {
        await using var transaction = await _equipmentRepository.BeginTransactionAsync(ct);

        try
        {
            await using var fileStream = File.OpenRead(filePath);
            var fileName = Path.GetFileName(filePath);

            // 1) Upload do Excel para Blob Storage
            fileStream.Position = 0;
            var excelUrl = await _blobStorage.UploadAsync(
                fileStream,
                ExcelContainer,
                $"{Guid.NewGuid()}-{fileName}",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ct);

            _logger.LogInformation("Excel enviado para Blob Storage: {Url}", excelUrl);

            // 2) Parse do arquivo Excel com validação de linhas
            fileStream.Position = 0;
            var parsed = await _excelParser.ParseAsync(fileStream, ct);

            if (!parsed.Rows.Any())
            {
                throw new InvalidOperationException("Nenhuma linha válida encontrada no arquivo");
            }

            _logger.LogInformation("Parseadas {Count} linha(s) válida(s)", parsed.Rows.Count);

            // 3) Criar registros e fazer upload das imagens
            var records = new List<Equipamento>();

            foreach (var row in parsed.Rows)
            {
                var entity = new Equipamento
                {
                    Id = Guid.NewGuid(),
                    PlantaId = plantaId,

                    // Cabeçalho
                    Tag = parsed.Tag,
                    EquipmentName = parsed.EquipmentName,
                    FactoryName = parsed.FactoryName,
                    RevisionInfo = parsed.RevisionInfo,

                    // I. Identificação
                    LineNumber = row.LineNumber,
                    EnergyType = row.EnergyType,
                    HazardDescription = row.HazardDescription,

                    // II. Controle
                    IsolationDeviceTag = row.IsolationDeviceTag,
                    IsolationDeviceLocation = row.IsolationDeviceLocation,
                    IsolationDeviceDescription = row.IsolationDeviceDescription,
                    LockoutType = row.LockoutType,
                    Test = row.Test,
                    ZeroEnergyVerification = row.ZeroEnergyVerification,

                    SourceExcelBlobUrl = excelUrl,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    ImageNotes = row.ShapeNotes
                };

                // Upload da imagem específica desta linha
                if (row.ImageBytes is { Length: > 0 })
                {
                    await using var imgStream = new MemoryStream(row.ImageBytes);
                    var imageBlobName = $"{entity.Id}-L{row.LineNumber}.png";

                    entity.ImageBlobUrl = await _blobStorage.UploadAsync(
                        imgStream,
                        ImagesContainer,
                        imageBlobName,
                        "image/png",
                        ct);

                    _logger.LogDebug("Imagem da linha {Line} enviada: {Url}",
                        row.LineNumber, entity.ImageBlobUrl);
                }

                records.Add(entity);
            }

            // 4) Salvar todos os registros no banco
            await _equipmentRepository.AddRangeAsync(records, ct);
            await _equipmentRepository.SaveChangesAsync(ct);

            // 5) Commit da transação
            await transaction.CommitAsync(ct);

            _logger.LogInformation("Transação confirmada. {Count} equipamento(s) importado(s)", records.Count);
        }
        catch
        {
            // Rollback automático em caso de erro
            await transaction.RollbackAsync(ct);
            _logger.LogWarning("Transação revertida (rollback)");
            throw;
        }
    }
}