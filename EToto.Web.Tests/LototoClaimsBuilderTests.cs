using System.Security.Claims;
using EToto.Domain.Enums;
using EToto.Web.Services;
using Xunit;
using static EToto.Web.Services.LototoAuthenticationStateProvider;

namespace EToto.Web.Tests;

public class LototoClaimsBuilderTests
{
    [Fact]
    public void Build_UsuarioComUmPerfil_EmiteUmaClaimRole()
    {
        var user = NovoUser(PerfilUsuario.Administrador);

        var principal = LototoClaimsBuilder.Build(user);

        Assert.True(principal.IsInRole(nameof(PerfilUsuario.Administrador)));
        Assert.False(principal.IsInRole(nameof(PerfilUsuario.SuperGestor)));
    }

    [Fact]
    public void Build_UsuarioComMultiplosPerfis_EmiteRoleParaCadaUm()
    {
        var user = NovoUser(PerfilUsuario.Administrador, PerfilUsuario.UsuarioFinal, PerfilUsuario.ComandoCentral);

        var principal = LototoClaimsBuilder.Build(user);

        Assert.True(principal.IsInRole(nameof(PerfilUsuario.Administrador)));
        Assert.True(principal.IsInRole(nameof(PerfilUsuario.UsuarioFinal)));
        Assert.True(principal.IsInRole(nameof(PerfilUsuario.ComandoCentral)));
        Assert.False(principal.IsInRole(nameof(PerfilUsuario.SuperGestor)));
        Assert.False(principal.IsInRole(nameof(PerfilUsuario.Usuario)));

        // Conta exatamente 3 Role claims
        Assert.Equal(3, principal.FindAll(ClaimTypes.Role).Count());
    }

    [Fact]
    public void Build_SuperGestor_SozinhoEmiteUmaClaimRole()
    {
        var user = NovoUser(PerfilUsuario.SuperGestor);

        var principal = LototoClaimsBuilder.Build(user);

        Assert.True(principal.IsInRole(nameof(PerfilUsuario.SuperGestor)));
        Assert.Single(principal.FindAll(ClaimTypes.Role));
    }

    [Fact]
    public void Build_QuandoColecaoVazia_UsaCampoLegado()
    {
        // Cenário de compat: SerializableUser antigo no SessionStorage sem PerfisNomes preenchidos.
        var user = new SerializableUser
        {
            Nome = "Compat",
            UserId = 9,
            Perfil = (int)PerfilUsuario.UsuarioFinal,
            PerfilNome = nameof(PerfilUsuario.UsuarioFinal)
            // Perfis/PerfisNomes deliberadamente vazios
        };

        var principal = LototoClaimsBuilder.Build(user);

        Assert.True(principal.IsInRole(nameof(PerfilUsuario.UsuarioFinal)));
        Assert.Single(principal.FindAll(ClaimTypes.Role));
    }

    [Fact]
    public void Build_PerfilDuplicado_EmiteUmaUnicaClaimRole()
    {
        var user = new SerializableUser
        {
            Nome = "Dup",
            UserId = 10,
            Perfil = (int)PerfilUsuario.Administrador,
            PerfilNome = nameof(PerfilUsuario.Administrador),
            Perfis = new() { (int)PerfilUsuario.Administrador, (int)PerfilUsuario.Administrador },
            PerfisNomes = new() { nameof(PerfilUsuario.Administrador), nameof(PerfilUsuario.Administrador) }
        };

        var principal = LototoClaimsBuilder.Build(user);

        Assert.Single(principal.FindAll(ClaimTypes.Role));
    }

    [Theory]
    [InlineData(PerfilUsuario.Administrador, PerfilUsuario.UsuarioFinal)]
    [InlineData(PerfilUsuario.UsuarioFinal, PerfilUsuario.ComandoCentral)]
    [InlineData(PerfilUsuario.Usuario, PerfilUsuario.Administrador, PerfilUsuario.ComandoCentral)]
    public void Build_OrEntrePerfis_SatisfazRegraDeTelaQueUsaIsInRoleComOr(params PerfilUsuario[] perfis)
    {
        // Simula a regra tipica das telas (ex.: MainLayout) que usa: IsAdmin || IsSuperGestor.
        var user = NovoUser(perfis);
        var principal = LototoClaimsBuilder.Build(user);

        bool isAdmin = principal.IsInRole(nameof(PerfilUsuario.Administrador));
        bool isSuper = principal.IsInRole(nameof(PerfilUsuario.SuperGestor));
        bool isUsuarioFinal = principal.IsInRole(nameof(PerfilUsuario.UsuarioFinal));
        bool isComandoCentral = principal.IsInRole(nameof(PerfilUsuario.ComandoCentral));

        // Confere se cada perfil esperado satisfaz o IsInRole correspondente
        foreach (var p in perfis)
        {
            switch (p)
            {
                case PerfilUsuario.Administrador: Assert.True(isAdmin); break;
                case PerfilUsuario.SuperGestor: Assert.True(isSuper); break;
                case PerfilUsuario.UsuarioFinal: Assert.True(isUsuarioFinal); break;
                case PerfilUsuario.ComandoCentral: Assert.True(isComandoCentral); break;
            }
        }
    }

    [Fact]
    public void Build_IdentidadeAutenticadaComTipoLototoAuth()
    {
        var user = NovoUser(PerfilUsuario.Usuario);

        var principal = LototoClaimsBuilder.Build(user);

        Assert.True(principal.Identity?.IsAuthenticated);
        Assert.Equal(LototoClaimsBuilder.AuthenticationType, principal.Identity?.AuthenticationType);
    }

    private static SerializableUser NovoUser(params PerfilUsuario[] perfis)
    {
        var primario = Array.Exists(perfis, p => p == PerfilUsuario.SuperGestor)
            ? PerfilUsuario.SuperGestor
            : perfis[0];

        return new SerializableUser
        {
            Nome = "Teste",
            UserId = 42,
            Perfil = (int)primario,
            PerfilNome = primario.ToString(),
            Perfis = perfis.Select(p => (int)p).ToList(),
            PerfisNomes = perfis.Select(p => p.ToString()).ToList()
        };
    }
}
