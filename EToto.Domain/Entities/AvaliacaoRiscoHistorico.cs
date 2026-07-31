namespace EToto.Domain.Entities
{
    public class AvaliacaoRiscoHistorico
    {
        public Guid Id { get; set; }

        public Guid AvaliacaoRiscoId { get; set; }
        public AvaliacaoRisco AvaliacaoRisco { get; set; } = null!;

        public int UsuarioId { get; set; }
        public Usuarios Usuario { get; set; } = null!;

        public string Acao { get; set; } = string.Empty;
        public string? Detalhes { get; set; }

        public DateTime DataAcao { get; set; }
    }
}
