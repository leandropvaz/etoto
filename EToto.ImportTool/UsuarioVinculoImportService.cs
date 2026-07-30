using System.Globalization;
using EToto.Domain.Enums;
using EToto.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EToto.ImportTool;

// Importa vínculo Funcionário/Terceiro (#2). Idempotente: só grava quando o estado vai mudar.
// Aceita a data em ISO (YYYY-MM-DD) ou BR (DD/MM/YYYY).
public class UsuarioVinculoImportService : IUsuarioVinculoImportService
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<UsuarioVinculoImportService> _logger;

    private static readonly string[] FormatosData = { "yyyy-MM-dd", "dd/MM/yyyy", "yyyy/MM/dd" };

    public UsuarioVinculoImportService(
        IUsuarioRepository usuarios,
        IUnitOfWork uow,
        ILogger<UsuarioVinculoImportService> logger)
    {
        _usuarios = usuarios;
        _uow = uow;
        _logger = logger;
    }

    public async Task<UsuarioVinculoImportResult> ImportFromCsvAsync(string csvPath, CancellationToken ct = default)
    {
        var result = new UsuarioVinculoImportResult();

        if (!File.Exists(csvPath))
            throw new FileNotFoundException("CSV não encontrado.", csvPath);

        var lines = await File.ReadAllLinesAsync(csvPath, ct);
        if (lines.Length == 0)
            return result;

        for (int i = 1; i < lines.Length; i++)
        {
            ct.ThrowIfCancellationRequested();
            var raw = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(raw)) continue;
            result.TotalLinhas++;

            var partes = raw.Split(';', StringSplitOptions.TrimEntries);
            if (partes.Length < 2 || string.IsNullOrWhiteSpace(partes[0]))
            {
                result.LinhasInvalidas++;
                result.Erros.Add($"Linha {i + 1}: formato inválido (esperado 'Login;TipoVinculo;NomeEmpresa;DataValidadeAcesso').");
                continue;
            }

            var login = partes[0];
            if (!Enum.TryParse<TipoVinculo>(partes[1], ignoreCase: true, out var tipo))
            {
                result.LinhasInvalidas++;
                result.Erros.Add($"Linha {i + 1} ({login}): TipoVinculo '{partes[1]}' inválido (use Funcionario ou Terceiro).");
                continue;
            }

            string? empresa = partes.Length > 2 ? NullIfBlank(partes[2]) : null;
            DateTime? validade = null;
            if (partes.Length > 3 && !string.IsNullOrWhiteSpace(partes[3]))
            {
                if (DateTime.TryParseExact(partes[3], FormatosData, CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed))
                {
                    validade = parsed;
                }
                else
                {
                    result.LinhasInvalidas++;
                    result.Erros.Add($"Linha {i + 1} ({login}): data '{partes[3]}' inválida (use YYYY-MM-DD).");
                    continue;
                }
            }

            try
            {
                VinculoValidacao.ValidarTerceiro(tipo, empresa, validade);
            }
            catch (InvalidOperationException ex)
            {
                result.LinhasInvalidas++;
                result.Erros.Add($"Linha {i + 1} ({login}): {ex.Message}");
                continue;
            }

            var usuario = await _usuarios.ObterPorLoginAsync(login);
            if (usuario is null)
            {
                result.UsuariosNaoEncontrados++;
                result.Erros.Add($"Linha {i + 1}: usuário '{login}' não encontrado.");
                continue;
            }

            // Para Funcionario, ignora empresa e validade enviadas.
            var empresaAlvo = tipo == TipoVinculo.Terceiro ? empresa : null;
            var validadeAlvo = tipo == TipoVinculo.Terceiro ? validade : null;

            // Idempotência: nada para fazer se já está igual.
            if (usuario.TipoVinculo == tipo
                && string.Equals(usuario.NomeEmpresa, empresaAlvo, StringComparison.OrdinalIgnoreCase)
                && usuario.DataValidadeAcesso == validadeAlvo)
            {
                _logger.LogInformation("Usuário {Login} já está com o vínculo desejado.", login);
                continue;
            }

            usuario.TipoVinculo = tipo;
            usuario.NomeEmpresa = empresaAlvo;
            usuario.DataValidadeAcesso = validadeAlvo;

            _usuarios.Update(usuario);
            await _uow.CommitAsync(ct);
            result.UsuariosAtualizados++;
            _logger.LogInformation("Vínculo atualizado para {Login}: {Tipo}", login, tipo);
        }

        return result;
    }

    private static string? NullIfBlank(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
