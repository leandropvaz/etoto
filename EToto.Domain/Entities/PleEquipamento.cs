namespace EToto.Domain.Entities
{
    public class PleEquipamento
    {
        public Guid PleId { get; set; }
        public Ple Ple { get; set; } = null!;

        public Guid EquipamentoId { get; set; }
        public Equipamento Equipamento { get; set; } = null!;
    }
}
