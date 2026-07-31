namespace EToto.Web.Dto
{
    public class LototoConfig
    {
        public bool Habilitado { get; set; }
        public List<string> UsuariosPermitidos { get; set; } = new();

        public bool PermiteSemSenha(string? login)
        {
            if (!Habilitado || string.IsNullOrWhiteSpace(login))
                return false;

            return UsuariosPermitidos.Any(u =>
                u.Equals(login, StringComparison.OrdinalIgnoreCase));
        }
    }
}
