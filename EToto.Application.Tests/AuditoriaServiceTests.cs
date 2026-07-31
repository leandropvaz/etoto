using EToto.Application.Dto;
using EToto.Application.Services;
using EToto.Domain.Entities;
using EToto.Domain.Enums;
using EToto.Domain.Interfaces;
using EToto.Infrastructure.Data;
using EToto.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EToto.Application.Tests;

public class AuditoriaServiceTests
{
    private readonly DbContextOptions<LototoContext> _options;

    public AuditoriaServiceTests()
    {
        _options = new DbContextOptionsBuilder<LototoContext>()
            .UseInMemoryDatabase($"audit_svc_{Guid.NewGuid():N}")
            .Options;
    }

    [Fact]
    public async Task Consultar_FiltraPorPeriodo()
    {
        await Seed(
            new(2026, 06, 10, AcaoAuditoria.Criar, "Usuarios", null),
            new(2026, 06, 16, AcaoAuditoria.Atualizar, "Usuarios", null),
            new(2026, 06, 20, AcaoAuditoria.Excluir, "Usuarios", null));

        var svc = NovoService();

        var r = await svc.ConsultarAsync(new AuditoriaConsultaFiltro
        {
            PeriodoInicio = new DateTime(2026, 06, 15),
            PeriodoFim = new DateTime(2026, 06, 18)
        });

        Assert.Equal(1, r.Total);
        Assert.Equal((int)AcaoAuditoria.Atualizar, r.Itens[0].Acao);
    }

    [Fact]
    public async Task Consultar_FiltraPorUsuarioEntidadeAcao()
    {
        await Seed(
            new(2026, 06, 10, AcaoAuditoria.Criar, "Usuarios", 1),
            new(2026, 06, 11, AcaoAuditoria.Atualizar, "Usuarios", 2),
            new(2026, 06, 12, AcaoAuditoria.Criar, "Plantas", 1));

        var svc = NovoService();

        var r = await svc.ConsultarAsync(new AuditoriaConsultaFiltro
        {
            UsuarioId = 1,
            NomeTabela = "Usuarios",
            Acao = (int)AcaoAuditoria.Criar
        });

        Assert.Equal(1, r.Total);
        Assert.Equal("Usuarios", r.Itens[0].NomeTabela);
    }

    [Fact]
    public async Task Consultar_OrdenaPorExecutadoEmDesc()
    {
        await Seed(
            new(2026, 06, 10, AcaoAuditoria.Criar, "Usuarios", null),
            new(2026, 06, 16, AcaoAuditoria.Criar, "Usuarios", null),
            new(2026, 06, 12, AcaoAuditoria.Criar, "Usuarios", null));

        var svc = NovoService();

        var r = await svc.ConsultarAsync(new AuditoriaConsultaFiltro());

        Assert.Equal(3, r.Total);
        Assert.Equal(new DateTime(2026, 06, 16), r.Itens[0].ExecutadoEm.Date);
        Assert.Equal(new DateTime(2026, 06, 12), r.Itens[1].ExecutadoEm.Date);
        Assert.Equal(new DateTime(2026, 06, 10), r.Itens[2].ExecutadoEm.Date);
    }

    [Fact]
    public async Task Consultar_Paginacao()
    {
        var entries = Enumerable.Range(1, 25)
            .Select(i => new SeedEntry(2026, 06, i % 28 + 1, AcaoAuditoria.Criar, "Usuarios", null))
            .ToArray();
        await Seed(entries);

        var svc = NovoService();

        var p1 = await svc.ConsultarAsync(new AuditoriaConsultaFiltro { Pagina = 1, TamanhoPagina = 10 });
        var p2 = await svc.ConsultarAsync(new AuditoriaConsultaFiltro { Pagina = 2, TamanhoPagina = 10 });
        var p3 = await svc.ConsultarAsync(new AuditoriaConsultaFiltro { Pagina = 3, TamanhoPagina = 10 });

        Assert.Equal(25, p1.Total);
        Assert.Equal(10, p1.Itens.Count);
        Assert.Equal(10, p2.Itens.Count);
        Assert.Equal(5, p3.Itens.Count);
    }

    [Fact]
    public async Task Consultar_MapeiaNomeUsuario_QuandoIncluido()
    {
        await using (var ctx = new LototoContext(_options))
        {
            ctx.Usuarios.Add(new Usuarios { Id = 42, Login = "joao", NomeCompleto = "João Silva", SenhaHash = "h" });
            await ctx.SaveChangesAsync();

            ctx.AuditoriaEntradas.Add(new AuditoriaEntrada
            {
                NomeTabela = "Plantas",
                ChaveRegistro = "1",
                Acao = AcaoAuditoria.Criar,
                UsuarioId = 42,
                ExecutadoEm = new DateTime(2026, 06, 16, 0, 0, 0, DateTimeKind.Utc)
            });
            await ctx.SaveChangesAsync();
        }

        var svc = NovoService();
        var r = await svc.ConsultarAsync(new AuditoriaConsultaFiltro());

        var entrada = r.Itens.First(i => i.UsuarioId == 42);
        Assert.Equal("João Silva", entrada.UsuarioNome);
    }

    private AuditoriaService NovoService()
    {
        var ctx = new LototoContext(_options);
        return new AuditoriaService(new AuditoriaRepository(ctx));
    }

    private record SeedEntry(int Ano, int Mes, int Dia, AcaoAuditoria Acao, string Tabela, int? UsuarioId);

    private async Task Seed(params SeedEntry[] entries)
    {
        await using var ctx = new LototoContext(_options);
        foreach (var e in entries)
        {
            ctx.AuditoriaEntradas.Add(new AuditoriaEntrada
            {
                NomeTabela = e.Tabela,
                ChaveRegistro = "1",
                Acao = e.Acao,
                UsuarioId = e.UsuarioId,
                ExecutadoEm = new DateTime(e.Ano, e.Mes, e.Dia, 0, 0, 0, DateTimeKind.Utc)
            });
        }
        await ctx.SaveChangesAsync();
    }
}
