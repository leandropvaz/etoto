using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EToto.Application.Dto;
using EToto.Application.Interfaces;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;

namespace EToto.Infrastructure.Storage;

public class EquipmentExcelParser : IEquipamentoExcelParser
{
    private readonly ILogger<EquipmentExcelParser> _logger;

    // Constantes do template — definidas uma vez para serem reutilizadas
    // na validação e no parse.
    private const int StartRow = 7;
    private const int ColNumero = 2;               // B - "N°"
    private const int ColTipoEnergia = 3;          // C - "Tipo de energia"
    private const int ColDescricao = 4;            // D - "Descrição da energia perigosa"
    private const int ColFoto = 5;                 // E - Coluna de foto
    private const int ColIsoTag = 9;               // I - TAG dispositivo de isolamento
    private const int ColIsoLocation = 10;         // J - Localização dispositivo
    private const int ColIsoDescription = 11;      // K - Dispositivo de isolamento
    private const int ColLockout = 12;             // L - Bloqueio
    private const int ColZeroEnergyVerification = 13; // M - Verificação Energia Zero
    private const int ColTest = 14;                // N - Teste
    private const int ColMinFloat = 4;             // coluna mínima para imagens flutuantes (E, 0-based)
    private const int ColMaxFloat = 7;             // coluna máxima para imagens flutuantes (H, 0-based)
    private const int MinRequiredColumns = 14;     // A até N

    public EquipmentExcelParser(ILogger<EquipmentExcelParser> logger)
    {
        _logger = logger;
    }

    // =========================================================================
    // VALIDAÇÃO DO TEMPLATE
    // =========================================================================

    /// <summary>
    /// Valida se o arquivo Excel respeita o template esperado.
    /// Deve ser chamado antes de <see cref="ParseAsync"/>.
    /// </summary>
    public TemplateValidationResult ValidateTemplate(Stream excelStream)
    {
        if (excelStream == null)
            return TemplateValidationResult.Failure(new List<string> { "Stream do arquivo é nulo." });

        if (!excelStream.CanRead)
            return TemplateValidationResult.Failure(new List<string> { "O stream não está disponível para leitura." });

        try
        {
            ExcelPackage.License.SetNonCommercialPersonal("Leandro Vaz");

            excelStream.Position = 0;
            using var package = new ExcelPackage(excelStream);

            // 1) Verificar que existe pelo menos uma worksheet
            if (package.Workbook.Worksheets.Count == 0)
                return TemplateValidationResult.Failure(
                    new List<string> { "O arquivo não contém nenhuma aba (worksheet)." });

            var ws = package.Workbook.Worksheets[0];
            return ValidateWorksheet(ws);
        }
        catch (Exception ex)
        {
            // *** NÃO mascara o erro — loga com detalhe e devolve mensagem com causa real ***
            _logger.LogError(ex, "Erro ao validar template do Excel");

            return TemplateValidationResult.Failure(new List<string>
            {
                $"Erro ao validar template do Excel: {ex.Message}",
            });
        }
    }

    /// <summary>
    /// Valida a worksheet propriamente dita.
    /// </summary>
    private TemplateValidationResult ValidateWorksheet(ExcelWorksheet ws)
    {
        var errors = new List<string>();

        // 2) Cabeçalho obrigatório — D3 (Tag) e F3 (Nome do equipamento)
        var tag = ws.Cells["D3"].GetValue<string>();
        if (string.IsNullOrWhiteSpace(tag))
            errors.Add("Célula D3 (Tag do equipamento) está vazia. Verifique o cabeçalho do template.");

        var equipmentName = ws.Cells["F3"].GetValue<string>();
        if (string.IsNullOrWhiteSpace(equipmentName))
            errors.Add("Célula F3 (Nome do equipamento) está vazia. Verifique o cabeçalho do template.");

        // 3) Dimensões mínimas — o arquivo deve ter pelo menos 14 colunas (A-N)
        var lastCol = ws.Dimension?.End.Column ?? 0;
        if (lastCol < MinRequiredColumns)
        {
            errors.Add(
                $"O template deve conter pelo menos {MinRequiredColumns} colunas (A–N). " +
                $"O arquivo apresentado tem apenas {lastCol} coluna(s).");
        }

        // 4) Verificar se existe pelo menos 1 linha de dados válida a partir da linha StartRow
        var lastRow = ws.Dimension?.End.Row ?? 0;
        if (lastRow < StartRow)
        {
            errors.Add($"O arquivo não possui linhas de dados a partir da linha {StartRow}.");
        }
        else
        {
            var primeiraLinhaValida = EncontrarPrimeiraLinhaValida(ws, StartRow);

            if (primeiraLinhaValida < 0)
            {
                errors.Add(
                    $"Nenhuma linha de dados válida encontrada a partir da linha {StartRow}. " +
                    $"A coluna B deve conter um número inteiro em pelo menos uma linha.");
            }
            else
            {
                // 5) O número na primeira linha válida deve ser um inteiro positivo
                var num = ws.Cells[primeiraLinhaValida, ColNumero].GetValue<int?>();
                if (!num.HasValue || num.Value <= 0)
                {
                    errors.Add(
                        $"Linha {primeiraLinhaValida}: coluna B (N°) deve conter um número inteiro positivo. " +
                        $"Valor encontrado: '{ws.Cells[primeiraLinhaValida, ColNumero].GetValue<string>()}'.");
                }
            }
        }

        if (errors.Count > 0)
        {
            _logger.LogWarning("Template inválido — {Count} erro(s): {Erros}", errors.Count, string.Join(" | ", errors));
            return TemplateValidationResult.Failure(errors);
        }

        _logger.LogInformation("Template validado com sucesso: aba '{Sheet}', Tag='{Tag}', Equipamento='{Equipment}'",
            ws.Name, tag, equipmentName?.Trim());

        return TemplateValidationResult.Success();
    }

