using EToto.Domain.Entities;
using EToto.Domain.Enums;
using Xunit;

namespace EToto.Domain.Tests;

public class VinculoValidacaoTests
{
    [Fact]
    public void ValidarTerceiro_Funcionario_NaoLanca_MesmoSemEmpresa()
    {
        VinculoValidacao.ValidarTerceiro(TipoVinculo.Funcionario, null, null);
    }

    [Fact]
    public void ValidarTerceiro_TerceiroSemEmpresa_Lanca()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => VinculoValidacao.ValidarTerceiro(TipoVinculo.Terceiro, "", DateTime.UtcNow.AddDays(60)));
        Assert.Equal(VinculoValidacao.MensagemEmpresaObrigatoria, ex.Message);
    }

    [Fact]
    public void ValidarTerceiro_TerceiroSemValidade_Lanca()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => VinculoValidacao.ValidarTerceiro(TipoVinculo.Terceiro, "Empresa X", null));
        Assert.Equal(VinculoValidacao.MensagemValidadeObrigatoria, ex.Message);
    }

    [Fact]
    public void ValidarTerceiro_TerceiroCompleto_NaoLanca()
    {
        VinculoValidacao.ValidarTerceiro(TipoVinculo.Terceiro, "Empresa X", DateTime.UtcNow.AddDays(60));
    }

    [Fact]
    public void ValidarLimiteValidadeAcesso_AlemDe6Meses_Lanca()
    {
        var referencia = new DateTime(2026, 06, 16, 12, 0, 0, DateTimeKind.Utc);
        // 1 dia além do limite de 6 meses.
        var dataInvalida = referencia.Date.AddMonths(6).AddDays(1);

        var ex = Assert.Throws<InvalidOperationException>(
            () => VinculoValidacao.ValidarLimiteValidadeAcesso(TipoVinculo.Terceiro, dataInvalida, referencia));
        Assert.Equal(VinculoValidacao.MensagemValidadeMaxima, ex.Message);
    }

    [Fact]
    public void ValidarLimiteValidadeAcesso_Exatamente6Meses_NaoLanca()
    {
        var referencia = new DateTime(2026, 06, 16, 12, 0, 0, DateTimeKind.Utc);
        var dataLimite = referencia.Date.AddMonths(6);

        VinculoValidacao.ValidarLimiteValidadeAcesso(TipoVinculo.Terceiro, dataLimite, referencia);
    }

    [Fact]
    public void ValidarLimiteValidadeAcesso_Funcionario_NaoLanca()
    {
        var referencia = new DateTime(2026, 06, 16, 12, 0, 0, DateTimeKind.Utc);
        var dataLonge = referencia.Date.AddYears(5);

        VinculoValidacao.ValidarLimiteValidadeAcesso(TipoVinculo.Funcionario, dataLonge, referencia);
    }

    [Theory]
    [InlineData(null, StatusValidadeAcesso.SemValidade)]
    [InlineData(-1, StatusValidadeAcesso.Vencido)]
    [InlineData(0, StatusValidadeAcesso.Vencendo)]
    [InlineData(15, StatusValidadeAcesso.Vencendo)]
    [InlineData(30, StatusValidadeAcesso.Vencendo)]
    [InlineData(31, StatusValidadeAcesso.Vigente)]
    [InlineData(365, StatusValidadeAcesso.Vigente)]
    public void AvaliarStatus_VariosOffsets(int? diasOffset, StatusValidadeAcesso esperado)
    {
        var referencia = new DateTime(2026, 06, 16, 12, 0, 0, DateTimeKind.Utc);
        DateTime? data = diasOffset.HasValue ? referencia.AddDays(diasOffset.Value) : null;

        var status = VinculoValidacao.AvaliarStatus(data, referencia);

        Assert.Equal(esperado, status);
    }

    [Fact]
    public void Usuarios_EhTerceiro_ReflectsTipoVinculo()
    {
        var u = new Usuarios { Login = "x", NomeCompleto = "X", SenhaHash = "h" };
        Assert.False(u.EhTerceiro);
        u.TipoVinculo = TipoVinculo.Terceiro;
        Assert.True(u.EhTerceiro);
    }
}
