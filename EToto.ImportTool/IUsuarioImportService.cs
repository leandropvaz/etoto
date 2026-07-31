namespace EToto.ImportTool;

public interface IUsuarioImportService
{
    /// <summary>
    /// Importa usuários a partir de planilha xlsx revisada (#7).
    ///
    /// Formato esperado:
    ///  • Cada ABA representa "PlantaCodigo - TipoVinculo" (ex.: "PLA-Funcionarios", "PLA-Terceiros").
    ///    O parser separa pelo último "-" (case-insensitive): à esquerda código da planta, à direita
    ///    palavra contendo "terceir" para Terceiro; senão Funcionário.
    ///  • Cabeçalho mínimo na primeira linha (case-insensitive):
    ///    Login | NomeCompleto | Perfil | NomeEmpresa | DataValidadeAcesso | DataValidadeTreinamento
    ///  • Datas em formato ISO (YYYY-MM-DD) ou BR (DD/MM/YYYY).
    ///  • Perfil: nome do enum (Usuario, Administrador, SuperGestor, UsuarioFinal, ComandoCentral).
    ///
    /// Comportamento:
    ///  • Detecta duplicidade do mesmo Login entre abas (mantém o primeiro, reporta os demais).
    ///  • Usuários sem e-mail (campo não existe na entidade) são aceitos normalmente — não bloqueia.
    ///  • Idempotente: re-importar a mesma planilha não muda nada (compara antes de gravar).
    ///  • Apenas dry-run (=true) por padrão na chamada CLI — produção exige confirmação explícita.
    /// </summary>
    Task<UsuarioImportResult> ImportFromXlsxAsync(string xlsxPath, bool dryRun, CancellationToken ct = default);
}

public class UsuarioImportResult
{
    public bool DryRun { get; set; }
    public int LinhasLidas { get; set; }
    public int UsuariosCriados { get; set; }
    public int UsuariosAtualizados { get; set; }
    public int UsuariosSemAlteracao { get; set; }
    public int LinhasInvalidas { get; set; }
    public int DuplicidadesEntreAbas { get; set; }
    public List<string> Erros { get; set; } = new();
    public List<string> Avisos { get; set; } = new();
}
