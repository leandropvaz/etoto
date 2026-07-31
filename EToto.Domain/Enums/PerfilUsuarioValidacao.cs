namespace EToto.Domain.Enums
{
    public static class PerfilUsuarioValidacao
    {
        public const string MensagemSuperGestorExclusivo =
            "O perfil SuperGestor não pode coexistir com outros perfis.";

        public const string MensagemPerfilObrigatorio =
            "O usuário precisa de pelo menos um perfil.";

        public static void ValidarCombinacao(IEnumerable<PerfilUsuario> perfis)
        {
            if (perfis is null)
                throw new ArgumentNullException(nameof(perfis));

            var distintos = perfis.Distinct().ToList();

            if (distintos.Count == 0)
                throw new InvalidOperationException(MensagemPerfilObrigatorio);

            if (distintos.Contains(PerfilUsuario.SuperGestor) && distintos.Count > 1)
                throw new InvalidOperationException(MensagemSuperGestorExclusivo);
        }
    }
}
