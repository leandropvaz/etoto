namespace EToto.ImportTool;

// #7 — variante específica para a planilha "LÍDERES LOTOTO Versão Final".
//
// Formato esperado:
//  • Header na LINHA 2 (linha 1 traz o título "LÍDERES LOTOTO" merged).
//  • Cabeçalho fixo:
//      A=NOME DO COLABORADOR | B=ÁREA | C=EMAIL (funcionários) ou Empresa (parceiros)
//      D=DATA DO TREINAMENTO (serial Excel) | E=Perfil de acesso (texto livre)
//  • Abas:
//      - FARC, FPIT, FMTZ, FSET, FCTG  → planta de mesmo código, vínculo Funcionário.
//      - "Parceiros <CODIGO>" → planta <CODIGO>, vínculo Terceiro (NomeEmpresa = col C).
//  • Perfil:
//      - "Lider"                        → UsuarioFinal
//      - "Comando Central e Lider"      → UsuarioFinal + ComandoCentral
//      - demais variações são tratadas como UsuarioFinal por segurança.
//  • Login derivado:
//      - Funcionário: parte antes do "@" do email.
//      - Parceiro:   primeiro nome + último sobrenome, ambos em minúsculas e sem acentos
//                    (ex.: "ALEXANDRO MARTINS DA CONCEICAO" → "alexandro.conceicao").
//  • DataTreinamento: célula serial Excel convertida para DateTime UTC.
//  • DataValidadeTreinamento = DataTreinamento + 2 anos (Funcionário e Parceiro/Terceiro).
//  • DataValidadeAcesso:
//      - Funcionário: nenhuma (sem validade de acesso).
//      - Terceiro:    DataCriacao + 6 meses (independe da data de treinamento).
//
// Comportamento:
//  • Detecta duplicidade do mesmo Login entre abas (mantém o primeiro).
//  • Idempotente (compara antes de gravar).
//  • Modo DRY-RUN simula sem persistir.
public interface ILideresLototoImportService
{
    /// <param name="criadoPorId">
    /// Id do operador rodando o import. Quando informado, vai para CriadoPorId
    /// em usuarios novos e AlteradoPorId em usuarios atualizados.
    /// </param>
    Task<LideresLototoImportResult> ImportFromXlsxAsync(
        string xlsxPath,
        bool dryRun,
        int? criadoPorId = null,
        CancellationToken ct = default);
}

public class LideresLototoImportResult
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
