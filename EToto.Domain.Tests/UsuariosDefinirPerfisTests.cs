using EToto.Domain.Entities;
using EToto.Domain.Enums;
using Xunit;

namespace EToto.Domain.Tests;

public class UsuariosDefinirPerfisTests
{
    [Fact]
    public void DefinirPerfis_ListaSomenteSuperGestor_SincronizaCampoLegado()
    {
        var u = NovoUsuario();

        u.DefinirPerfis(new[] { PerfilUsuario.SuperGestor });

        Assert.Single(u.Perfis);
        Assert.Equal(PerfilUsuario.SuperGestor, u.Perfil);
        Assert.True(u.EhSuperGestor);
    }

    [Fact]
    public void DefinirPerfis_MultiplosPerfis_SemSuperGestor_PreservaTodos()
    {
        var u = NovoUsuario();

        u.DefinirPerfis(new[] { PerfilUsuario.Administrador, PerfilUsuario.UsuarioFinal });

        Assert.Equal(2, u.Perfis.Count);
        Assert.False(u.EhSuperGestor);
        Assert.True(u.EhUsuarioFinal);
    }

    [Fact]
    public void DefinirPerfis_SuperGestorComOutro_Lanca()
    {
        var u = NovoUsuario();

        Assert.Throws<InvalidOperationException>(() =>
            u.DefinirPerfis(new[] { PerfilUsuario.SuperGestor, PerfilUsuario.UsuarioFinal }));
    }

    [Fact]
    public void DefinirPerfis_SubstituiPerfisAnteriores()
    {
        var u = NovoUsuario();
        u.DefinirPerfis(new[] { PerfilUsuario.Administrador });

        u.DefinirPerfis(new[] { PerfilUsuario.UsuarioFinal, PerfilUsuario.ComandoCentral });

        Assert.Equal(2, u.Perfis.Count);
        Assert.DoesNotContain(u.Perfis, p => p.Perfil == PerfilUsuario.Administrador);
    }

    [Fact]
    public void EhSuperGestor_QuandoColecaoVazia_UsaCampoLegado()
    {
        var u = NovoUsuario();
        u.Perfil = PerfilUsuario.SuperGestor;

        Assert.True(u.EhSuperGestor);
    }

    [Fact]
    public void EhUsuarioFinal_QuandoColecaoVazia_UsaCampoLegado()
    {
        var u = NovoUsuario();
        u.Perfil = PerfilUsuario.UsuarioFinal;

        Assert.True(u.EhUsuarioFinal);
    }

    private static Usuarios NovoUsuario() => new()
    {
        Id = 1,
        Login = "teste",
        NomeCompleto = "Teste",
        SenhaHash = "x"
    };
}
