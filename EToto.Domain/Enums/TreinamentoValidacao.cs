namespace EToto.Domain.Enums
{
    // Regras de validade do treinamento LOTOTO (#3).
    public static class TreinamentoValidacao
    {
        // Perfis que exigem treinamento válido para acessar o sistema.
        // SuperGestor e Administrador NÃO precisam (são supervisão/cadastro).
        private static readonly HashSet<PerfilUsuario> PerfisQueExigem = new()
        {
            PerfilUsuario.Usuario,
            PerfilUsuario.UsuarioFinal,    // Líder de Bloqueio
            PerfilUsuario.ComandoCentral
        };

        public static bool PerfilExigeTreinamento(PerfilUsuario perfil)
            => PerfisQueExigem.Contains(perfil);

        public static bool AlgumPerfilExigeTreinamento(IEnumerable<PerfilUsuario> perfis)
            => perfis is not null && perfis.Any(PerfilExigeTreinamento);
    }
}
