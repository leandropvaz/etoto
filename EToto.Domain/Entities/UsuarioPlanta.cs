namespace EToto.Domain.Entities
{
    public class UsuarioPlanta
    {
        public int UsuarioId { get; set; }
        public Usuarios Usuario { get; set; } = default!;

        public int PlantaId { get; set; }
        public Plantas Planta { get; set; } = default!;

        public DateTime DataAssociacao { get; set; } = DateTime.UtcNow;
    }
}