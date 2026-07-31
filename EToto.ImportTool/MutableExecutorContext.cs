using EToto.Domain.Interfaces;

namespace EToto.ImportTool;

// IExecutorContext com setter, usado pelo ImportTool para informar o operador
// que esta rodando o import. O interceptor de auditoria (#5a) le esse valor
// no SaveChangesAsync e grava em AuditoriaEntradas.UsuarioId, permitindo
// rastrear quem disparou cada importacao na tela /auditoria.
//
// Registrado como Scoped no Program.cs APOS o AddInfrastructure() — assim
// sobrescreve o AnonymousExecutorContext default.
public sealed class MutableExecutorContext : IExecutorContext
{
    public int? UsuarioIdAtual { get; set; }
}
