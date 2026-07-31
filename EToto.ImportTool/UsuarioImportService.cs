using System.Globalization;
using EToto.Domain.Entities;
using EToto.Domain.Enums;
using EToto.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;

namespace EToto.ImportTool;

// #7: importa usuários da planilha revisada (uma aba por planta+vínculo). Idempotente.
// Sem suporte a e-mail (entidade não tem o campo); a presença/ausência dele na planilha é ignorada.
public class UsuarioImportService : IUsuarioImportService
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IPlantaRepository _plantas;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<UsuarioImportService> _logger;

    private static readonly string[] FormatosData = { "yyyy-MM-dd", "dd/MM/yyyy", "yyyy/MM/dd" };

    public UsuarioImportService(
        IUsuarioRepository usuarios,
        IPlantaRepository plantas,
        IUnitOfWork uow,
        ILogger<UsuarioImportService> logger)
    {
        _usuarios = usuarios;
        _plantas = plantas;
        _uow = uow;
        _logger = logger;
        ExcelPackage.License.SetNonCommercialPersonal("EToto");
    }

    public async Task<UsuarioImportResult> ImportFromXlsxAsync(
        string xlsxPath, bool dryRun, CancellationToken ct = default)
    {
        if (!File.Exists(xlsxPath))
            throw new FileNotFoundException("Planilha não encontrada.", xlsxPath);

        var result = new UsuarioImportResult { DryRun = dryRun };
        using var package = new ExcelPackage(new FileInfo(xlsxPath));

        // Cache de login → primeira aba em que apareceu (dedup global).
        var loginsVistos = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var ws in package.Workbook.Worksheets)
        {
            ct.ThrowIfCancellationRequested();

            if (!TryParseNomeAba(ws.Name, out var codigoPlanta, out var tipoVinculo))
            {
                result.Avisos.Add($"Aba '{ws.Name}' ignorada — formato esperado 'CodigoPlanta-Funcionarios|Terceiros'.");
                continue;
            }

            var planta = await _plantas.ObterPorCodigoAsync(codigoPlanta);
            if (planta is null)
            {
                result.Erros.Add($"Aba '{ws.Name}': planta com código '{codigoPlanta}' não encontrada.");
                continue;
            }

            var indicesColunas = MapearColunas(ws);
            if (indicesColunas is null)
            {
                result.Erros.Add($"Aba '{ws.Name}': cabeçalho inválido (faltam Login e/ou NomeCompleto).");
                continue;
            }

            var lastRow = ws.Dimension?.End.Row ?? 0;
            for (int row = 2; row <= lastRow; row++)
            {
                ct.ThrowIfCancellationRequested();

                var login = ws.Cells[row, indicesColunas.Login].GetValue<string>()?.Trim();
                if (string.IsNullOrWhiteSpace(login)) continue;

                result.LinhasLidas++;

                if (loginsVistos.TryGetValue(login, out var primeiraAba))
                {
                    result.DuplicidadesEntreAbas++;
                    result.Avisos.Add(
                        $"Login '{login}' duplicado — mantido em '{primeiraAba}', ignorado em '{ws.Name}'.");
                    continue;
                }
                loginsVistos[login] = ws.Name;

                var nome = ws.Cells[row, indicesColunas.Nome].GetValue<string>()?.Trim();
                if (string.IsNullOrWhiteSpace(nome))
                {
                    result.LinhasInvalidas++;
                    result.Erros.Add($"Aba '{ws.Name}' linha {row}: NomeCompleto vazio.");
                    continue;
                }

                var perfilNome = indicesColunas.Perfil.HasValue
                    ? ws.Cells[row, indicesColunas.Perfil.Value].GetValue<string>()?.Trim()
                    : null;

                if (string.IsNullOrWhiteSpace(perfilNome) ||
                    !Enum.TryParse<PerfilUsuario>(perfilNome, ignoreCase: true, out var perfil))
                {
                    perfil = PerfilUsuario.Usuario; // default seguro
                }

                var nomeEmpresa = indicesColunas.NomeEmpresa.HasValue
                    ? ws.Cells[row, indicesColunas.NomeEmpresa.Value].GetValue<string>()?.Trim()
                    : null;
                var dataValAcesso = LerData(ws, row, indicesColunas.DataValidadeAcesso);
                var dataValTreino = LerData(ws, row, indicesColunas.DataValidadeTreinamento);

                // Regra de Terceiro: empresa+validade obrigatórias.
                try
                {
                    VinculoValidacao.ValidarTerceiro(tipoVinculo, nomeEmpresa, dataValAcesso);
                }
                catch (InvalidOperationException ex)
                {
                    result.LinhasInvalidas++;
                    result.Erros.Add($"Aba '{ws.Name}' linha {row} ({login}): {ex.Message}");
                    continue;
                }

                var existente = await _usuarios.ObterPorLoginAsync(login);
                if (existente is null)
                {
                    if (!dryRun)
                    {
                        var novo = new Usuarios
                        {
                            Login = login,
                            NomeCompleto = nome,
                            SenhaHash = "", // operador define no primeiro acesso
                            Perfil = perfil,
                            Ativa = true,
                            PlantaId = planta.Id,
                            TipoVinculo = tipoVinculo,
                            NomeEmpresa = tipoVinculo == TipoVinculo.Terceiro ? nomeEmpresa : null,
                            DataValidadeAcesso = tipoVinculo == TipoVinculo.Terceiro ? dataValAcesso : null,
                            DataValidadeTreinamento = dataValTreino,
                            DataCriacao = DateTime.UtcNow,
                            CriadoEm = DateTime.UtcNow
                        };
                        novo.DefinirPerfis(new[] { perfil });
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
                    var perfisAtuais = existente.Perfis.Select(p => p.Perfil).OrderBy(p => p).ToList();
                    var mudou =
                        existente.NomeCompleto != nome
                        || existente.PlantaId != planta.Id
                        || existente.TipoVinculo != tipoVinculo
                        || existente.NomeEmpresa != (tipoVinculo == TipoVinculo.Terceiro ? nomeEmpresa : null)
                        || existente.DataValidadeAcesso != (tipoVinculo == TipoVinculo.Terceiro ? dataValAcesso : null)
                        || existente.DataValidadeTreinamento != dataValTreino
                        || !perfisAtuais.SequenceEqual(new[] { perfil });

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
                        existente.NomeEmpresa = tipoVinculo == TipoVinculo.Terceiro ? nomeEmpresa : null;
                        existente.DataValidadeAcesso = tipoVinculo == TipoVinculo.Terceiro ? dataValAcesso : null;
                        existente.DataValidadeTreinamento = dataValTreino;
                        existente.DataAtualizacao = DateTime.UtcNow;
                        existente.AlteradoEm = DateTime.UtcNow;
                        existente.DefinirPerfis(new[] { perfil });
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

    private static bool TryParseNomeAba(string aba, out string codigoPlanta, out TipoVinculo tipo)
    {
        codigoPlanta = ""; tipo = TipoVinculo.Funcionario;
        if (string.IsNullOrWhiteSpace(aba)) return false;

        var idx = aba.LastIndexOf('-');
        if (idx <= 0 || idx == aba.Length - 1) return false;

        codigoPlanta = aba[..idx].Trim();
        var sufixo = aba[(idx + 1)..].Trim();
        tipo = sufixo.Contains("terceir", StringComparison.OrdinalIgnoreCase)
            ? TipoVinculo.Terceiro
            : TipoVinculo.Funcionario;
        return !string.IsNullOrWhiteSpace(codigoPlanta);
    }

    private static DateTime? LerData(ExcelWorksheet ws, int row, int? col)
    {
        if (!col.HasValue) return null;
        var cell = ws.Cells[row, col.Value];

        // Já é DateTime numérica
        if (cell.Value is DateTime dt) return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        if (cell.Value is double d) return DateTime.FromOADate(d).ToUniversalTime();

        var s = cell.GetValue<string>()?.Trim();
        if (string.IsNullOrWhiteSpace(s)) return null;
        if (DateTime.TryParseExact(s, FormatosData, CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed))
            return parsed;
        return null;
    }

    private static IndicesColunas? MapearColunas(ExcelWorksheet ws)
    {
        var lastCol = ws.Dimension?.End.Column ?? 0;
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int c = 1; c <= lastCol; c++)
        {
            var nome = ws.Cells[1, c].GetValue<string>()?.Trim();
            if (!string.IsNullOrWhiteSpace(nome)) map[nome] = c;
        }

        if (!map.TryGetValue("Login", out var iLogin)) return null;
        if (!map.TryGetValue("NomeCompleto", out var iNome)) return null;

        return new IndicesColunas
        {
            Login = iLogin,
            Nome = iNome,
            Perfil = map.TryGetValue("Perfil", out var p) ? p : null,
            NomeEmpresa = map.TryGetValue("NomeEmpresa", out var e) ? e : null,
            DataValidadeAcesso = map.TryGetValue("DataValidadeAcesso", out var dv) ? dv : null,
            DataValidadeTreinamento = map.TryGetValue("DataValidadeTreinamento", out var dt) ? dt : null
        };
    }

    private sealed class IndicesColunas
    {
        public int Login { get; set; }
        public int Nome { get; set; }
        public int? Perfil { get; set; }
        public int? NomeEmpresa { get; set; }
        public int? DataValidadeAcesso { get; set; }
        public int? DataValidadeTreinamento { get; set; }
    }
}
