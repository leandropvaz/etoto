namespace EToto.Domain.Interfaces
{
    // Expõe o usuário que está executando a operação corrente.
    // Usado pela auditoria (#5a) para anexar UsuarioId às entradas geradas no SaveChanges.
    public interface IExecutorContext
    {
        // Retorna null quando não há usuário identificado (ImportTool, jobs, contexto pré-login).
        int? UsuarioIdAtual { get; }
    }
}
