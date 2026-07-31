using EToto.Domain.Interfaces;

namespace EToto.Infrastructure.Data
{
    // Implementação default — operações sem usuário identificado (ImportTool, jobs).
    public class AnonymousExecutorContext : IExecutorContext
    {
        public int? UsuarioIdAtual => null;
    }
}
