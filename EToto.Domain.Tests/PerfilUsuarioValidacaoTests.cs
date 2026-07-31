using EToto.Domain.Enums;
using Xunit;

namespace EToto.Domain.Tests;

public class PerfilUsuarioValidacaoTests
{
    [Fact]
    public void ValidarCombinacao_ListaVazia_LancaInvalidOperation()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => PerfilUsuarioValidacao.ValidarCombinacao(Array.Empty<PerfilUsuario>()));

        Assert.Equal(PerfilUsuarioValidacao.MensagemPerfilObrigatorio, ex.Message);
    }

    [Fact]
    public void ValidarCombinacao_SuperGestorSozinho_Aceita()
    {
        PerfilUsuarioValidacao.ValidarCombinacao(new[] { PerfilUsuario.SuperGestor });
    }

    [Fact]
    public void ValidarCombinacao_SuperGestorComOutroPerfil_LancaInvalidOperation()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => PerfilUsuarioValidacao.ValidarCombinacao(new[]
            {
                PerfilUsuario.SuperGestor,
                PerfilUsuario.Administrador
            }));

        Assert.Equal(PerfilUsuarioValidacao.MensagemSuperGestorExclusivo, ex.Message);
    }

    [Fact]
    public void ValidarCombinacao_MultiplosPerfisSemSuperGestor_Aceita()
    {
        PerfilUsuarioValidacao.ValidarCombinacao(new[]
        {
            PerfilUsuario.Administrador,
            PerfilUsuario.UsuarioFinal,
            PerfilUsuario.ComandoCentral
        });
    }

    [Fact]
    public void ValidarCombinacao_PerfilDuplicado_NaoLancaQuandoUnicoEfetivo()
    {
        PerfilUsuarioValidacao.ValidarCombinacao(new[]
        {
            PerfilUsuario.Administrador,
            PerfilUsuario.Administrador
        });
    }
}
