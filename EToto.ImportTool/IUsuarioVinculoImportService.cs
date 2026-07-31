namespace EToto.ImportTool;

public interface IUsuarioVinculoImportService
{
    /// <summary>
    /// Importa a relação inicial de vínculo Funcionário/Terceiro (#2) a partir de um CSV.
    /// Formato esperado: Login;TipoVinculo;NomeEmpresa;DataValidadeAcesso (YYYY-MM-DD).
    /// Terceiros exigem NomeEmpresa e DataValidadeAcesso. Funcionário ignora os dois últimos.
    /// Idempotente: não regrava se já estiver igual.
    /// </summary>
    Task<UsuarioVinculoImportResult> ImportFromCsvAsync(string csvPath, CancellationToken ct = default);
}

public class UsuarioVinculoImportResult
{
    public int TotalLinhas { get; set; }
    public int UsuariosAtualizados { get; set; }
    public int UsuariosNaoEncontrados { get; set; }
    public int LinhasInvalidas { get; set; }
    public List<string> Erros { get; set; } = new();
}
