namespace EToto.Domain.Entities
{
    public class AvaliacaoRiscoEquipamento
    {
        public Guid AvaliacaoRiscoId { get; set; }
        public AvaliacaoRisco AvaliacaoRisco { get; set; } = null!;

        public Guid EquipamentoId { get; set; }
        public Equipamento Equipamento { get; set; } = null!;
    }
}
