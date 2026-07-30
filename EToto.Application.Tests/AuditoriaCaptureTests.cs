using System.Text.Json;
using EToto.Domain.Entities;
using EToto.Domain.Enums;
using EToto.Domain.Interfaces;
using EToto.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EToto.Application.Tests;

// Testes integrados do interceptor de auditoria (#5a) com EF Core InMemory.
public class AuditoriaCaptureTests
{
    private readonly DbContextOptions<LototoContext> _options;
    private readonly string _dbName;

    public AuditoriaCaptureTests()
    {
        _dbName = $"audit_{Guid.NewGuid():N}";
        _options = new DbContextOptionsBuilder<LototoContext>()
            .UseInMemoryDatabase(_dbName)
            .Options;
    }

    [Fact]
    public async Task Criar_Usuario_GeraEntradaCriar()
    {
        await using var ctx = NovoContexto(usuarioId: 99);
        ctx.Usuarios.Add(NovoUsuario("joao"));
        await ctx.SaveChangesAsync();

        var entrada = Assert.Single(ctx.AuditoriaEntradas.ToList());
        Assert.Equal("Usuarios", entrada.NomeTabela);
        Assert.Equal(AcaoAuditoria.Criar, entrada.Acao);
        Assert.Equal(99, entrada.UsuarioId);
        Assert.Null(entrada.ValoresAntes);
        Assert.NotNull(entrada.ValoresDepois);
        Assert.NotEqual(string.Empty, entrada.ChaveRegistro);
    }

    [Fact]
    public async Task Atualizar_Usuario_GeraEntradaAtualizarComAntesEDepois()
    {
        await using (var setup = NovoContexto())
        {
            setup.Usuarios.Add(NovoUsuario("joao"));
            await setup.SaveChangesAsync();
            setup.AuditoriaEntradas.RemoveRange(setup.AuditoriaEntradas);
            await setup.SaveChangesAsync();
        }

        await using (var ctx = NovoContexto(usuarioId: 7))
        {
            var u = ctx.Usuarios.First(x => x.Login == "joao");
            u.NomeCompleto = "Joao Atualizado";
            await ctx.SaveChangesAsync();
        }

        await using var assert = NovoContexto();
        var entrada = Assert.Single(
            assert.AuditoriaEntradas.Where(a => a.Acao == AcaoAuditoria.Atualizar).ToList());
        Assert.Equal(7, entrada.UsuarioId);
        Assert.NotNull(entrada.ValoresAntes);
        Assert.NotNull(entrada.ValoresDepois);

        var antes = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(entrada.ValoresAntes!);
        var depois = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(entrada.ValoresDepois!);
        Assert.Equal("joao", antes!["NomeCompleto"].GetString());
        Assert.Equal("Joao Atualizado", depois!["NomeCompleto"].GetString());
    }

    [Fact]
    public async Task Excluir_Usuario_GeraEntradaExcluirComAntesNaoNulo()
    {
        await using (var setup = NovoContexto())
        {
            setup.Usuarios.Add(NovoUsuario("maria"));
            await setup.SaveChangesAsync();
            setup.AuditoriaEntradas.RemoveRange(setup.AuditoriaEntradas);
            await setup.SaveChangesAsync();
        }

        await using (var ctx = NovoContexto())
        {
            var u = ctx.Usuarios.First(x => x.Login == "maria");
            ctx.Usuarios.Remove(u);
            await ctx.SaveChangesAsync();
        }

        await using var assert = NovoContexto();
        var entrada = Assert.Single(
            assert.AuditoriaEntradas.Where(a => a.Acao == AcaoAuditoria.Excluir).ToList());
        Assert.NotNull(entrada.ValoresAntes);
        Assert.Null(entrada.ValoresDepois);
    }

    [Fact]
    public async Task NaoAudita_AuditoriaEntrada_Recursivamente()
    {
        await using var ctx = NovoContexto();
        ctx.AuditoriaEntradas.Add(new AuditoriaEntrada
        {
            NomeTabela = "Plantas",
            ChaveRegistro = "1",
            Acao = AcaoAuditoria.Criar
        });
        await ctx.SaveChangesAsync();

        // Inserir uma AuditoriaEntrada manualmente não deve gerar OUTRA AuditoriaEntrada.
        Assert.Single(ctx.AuditoriaEntradas);
    }

    [Fact]
    public async Task UsuarioIdAnonimo_DeixaCampoNuloNasEntradas()
    {
        await using var ctx = NovoContexto(usuarioId: null);
        ctx.Plantas.Add(new Plantas { Nome = "P1", Codigo = "P1", Ativa = true });
        await ctx.SaveChangesAsync();

        var entrada = Assert.Single(
            ctx.AuditoriaEntradas.Where(a => a.Acao == AcaoAuditoria.Criar).ToList());
        Assert.Null(entrada.UsuarioId);
    }

    [Fact]
    public async Task EntidadeNaoAuditada_NaoGeraEntrada()
    {
        // UsuarioPlanta NÃO está na lista de tipos auditados — não deve gerar entrada.
        await using (var setup = NovoContexto())
        {
            setup.Plantas.Add(new Plantas { Id = 1, Nome = "P", Codigo = "P", Ativa = true });
            setup.Usuarios.Add(new Usuarios { Id = 1, Login = "u", NomeCompleto = "u", SenhaHash = "h" });
            await setup.SaveChangesAsync();
            setup.AuditoriaEntradas.RemoveRange(setup.AuditoriaEntradas);
            await setup.SaveChangesAsync();
        }

        await using var ctx = NovoContexto();
        ctx.UsuarioPlantas.Add(new UsuarioPlanta { UsuarioId = 1, PlantaId = 1 });
        await ctx.SaveChangesAsync();

        Assert.Empty(ctx.AuditoriaEntradas.ToList());
    }

    private LototoContext NovoContexto(int? usuarioId = null)
        => new LototoContext(_options, new FakeExecutor(usuarioId));

    private static Usuarios NovoUsuario(string login) => new()
    {
        Login = login,
        NomeCompleto = login,
        SenhaHash = "h"
    };

    private sealed class FakeExecutor : IExecutorContext
    {
        public FakeExecutor(int? id) => UsuarioIdAtual = id;
        public int? UsuarioIdAtual { get; }
    }
}
