using EToto.Domain.Enums;
using EToto.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EToto.ImportTool;

// Lê um CSV com cabeçalho "Login;Perfis" e aplica DefinirPerfis em cada usuário existente.
// Idempotente: se a coleção atual de perfis já bater com a do CSV, nada é gravado.
// A regra de domínio "SuperGestor exclusivo" é validada por Usuarios.DefinirPerfis (#1a).
public class UsuarioPerfilImportService : IUsuarioPerfilImportService
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<UsuarioPerfilImportService> _logger;

    private static readonly char[] PerfilSeparadores = { ',', '|', '/' };

    public UsuarioPerfilImportService(
        IUsuarioRepository usuarios,
        IUnitOfWork uow,
        ILogger<UsuarioPerfilImportService> logger)
    {
        _usuarios = usuarios;
        _uow = uow;
        _logger = logger;
    }

    public async Task<UsuarioPerfilImportResult> ImportFromCsvAsync(string csvPath, CancellationToken ct = default)
    {
        var result = new UsuarioPerfilImportResult();

        if (!File.Exists(csvPath))
            throw new FileNotFoundException("CSV não encontrado.", csvPath);

        var lines = await File.ReadAllLinesAsync(csvPath, ct);
        if (lines.Length == 0)
        {
            _logger.LogWarning("CSV vazio: {Path}", csvPath);
            return result;
        }

        // Assume cabeçalho na linha 0.
        for (int i = 1; i < lines.Length; i++)
        {
            ct.ThrowIfCancellationRequested();
            result.TotalLinhas++;

            var raw = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(raw))
                continue;

            var partes = raw.Split(';', 2, StringSplitOptions.TrimEntries);
            if (partes.Length < 2 || string.IsNullOrWhiteSpace(partes[0]))
            {
                result.LinhasInvalidas++;
                result.Erros.Add($"Linha {i + 1}: formato inválido (esperado 'Login;Perfis').");
                continue;
            }

            var login = partes[0];
            var perfis = ParsePerfis(partes[1]);
            if (perfis.Count == 0)
            {
                result.LinhasInvalidas++;
                result.Erros.Add($"Linha {i + 1} ({login}): nenhum perfil válido.");
                continue;
            }

            var usuario = await _usuarios.ObterPorLoginAsync(login);
            if (usuario is null)
            {
                result.UsuariosNaoEncontrados++;
                result.Erros.Add($"Linha {i + 1}: usuário '{login}' não encontrado.");
                _logger.LogWarning("Usuário não encontrado: {Login}", login);
                continue;
            }

            var perfisAtuais = usuario.Perfis.Select(p => p.Perfil).OrderBy(p => p).ToList();
            var perfisNovos = perfis.OrderBy(p => p).ToList();
            if (perfisAtuais.SequenceEqual(perfisNovos))
            {
                _logger.LogInformation("Usuário {Login} já está com os perfis desejados — sem alteração.", login);
                continue;
            }

            try
            {
                usuario.DefinirPerfis(perfis);
                _usuarios.Update(usuario);
                await _uow.CommitAsync();
                result.UsuariosAtualizados++;
                _logger.LogInformation("Perfis atualizados para {Login}: {Perfis}", login, string.Join(",", perfis));
            }
            catch (InvalidOperationException ex)
            {
                result.LinhasInvalidas++;
                result.Erros.Add($"Linha {i + 1} ({login}): {ex.Message}");
                _logger.LogError(ex, "Falha de regra em {Login}", login);
            }
        }

        return result;
    }

    private static List<PerfilUsuario> ParsePerfis(string raw)
    {
        var tokens = raw.Split(PerfilSeparadores, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var perfis = new List<PerfilUsuario>();
        foreach (var token in tokens)
        {
            if (Enum.TryParse<PerfilUsuario>(token, ignoreCase: true, out var perfil))
            {
                if (!perfis.Contains(perfil))
                    perfis.Add(perfil);
            }
        }
        return perfis;
    }
}
