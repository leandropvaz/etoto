using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EToto.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EToto.Infrastructure.Storage;

public class BlobDownloadService : IBlobDownloadService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<BlobDownloadService> _logger;

    public BlobDownloadService(
        IConfiguration configuration,
        ILogger<BlobDownloadService> logger)
    {
        var connectionString = configuration.GetConnectionString("BlobStorage")
            ?? configuration["BlobStorage:ConnectionString"];

        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("Blob Storage connection string não configurada.");

        _blobServiceClient = new BlobServiceClient(connectionString);
        _logger = logger;
    }

    public async Task<Application.Interfaces.BlobDownloadResult> DownloadFileAsync(
        string containerName,
        string blobName,
        string localFilePath,
        CancellationToken ct = default)
    {
        var result = new Application.Interfaces.BlobDownloadResult
        {
            BlobName = blobName,
            LocalFilePath = localFilePath,
            DownloadedAt = DateTime.UtcNow
        };

        try
        {
            _logger.LogInformation("Baixando blob {BlobName} do container {Container}", blobName, containerName);

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            // Verificar se o blob existe
            if (!await blobClient.ExistsAsync(ct))
            {
                result.Success = false;
                result.ErrorMessage = "Blob não encontrado.";
                _logger.LogWarning("Blob {BlobName} não existe no container {Container}", blobName, containerName);
                return result;
            }

            // Criar diretório se não existir
            var directory = Path.GetDirectoryName(localFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                _logger.LogDebug("Diretório criado: {Directory}", directory);
            }

            // Baixar o arquivo
            var response = await blobClient.DownloadToAsync(localFilePath, ct);

            result.Success = true;
            result.FileSizeBytes = new FileInfo(localFilePath).Length;

            _logger.LogInformation(
                "Blob {BlobName} baixado com sucesso. Tamanho: {Size} bytes. Local: {Path}",
                blobName, result.FileSizeBytes, localFilePath);

            return result;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Erro ao baixar blob {BlobName} do container {Container}", blobName, containerName);
            return result;
        }
    }

    public async Task<List<Application.Interfaces.BlobDownloadResult>> DownloadFilesAsync(
        string containerName,
        string localDirectoryPath,
        string? prefixFilter = null,
        CancellationToken ct = default)
    {
        var results = new List<Application.Interfaces.BlobDownloadResult>();

        try
        {
            _logger.LogInformation(
                "Baixando arquivos do container {Container} com prefixo '{Prefix}' para {LocalPath}",
                containerName, prefixFilter ?? "(nenhum)", localDirectoryPath);

            // Criar diretório se não existir
            if (!Directory.Exists(localDirectoryPath))
            {
                Directory.CreateDirectory(localDirectoryPath);
                _logger.LogDebug("Diretório criado: {Directory}", localDirectoryPath);
            }

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            // Listar blobs com prefixo opcional
            await foreach (var blobItem in containerClient.GetBlobsAsync(prefix: prefixFilter, cancellationToken: ct))
            {
                var blobName = blobItem.Name;
                var localFilePath = Path.Combine(localDirectoryPath, blobName);

                var result = await DownloadFileAsync(containerName, blobName, localFilePath, ct);
                results.Add(result);
            }

            _logger.LogInformation(
                "Download em lote concluído. Total: {Total}, Sucesso: {Success}, Erro: {Error}",
                results.Count,
                results.Count(r => r.Success),
                results.Count(r => !r.Success));

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao baixar arquivos do container {Container}", containerName);
            throw;
        }
    }

    public async Task<Stream> DownloadToStreamAsync(
        string containerName,
        string blobName,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogDebug("Baixando blob {BlobName} para stream", blobName);

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var memoryStream = new MemoryStream();
            await blobClient.DownloadToAsync(memoryStream, ct);
            memoryStream.Position = 0;

            _logger.LogDebug("Blob {BlobName} baixado para stream. Tamanho: {Size} bytes",
                blobName, memoryStream.Length);

            return memoryStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao baixar blob {BlobName} para stream", blobName);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(
        string containerName,
        string blobName,
        CancellationToken ct = default)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            return await blobClient.ExistsAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar existência do blob {BlobName}", blobName);
            return false;
        }
    }

    public async Task<List<Application.Interfaces.BlobInfo>> ListBlobsAsync(
        string containerName,
        string? prefixFilter = null,
        CancellationToken ct = default)
    {
        var blobs = new List<Application.Interfaces.BlobInfo>();

        try
        {
            _logger.LogInformation("Listando blobs do container {Container} com prefixo '{Prefix}'",
                containerName, prefixFilter ?? "(nenhum)");

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            await foreach (var blobItem in containerClient.GetBlobsAsync(
                prefix: prefixFilter,
                cancellationToken: ct))
            {
                blobs.Add(new Application.Interfaces.BlobInfo
                {
                    Name = blobItem.Name,
                    Container = containerName,
                    SizeInBytes = blobItem.Properties.ContentLength ?? 0,
                    LastModified = blobItem.Properties.LastModified,
                    ContentType = blobItem.Properties.ContentType ?? string.Empty
                });
            }

            _logger.LogInformation("Encontrados {Count} blob(s) no container {Container}",
                blobs.Count, containerName);

            return blobs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar blobs do container {Container}", containerName);
            throw;
        }
    }
}