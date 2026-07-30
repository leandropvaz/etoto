namespace EToto.ImportTool;

public interface IUsuarioPerfilImportService
{
    /// <summary>
    /// Importa a relação inicial Usuário↔Perfis a partir de um CSV (Login;Perfis), idempotente.
    /// Cada linha tem o login e uma lista de perfis separados por vírgula
    /// (ex.: "joao.silva;Administrador,UsuarioFinal").
    /// </summary>
    Task<UsuarioPerfilImportResult> ImportFromCsvAsync(string csvPath, CancellationToken ct = default);
}

public class UsuarioPerfilImportResult
{
    public int TotalLinhas { get; set; }
    public int UsuariosAtualizados { get; set; }
    public int UsuariosNaoEncontrados { get; set; }
    public int LinhasInvalidas { get; set; }
    public List<string> Erros { get; set; } = new();
}
