namespace EToto.Domain.Enums
{
    public enum StatusPle
    {
        Criado = 1,
        EmAndamento = 2,
        Cancelado = 3,
        // Antigo "Finalizado" - agora representa o passo intermediario "Inicio do Desbloqueio"
        // (Lider/Gestor/Admin chegam ate aqui; Comando Central conclui em Finalizado).
        InicioDesbloqueio = 4,
        // Novo passo terminal - exclusivo Comando Central / Admin / SuperGestor
        Finalizado = 5
    }
}
