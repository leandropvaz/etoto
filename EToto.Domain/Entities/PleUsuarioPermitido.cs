namespace EToto.Domain.Entities
{
    public class PleUsuarioPermitido
    {
        public Guid PleId { get; set; }
        public Ple Ple { get; set; } = null!;

        public int UsuarioId { get; set; }
        public Usuarios Usuario { get; set; } = null!;
    }
}
