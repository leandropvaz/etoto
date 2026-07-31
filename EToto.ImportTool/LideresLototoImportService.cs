using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using EToto.Domain.Entities;
using EToto.Domain.Enums;
using EToto.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;

namespace EToto.ImportTool;

// Implementa o formato específico da planilha "LÍDERES LOTOTO Versão Final". Ver
// ILideresLototoImportService para a especificação completa do mapeamento.
public class LideresLototoImportService : ILideresLototoImportService
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IPlantaRepository _plantas;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<LideresLototoImportService> _logger;

    private const int LinhaHeader = 2;
    private const int LinhaPrimeiroDado = 3;

    private const int ColNome = 1;
    private const int ColEmailOuEmpresa = 3;
    private const int ColDataTreinamento = 4;
    private const int ColPerfil = 5;
    private const int ColSenha = 6; // Nova coluna F na "Versão Final (1)".

    public LideresLototoImportService(
        IUsuarioRepository usuarios,
        IPlantaRepository plantas,
        IUnitOfWork uow,
        ILogger<LideresLototoImportService> logger)
    {
        _usuarios = usuarios;
        _plantas = plantas;
        _uow = uow;
        _logger = logger;
        ExcelPackage.License.SetNonCommercialPersonal("EToto");
    }

    public async Task<LideresLototoImportResult> ImportFromXlsxAsync(
        string xlsxPath, bool dryRun, int? criadoPorId = null, CancellationToken ct = default)
    {
        if (!File.Exists(xlsxPath))
            throw new FileNotFoundException("Planilha não encontrada.", xlsxPath);

        var result = new LideresLototoImportResult { DryRun = dryRun };
        using var package = new ExcelPackage(new FileInfo(xlsxPath));

        var loginsVistos = new Dictionary<string, OrigemLinha>(StringComparer.OrdinalIgnoreCase);

        foreach (var ws in package.Workbook.Worksheets)
        {
            ct.ThrowIfCancellationRequested();

            var (codigoPlanta, tipoVinculo) = InterpretarAba(ws.Name);
            if (string.IsNullOrWhiteSpace(codigoPlanta))
            {
                result.Avisos.Add($"Aba '{ws.Name}' ignorada — nome não reconhecido.");
                continue;
            }

            var planta = await _plantas.ObterPorCodigoAsync(codigoPlanta);
            if (planta is null)
            {
                result.Erros.Add($"Aba '{ws.Name}': planta com código '{codigoPlanta}' não encontrada.");
                continue;
            }

            var lastRow = ws.Dimension?.End.Row ?? 0;
            for (int row = LinhaPrimeiroDado; row <= lastRow; row++)
            {
                ct.ThrowIfCancellationRequested();

                var nome = ws.Cells[row, ColNome].GetValue<string>()?.Trim();
                if (string.IsNullOrWhiteSpace(nome)) continue;

                result.LinhasLidas++;

                var emailOuEmpresa = ws.Cells[row, ColEmailOuEmpresa].GetValue<string>()?.Trim();
                var login = DerivarLogin(tipoVinculo, nome, emailOuEmpresa);
                if (string.IsNullOrWhiteSpace(login))
                {
                    result.LinhasInvalidas++;
                    result.Erros.Add($"Aba '{ws.Name}' linha {row} ({nome}): não foi possível derivar Login.");
                    continue;
                }

                if (loginsVistos.TryGetValue(login, out var primeiro))
                {
                    result.DuplicidadesEntreAbas++;
                    result.Avisos.Add(
                        $"Aba '{ws.Name}' linha {row} ({nome}): login derivado '{login}' colide com aba '{primeiro.Aba}' linha {primeiro.Linha} ({primeiro.Nome}).");
                    continue;
                }
                loginsVistos[login] = new OrigemLinha(ws.Name, row, nome);

                var dataTreinamento = LerDataExcel(ws, row, ColDataTreinamento);
                var perfilTexto = ws.Cells[row, ColPerfil].GetValue<string>()?.Trim();
                var perfis = MapearPerfis(perfilTexto);
                var senhaPlain = ws.Cells[row, ColSenha].GetValue<string>()?.Trim();
                var senhaHash = string.IsNullOrWhiteSpace(senhaPlain) ? "" : GerarHash(senhaPlain);

                // Validade do TREINAMENTO: 2 anos a partir da data do treinamento
                // (mesma regra para Funcionário e Parceiro/Terceiro).
                DateTime? validadeTreinamento = dataTreinamento?.AddYears(2);

                string? nomeEmpresa = tipoVinculo == TipoVinculo.Terceiro ? emailOuEmpresa : null;

                // Regra: Terceiro exige NomeEmpresa. A validade de ACESSO é derivada da data de
                // criação (6 meses), portanto não depende mais da data de treinamento (col D).
                if (tipoVinculo == TipoVinculo.Terceiro && string.IsNullOrWhiteSpace(nomeEmpresa))
                {
                    result.LinhasInvalidas++;
                    result.Erros.Add($"Aba '{ws.Name}' linha {row} ({nome}): Terceiro precisa de NomeEmpresa (col C).");
                    continue;
                }

                var existente = await _usuarios.ObterPorLoginAsync(login);
                if (existente is null)
                {
                    var agora = DateTime.UtcNow;
                    // Terceiro: acesso vale 6 meses a partir da data de criação. Funcionário: sem validade de acesso.
                    DateTime? validadeAcesso = tipoVinculo == TipoVinculo.Terceiro
                        ? agora.Date.AddMonths(6)
                        : null;

                    if (!dryRun)
                    {
                        var novo = new Usuarios
                        {
                            Login = login,
                            NomeCompleto = nome,
                            SenhaHash = senhaHash, // se col F preenchida vem hashada; senao "", o operador define no primeiro acesso.
                            Perfil = perfis[0],
                            Ativa = true,
                            PlantaId = planta.Id,
                            TipoVinculo = tipoVinculo,
                            NomeEmpresa = nomeEmpresa,
                            DataValidadeAcesso = validadeAcesso,
                            TreinamentoConcluido = dataTreinamento.HasValue,
                            DataTreinamento = dataTreinamento,
                            DataValidadeTreinamento = validadeTreinamento,
                            DataCriacao = agora,
                            CriadoEm = agora,
                            CriadoPorId = criadoPorId
                        };
                        novo.DefinirPerfis(perfis);
                        novo.PlantasAssociadas.Add(new UsuarioPlanta
                        {
                            PlantaId = planta.Id,
                            DataAssociacao = DateTime.UtcNow
                        });
                        await _usuarios.AddAsync(novo);
                    }
                    result.UsuariosCriados++;
                }
                else
                {
                    // Terceiro: acesso vale 6 meses a partir da data de criação ORIGINAL do usuário
                    // (mantém idempotência: re-rodar a importação não empurra a validade para frente).
                    DateTime? validadeAcesso = tipoVinculo == TipoVinculo.Terceiro
                        ? existente.DataCriacao.Date.AddMonths(6)
                        : null;

                    var perfisAtuais = existente.Perfis.Select(p => p.Perfil).OrderBy(p => p).ToList();
                    var perfisNovos = perfis.OrderBy(p => p).ToList();
                    // Senha: so consideramos mudanca se a planilha trouxe valor e o hash difere.
                    var senhaMudou = !string.IsNullOrWhiteSpace(senhaPlain) && existente.SenhaHash != senhaHash;
                    var mudou =
                        existente.NomeCompleto != nome
                        || existente.PlantaId != planta.Id
                        || existente.TipoVinculo != tipoVinculo
                        || existente.NomeEmpresa != nomeEmpresa
                        || existente.DataValidadeAcesso != validadeAcesso
                        || existente.DataTreinamento != dataTreinamento
                        || existente.DataValidadeTreinamento != validadeTreinamento
                        || existente.TreinamentoConcluido != dataTreinamento.HasValue
                        || senhaMudou
                        || !perfisAtuais.SequenceEqual(perfisNovos);

                    if (!mudou)
                    {
                        result.UsuariosSemAlteracao++;
                        continue;
                    }

                    if (!dryRun)
                    {
                        existente.NomeCompleto = nome;
                        existente.PlantaId = planta.Id;
                        existente.TipoVinculo = tipoVinculo;
                        existente.NomeEmpresa = nomeEmpresa;
                        existente.DataValidadeAcesso = validadeAcesso;
                        existente.TreinamentoConcluido = dataTreinamento.HasValue;
                        existente.DataTreinamento = dataTreinamento;
                        existente.DataValidadeTreinamento = validadeTreinamento;
                        existente.DataAtualizacao = DateTime.UtcNow;
                        existente.AlteradoEm = DateTime.UtcNow;
                        existente.AlteradoPorId = criadoPorId;
                        if (senhaMudou) existente.SenhaHash = senhaHash;
                        existente.DefinirPerfis(perfis);
                        _usuarios.Update(existente);
                    }
                    result.UsuariosAtualizados++;
                }
            }

            if (!dryRun)
                await _uow.CommitAsync(ct);
        }

        return result;
    }

    // Aba "Parceiros XYZ" → planta XYZ, Terceiro. Demais → planta = nome da aba, Funcionário.
    private static (string codigo, TipoVinculo tipo) InterpretarAba(string nomeAba)
    {
        if (string.IsNullOrWhiteSpace(nomeAba)) return ("", TipoVinculo.Funcionario);

        var nome = nomeAba.Trim();
        if (nome.StartsWith("Parceiros", StringComparison.OrdinalIgnoreCase))
        {
            var partes = nome.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (partes.Length < 2) return ("", TipoVinculo.Terceiro);
            return (partes[^1].Trim(), TipoVinculo.Terceiro);
        }

        return (nome, TipoVinculo.Funcionario);
    }

    // Tracking de origem para mensagens de colisao precisas.
    private sealed record OrigemLinha(string Aba, int Linha, string Nome);

    // Sentinels comuns para celulas de email que na verdade significam "nao tem email".
    // O texto vem direto da planilha — tratamos como ausente e caimos no fallback por nome.
    private static bool EhEmailValido(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return false;
        var lower = RemoverAcentos(s).Trim().ToLowerInvariant();
        if (lower.Contains("nao possui")) return false;
        if (lower.Contains("nao tem")) return false;
        if (lower.Contains("sem email")) return false;
        if (lower.Contains("sem e-mail")) return false;
        if (lower == "n/a" || lower == "na" || lower == "-" || lower == "--") return false;
        // Email valido precisa ter '@'.
        return s.Contains('@');
    }

    private static string DerivarLogin(TipoVinculo tipo, string nomeCompleto, string? emailOuEmpresa)
    {
        // Funcionário com email valido: parte antes do '@'.
        if (tipo == TipoVinculo.Funcionario && EhEmailValido(emailOuEmpresa ?? ""))
        {
            var at = emailOuEmpresa!.IndexOf('@');
            var parte = emailOuEmpresa[..at].Trim();
            if (!string.IsNullOrWhiteSpace(parte))
                return parte.ToLowerInvariant();
        }

        // Fallback (Terceiro OU Funcionário sem email): primeiro nome + segundo nome + último sobrenome.
        // Sem acentos, ignorando partículas DE/DA/DO/DAS/DOS/E. Reduz colisões em relacao a "primeiro.ultimo".
        var tokens = RemoverAcentos(nomeCompleto)
            .Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(t => !PalavrasIgnoradas.Contains(t, StringComparer.OrdinalIgnoreCase))
            .ToArray();
        if (tokens.Length == 0) return "";
        if (tokens.Length == 1) return tokens[0].ToLowerInvariant();
        if (tokens.Length == 2) return $"{tokens[0]}.{tokens[1]}".ToLowerInvariant();
        // 3+ tokens: primeiro + segundo + último (3 partes para diminuir colisao).
        return $"{tokens[0]}.{tokens[1]}.{tokens[^1]}".ToLowerInvariant();
    }

    private static readonly string[] PalavrasIgnoradas =
        { "DE", "DA", "DO", "DAS", "DOS", "E" };

    private static string RemoverAcentos(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        var normalizado = s.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalizado.Length);
        foreach (var c in normalizado)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    private static DateTime? LerDataExcel(ExcelWorksheet ws, int row, int col)
    {
        var cell = ws.Cells[row, col];
        if (cell.Value is null) return null;
        if (cell.Value is DateTime dt)
            return DateTime.SpecifyKind(dt.Date, DateTimeKind.Utc);
        if (cell.Value is double d)
            return DateTime.SpecifyKind(DateTime.FromOADate(d).Date, DateTimeKind.Utc);

        var s = cell.GetValue<string>()?.Trim();
        if (string.IsNullOrWhiteSpace(s)) return null;

        // Serial numérico como string
        if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var serial))
            return DateTime.SpecifyKind(DateTime.FromOADate(serial).Date, DateTimeKind.Utc);

        // Strings de data soltas
        string[] fmt = { "yyyy-MM-dd", "dd/MM/yyyy", "yyyy/MM/dd", "MM/dd/yyyy" };
        if (DateTime.TryParseExact(s, fmt, CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed))
            return DateTime.SpecifyKind(parsed.Date, DateTimeKind.Utc);

        return null;
    }

    // SHA-256 hex lowercase — mesma logica do AuthService.GerarHash.
    // Mantenha em sincronia se um dia a regra mudar (BCrypt etc.).
    private static string GerarHash(string senha)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(senha));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static List<PerfilUsuario> MapearPerfis(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return new List<PerfilUsuario> { PerfilUsuario.UsuarioFinal };

        var t = texto.ToLowerInvariant();
        var perfis = new List<PerfilUsuario>();

        if (t.Contains("lider") || t.Contains("líder"))
            perfis.Add(PerfilUsuario.UsuarioFinal);
        if (t.Contains("comando central"))
            perfis.Add(PerfilUsuario.ComandoCentral);

        if (perfis.Count == 0)
            perfis.Add(PerfilUsuario.UsuarioFinal); // fallback seguro

        return perfis;
    }
}
