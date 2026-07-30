namespace EToto.Domain.Enums
{
    public enum PerfilUsuario
    {
        Usuario = 1,         // vê e pesquisa equipamentos da própria planta
        Administrador = 2,   // gerencia usuários e equipamento da planta
        SuperGestor = 3,     // vê todas as plantas e cadastra administradores
        UsuarioFinal = 4,    // Líder de Bloqueio - acesso full exceto criar Usuário e Planta
        ComandoCentral = 5   // Sala de Operações - finaliza bloqueios e imprime; AR só consulta+impressão
    }
}
