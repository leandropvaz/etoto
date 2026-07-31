namespace EToto.Domain.Entities
{
    public class PleHistorico
    {
        public Guid Id { get; set; }

        public Guid PleId { get; set; }
        public Ple Ple { get; set; } = null!;

        // Quem executou a ação
        public int UsuarioId { get; set; }
        public Usuarios Usuario { get; set; } = null!;

        // Ação realizada
        public string Acao { get; set; } = string.Empty; // Criado, Modificado, Suspenso, Retomado, Finalizado
        public string? StatusAnterior { get; set; }
        public string? StatusNovo { get; set; }
        public string? Detalhes { get; set; }

        public DateTime DataAcao { get; set; }
    }
}
