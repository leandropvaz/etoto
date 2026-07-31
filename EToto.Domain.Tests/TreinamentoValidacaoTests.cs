using EToto.Domain.Entities;
using EToto.Domain.Enums;
using Xunit;

namespace EToto.Domain.Tests;

public class TreinamentoValidacaoTests
{
    [Theory]
    [InlineData(PerfilUsuario.Usuario, true)]
    [InlineData(PerfilUsuario.UsuarioFinal, true)]
    [InlineData(PerfilUsuario.ComandoCentral, true)]
    [InlineData(PerfilUsuario.Administrador, false)]
    [InlineData(PerfilUsuario.SuperGestor, false)]
    public void PerfilExigeTreinamento_PorPerfil(PerfilUsuario perfil, bool esperado)
    {
        Assert.Equal(esperado, TreinamentoValidacao.PerfilExigeTreinamento(perfil));
    }

    [Fact]
    public void AlgumPerfilExigeTreinamento_AdminMaisUsuarioFinal_True()
    {
        var resultado = TreinamentoValidacao.AlgumPerfilExigeTreinamento(
            new[] { PerfilUsuario.Administrador, PerfilUsuario.UsuarioFinal });

        Assert.True(resultado);
    }

    [Fact]
    public void AlgumPerfilExigeTreinamento_SoSupervisao_False()
    {
        var resultado = TreinamentoValidacao.AlgumPerfilExigeTreinamento(
            new[] { PerfilUsuario.SuperGestor });

        Assert.False(resultado);
    }

    [Fact]
    public void Usuarios_ExigeTreinamentoValido_UsaColecaoPerfis()
    {
        var u = new Usuarios { Login = "x", NomeCompleto = "X", SenhaHash = "h" };
        u.DefinirPerfis(new[] { PerfilUsuario.SuperGestor });
        Assert.False(u.ExigeTreinamentoValido());

        u.DefinirPerfis(new[] { PerfilUsuario.UsuarioFinal });
        Assert.True(u.ExigeTreinamentoValido());
    }

    [Fact]
    public void Usuarios_ExigeTreinamentoValido_FallbackParaPerfilLegado()
    {
        var u = new Usuarios
        {
            Login = "x", NomeCompleto = "X", SenhaHash = "h",
            Perfil = PerfilUsuario.UsuarioFinal
            // Perfis deliberadamente vazio.
        };

        Assert.True(u.ExigeTreinamentoValido());
    }

    [Theory]
    [InlineData(null, StatusValidadeAcesso.SemValidade)]
    [InlineData(-1, StatusValidadeAcesso.Vencido)]
    [InlineData(0, StatusValidadeAcesso.Vencendo)]
    [InlineData(30, StatusValidadeAcesso.Vencendo)]
    [InlineData(31, StatusValidadeAcesso.Vigente)]
    public void Usuarios_StatusValidadeTreinamento_UsaJanelaPadrao(int? diasOffset, StatusValidadeAcesso esperado)
    {
        var referencia = new DateTime(2026, 06, 16, 12, 0, 0, DateTimeKind.Utc);
        var u = new Usuarios { Login = "x", NomeCompleto = "X", SenhaHash = "h" };
        u.DataValidadeTreinamento = diasOffset.HasValue ? referencia.AddDays(diasOffset.Value) : null;

        Assert.Equal(esperado, u.StatusValidadeTreinamento(referencia));
    }
}