    /// <summary>
    /// Encontra a primeira linha numérica válida a partir de <paramref name="fromRow"/>,
    /// respeitando a mesma lógica de linhas de comentário usada no parse.
    /// Devolve -1 se não encontrar nenhuma.
    /// </summary>
    private int EncontrarPrimeiraLinhaValida(ExcelWorksheet ws, int fromRow)
    {
        var lastRow = ws.Dimension?.End.Row ?? 0;
        var maxCheck = Math.Min(lastRow, fromRow + 50);

        for (var row = fromRow; row <= maxCheck; row++)
        {
            if (IsCommentRow(ws, row)) continue;
            if (ws.Cells[row, ColNumero].GetValue<int?>().HasValue)
                return row;
        }

        return -1;
    }

    // =========================================================================
    // PARSE
    // =========================================================================

    public async Task<ParsedEquipamentoFile> ParseAsync(Stream excelStream, CancellationToken ct = default)
    {
        if (excelStream == null)
            throw new ArgumentNullException(nameof(excelStream));

        if (!excelStream.CanRead)
            throw new ArgumentException("Stream não está aberto para leitura.", nameof(excelStream));

        ExcelPackage.License.SetNonCommercialPersonal("Leandro Vaz");

        excelStream.Position = 0;
        using var package = new ExcelPackage(excelStream);
        var ws = package.Workbook.Worksheets[0];

        _logger.LogInformation("Iniciando parse do arquivo Excel: {SheetName}", ws.Name);

        // Cabeçalho do equipamento
        var result = new ParsedEquipamentoFile
        {
            Tag = ws.Cells["D3"].GetValue<string>() ?? string.Empty,
            EquipmentName = (ws.Cells["F3"].GetValue<string>() ?? string.Empty).Trim(),
            FactoryName = (ws.Cells["J3"].GetValue<string>() ?? string.Empty).Trim(),
            RevisionInfo = (ws.Cells["K3"].GetValue<string>() ?? string.Empty).Trim(),
            Rows = new List<ParsedEquipamentoRow>()
        };

        _logger.LogInformation("Cabeçalho: Tag={Tag}, Equipment={Equipment}", result.Tag, result.EquipmentName);

        // Fotos flutuantes
        var floatPictures = ws.Drawings
            .OfType<ExcelPicture>()
            .Where(p => p.From != null && p.From.Column >= ColMinFloat && p.From.Column <= ColMaxFloat)
            .OrderBy(p => p.From.Row)
            .ToList();

        _logger.LogInformation("Encontradas {Count} imagens flutuantes", floatPictures.Count);

        // Shapes com texto
        var shapes = ws.Drawings
            .OfType<ExcelShape>()
            .Where(s => s.From != null && s.From.Column >= ColMinFloat && s.From.Column <= ColMaxFloat)
            .OrderBy(s => s.From.Row)
            .ToList();

        _logger.LogInformation("Encontrados {Count} shapes com texto", shapes.Count);

        var currentRow = StartRow;
        var maxEmptyRows = 10;
        var emptyRowCount = 0;

        while (emptyRowCount < maxEmptyRows)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                if (IsCommentRow(ws, currentRow))
                {
                    _logger.LogDebug("Linha {Row} é um comentário, ignorando", currentRow);
                    currentRow++;
                    continue;
                }

                var lineNumber = ws.Cells[currentRow, ColNumero].GetValue<int?>();

                if (!lineNumber.HasValue)
                {
                    emptyRowCount++;
                    currentRow++;
                    continue;
                }

                emptyRowCount = 0;

                _logger.LogDebug("Processando linha {Row} (N° {LineNumber})", currentRow, lineNumber.Value);

                var energyType = ws.Cells[currentRow, ColTipoEnergia].GetValue<string>() ?? string.Empty;
                var hazardDescription = ws.Cells[currentRow, ColDescricao].GetValue<string>() ?? string.Empty;
                var isoTag = ws.Cells[currentRow, ColIsoTag].GetValue<string>() ?? string.Empty;
                var isoLocation = ws.Cells[currentRow, ColIsoLocation].GetValue<string>() ?? string.Empty;
                var isoDescription = ws.Cells[currentRow, ColIsoDescription].GetValue<string>() ?? string.Empty;
                var lockout = ws.Cells[currentRow, ColLockout].GetValue<string>() ?? string.Empty;
                var zeroEnergyVerification = ws.Cells[currentRow, ColZeroEnergyVerification].GetValue<string>() ?? string.Empty;
                var test = ws.Cells[currentRow, ColTest].GetValue<string>() ?? string.Empty;

                byte[]? finalImageBytes = null;
                string? shapeNotes = null;
                var imagensLinha = new List<byte[]>();

                // 1) Imagem in-cell
                try
                {
                    var fotoCell = ws.Cells[currentRow, ColFoto];
                    if (fotoCell.Picture != null && fotoCell.Picture.Exists)
                    {
                        var cellPic = fotoCell.Picture.Get();
                        if (cellPic != null)
                        {
                            var bytes = cellPic.GetImageBytes();
                            if (bytes != null && bytes.Length > 0)
                            {
                                imagensLinha.Add(bytes);
                                _logger.LogDebug("Imagem in-cell extraída da linha {Row}, tamanho: {Size} bytes", currentRow, bytes.Length);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Erro ao extrair imagem in-cell da linha {Row}", currentRow);
                }

                var rowZeroBased = currentRow - 1;

                // 2) Shape / balão de nota
                var shape = shapes.FirstOrDefault(s =>
                    s.From != null &&
                    s.From.Row <= rowZeroBased &&
                    (s.To?.Row ?? s.From.Row) >= rowZeroBased);

                if (shape != null)
                {
                    if (!string.IsNullOrWhiteSpace(shape.Text))
                    {
                        shapeNotes = shape.Text.Trim();
                        _logger.LogDebug("Shape text extraído da linha {Row}: {Text}", currentRow,
                            shapeNotes.Substring(0, Math.Min(50, shapeNotes.Length)));
                    }
                    else if (shape.RichText != null && shape.RichText.Count > 0)
                    {
                        shapeNotes = string.Concat(shape.RichText.Select(rt => rt.Text)).Trim();
                        _logger.LogDebug("Shape rich text extraído da linha {Row}: {Text}", currentRow,
                            shapeNotes.Substring(0, Math.Min(50, shapeNotes.Length)));
                    }
                }

                // 3) Fotos flutuantes que cobrem essa linha
                var picsForRow = floatPictures
                    .Where(p => p.From != null &&
                                p.From.Row <= rowZeroBased &&
                                (p.To?.Row ?? p.From.Row) >= rowZeroBased)
                    .ToList();

                foreach (var p in picsForRow)
                {
                    if (p.Image?.ImageBytes is { Length: > 0 } bytes)
                    {
                        imagensLinha.Add(bytes);
                        _logger.LogDebug("Imagem flutuante extraída da linha {Row}, tamanho: {Size} bytes", currentRow, bytes.Length);
                    }
                    floatPictures.Remove(p);
                }

                // Fallback
                if (imagensLinha.Count == 0 && floatPictures.Count > 0)
                {
                    var floatPic = floatPictures.First();
                    if (floatPic.Image?.ImageBytes is { Length: > 0 } bytes)
                    {
                        imagensLinha.Add(bytes);
                        _logger.LogDebug("Imagem flutuante (fallback) extraída para linha {Row}, tamanho: {Size} bytes", currentRow, bytes.Length);
                    }
                    floatPictures.Remove(floatPic);
                }

                // 4) Decide imagem final
                if (imagensLinha.Count == 1)
                {
                    finalImageBytes = imagensLinha[0];
                    _logger.LogInformation("1 imagem extraída da linha {Row}", currentRow);
                }
                else if (imagensLinha.Count > 1)
                {
                    finalImageBytes = MergeImagesVertical(imagensLinha);
                    _logger.LogInformation("{Count} imagens mescladas da linha {Row}", imagensLinha.Count, currentRow);
                }

                // 5) Adiciona linha ao resultado
                result.Rows.Add(new ParsedEquipamentoRow
                {
                    LineNumber = lineNumber.Value,
                    EnergyType = energyType,
                    HazardDescription = hazardDescription,
                    IsolationDeviceTag = string.IsNullOrWhiteSpace(isoTag) ? null : isoTag,
                    IsolationDeviceLocation = string.IsNullOrWhiteSpace(isoLocation) ? null : isoLocation,
                    IsolationDeviceDescription = string.IsNullOrWhiteSpace(isoDescription) ? null : isoDescription,
                    LockoutType = string.IsNullOrWhiteSpace(lockout) ? null : lockout,
                    ZeroEnergyVerification = string.IsNullOrWhiteSpace(zeroEnergyVerification) ? null : zeroEnergyVerification,
                    Test = string.IsNullOrWhiteSpace(test) ? null : test,
                    ImageBytes = finalImageBytes,
                    ShapeNotes = shapeNotes
                });

                currentRow++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar linha {Row}", currentRow);
                currentRow++;
            }
        }

        _logger.LogInformation("Parse concluído. Total de {Count} linha(s) válida(s) parseadas", result.Rows.Count);

        return await Task.FromResult(result);
    }

    // =========================================================================
    // MÉTODOS AUXILIARES
    // =========================================================================

    /// <summary>
    /// Verifica se a linha é um comentário (célula mesclada que vai de A até várias colunas).
    /// </summary>
    private bool IsCommentRow(ExcelWorksheet ws, int row)
    {
        try
        {
            var cellA = ws.Cells[row, 1];

            if (!cellA.Merge)
                return false;

            var mergedAddress = ws.MergedCells
                .FirstOrDefault(m => m.Contains($"A{row}"));

            if (string.IsNullOrEmpty(mergedAddress))
                return false;

            if (mergedAddress.StartsWith($"A{row}:") &&
                (mergedAddress.Contains("XFD") ||
                 mergedAddress.Contains($":{(char)('Z')}{row}") ||
                 mergedAddress.Contains($":{(char)('Y')}{row}") ||
                 mergedAddress.Contains($":{(char)('X')}{row}")))
            {
                var cellValue = cellA.GetValue<string>();
                _logger.LogDebug("Linha de comentário detectada: {Row} = '{Value}'", row, cellValue);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Erro ao verificar se linha {Row} é comentário", row);
            return false;
        }
    }

    /// <summary>
    /// Mescla várias imagens verticalmente, normalizando a largura.
    /// </summary>
    private byte[] MergeImagesVertical(IReadOnlyList<byte[]> imagensBytes)
    {
        if (imagensBytes == null || imagensBytes.Count == 0)
            return Array.Empty<byte>();

        const int padding = 10;

        var originalStreams = new List<MemoryStream>();
        var originalImages = new List<Image>();
        var resizedImages = new List<Image>();

        try
        {
            foreach (var bytes in imagensBytes)
            {
                if (bytes == null || bytes.Length == 0) continue;
                var ms = new MemoryStream(bytes);
                originalStreams.Add(ms);
                originalImages.Add(Image.FromStream(ms));
            }

            if (originalImages.Count == 0)
                return Array.Empty<byte>();

            var maxOrigWidth = originalImages.Max(i => i.Width);
            var targetWidth = Math.Min(maxOrigWidth, 1024);

            foreach (var img in originalImages)
            {
                if (img.Width == targetWidth)
                {
                    resizedImages.Add((Image)img.Clone());
                }
                else
                {
                    var newHeight = (int)Math.Round(img.Height * (targetWidth / (double)img.Width));
                    var bmp = new Bitmap(targetWidth, newHeight);
                    using (var g = Graphics.FromImage(bmp))
                    {
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.DrawImage(img, 0, 0, targetWidth, newHeight);
                    }
                    resizedImages.Add(bmp);
                }
            }

            var totalHeight = padding * (resizedImages.Count + 1) + resizedImages.Sum(i => i.Height);
            var finalWidth = targetWidth + padding * 2;

            using var merged = new Bitmap(finalWidth, totalHeight);
            using (var g = Graphics.FromImage(merged))
            {
                g.Clear(Color.White);
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                var y = padding;
                foreach (var img in resizedImages)
                {
                    g.DrawImage(img, padding, y, img.Width, img.Height);
                    y += img.Height + padding;
                }
            }

            using var outStream = new MemoryStream();
            merged.Save(outStream, ImageFormat.Png);
            return outStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao mesclar imagens verticalmente");
            return imagensBytes.FirstOrDefault() ?? Array.Empty<byte>();
        }
        finally
        {
            foreach (var img in originalImages) img?.Dispose();
            foreach (var img in resizedImages) img?.Dispose();
            foreach (var ms in originalStreams) ms?.Dispose();
        }
    }
}