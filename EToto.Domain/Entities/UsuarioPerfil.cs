using EToto.Domain.Enums;

namespace EToto.Domain.Entities
{
    public class UsuarioPerfil
    {
        public int UsuarioId { get; set; }
        public Usuarios Usuario { get; set; } = default!;

        public PerfilUsuario Perfil { get; set; }

        public DateTime DataAssociacao { get; set; } = DateTime.UtcNow;
    }
}
