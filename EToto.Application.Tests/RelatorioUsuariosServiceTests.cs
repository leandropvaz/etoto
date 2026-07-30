using EToto.Application.Dto;
using EToto.Application.Services;
using EToto.Domain.Entities;
using EToto.Domain.Enums;
using EToto.Domain.Interfaces;
using Xunit;

namespace EToto.Application.Tests;

public class RelatorioUsuariosServiceTests
{
    [Fact]
    public async Task Gerar_FiltraSomenteAtivos()
    {
        var repo = new FakeRepo();
        repo.Usuarios.Add(NovoUsuario("ana", ativa: true));
        repo.Usuarios.Add(NovoUsuario("bia", ativa: false));

        var svc = new RelatorioUsuariosService(repo);
        var r = await svc.GerarAsync(new RelatorioUsuariosFiltro());

        Assert.Single(r);
        Assert.Equal("ana", r[0].Login);
    }

    [Fact]
    public async Task Gerar_FiltraPorPerfil()
    {
        var repo = new FakeRepo();
        var ana = NovoUsuario("ana", ativa: true);
        ana.DefinirPerfis(new[] { PerfilUsuario.UsuarioFinal });
        var bia = NovoUsuario("bia", ativa: true);
        bia.DefinirPerfis(new[] { PerfilUsuario.Administrador });
        repo.Usuarios.Add(ana);
        repo.Usuarios.Add(bia);

        var svc = new RelatorioUsuariosService(repo);
        var r = await svc.GerarAsync(new RelatorioUsuariosFiltro { Perfil = (int)PerfilUsuario.UsuarioFinal });

        Assert.Single(r);
        Assert.Equal("ana", r[0].Login);
    }

    [Fact]
    public async Task Gerar_FiltraPorTipoVinculo()
    {
        var repo = new FakeRepo();
        var ana = NovoUsuario("ana", ativa: true);
        ana.TipoVinculo = TipoVinculo.Terceiro;
        ana.NomeEmpresa = "ACME";
        ana.DataValidadeAcesso = DateTime.UtcNow.AddDays(60);
        var bia = NovoUsuario("bia", ativa: true); // Funcionario padrão
        repo.Usuarios.Add(ana);
        repo.Usuarios.Add(bia);

        var svc = new RelatorioUsuariosService(repo);
        var r = await svc.GerarAsync(new RelatorioUsuariosFiltro { TipoVinculo = (int)TipoVinculo.Terceiro });

        Assert.Single(r);
        Assert.Equal("ACME", r[0].NomeEmpresa);
    }

    [Fact]
    public async Task Gerar_FiltraPorStatusValidadeVencido()
    {
        var repo = new FakeRepo();
        var ana = NovoUsuario("ana", ativa: true);
        ana.TipoVinculo = TipoVinculo.Terceiro;
        ana.DataValidadeAcesso = DateTime.UtcNow.AddDays(-1); // Vencido
        var bia = NovoUsuario("bia", ativa: true);
        bia.DataValidadeAcesso = DateTime.UtcNow.AddDays(60); // Vigente
        repo.Usuarios.Add(ana);
        repo.Usuarios.Add(bia);

        var svc = new RelatorioUsuariosService(repo);
        var r = await svc.GerarAsync(new RelatorioUsuariosFiltro
        {
            StatusValidade = (int)StatusValidadeAcesso.Vencido
        });

        Assert.Single(r);
        Assert.Equal("ana", r[0].Login);
    }

    [Fact]
    public async Task Gerar_OrdenaPorNome()
    {
        var repo = new FakeRepo();
        repo.Usuarios.Add(NovoUsuario("zeca", ativa: true, nome: "Zeca Pagodinho"));
        repo.Usuarios.Add(NovoUsuario("ana",  ativa: true, nome: "Ana Maria"));

        var svc = new RelatorioUsuariosService(repo);
        var r = await svc.GerarAsync(new RelatorioUsuariosFiltro());

        Assert.Equal(new[] { "Ana Maria", "Zeca Pagodinho" }, r.Select(i => i.NomeCompleto).ToArray());
    }

    [Fact]
    public async Task Gerar_ExigeTreinamento_DependeDosPerfis()
    {
        var repo = new FakeRepo();
        var super = NovoUsuario("supergestor", ativa: true);
        super.DefinirPerfis(new[] { PerfilUsuario.SuperGestor });
        var lider = NovoUsuario("lider", ativa: true);
        lider.DefinirPerfis(new[] { PerfilUsuario.UsuarioFinal });
        repo.Usuarios.Add(super);
        repo.Usuarios.Add(lider);

        var svc = new RelatorioUsuariosService(repo);
        var r = await svc.GerarAsync(new RelatorioUsuariosFiltro());

        Assert.False(r.First(i => i.Login == "supergestor").ExigeTreinamento);
        Assert.True(r.First(i => i.Login == "lider").ExigeTreinamento);
    }

    private static Usuarios NovoUsuario(string login, bool ativa, string? nome = null) => new()
    {
        Login = login,
        NomeCompleto = nome ?? login,
        SenhaHash = "h",
        Ativa = ativa
    };

    private sealed class FakeRepo : IUsuarioRepository
    {
        public List<Usuarios> Usuarios { get; } = new();
        public Task<IReadOnlyList<Usuarios>> ListarComPlantasAsync()
            => Task.FromResult<IReadOnlyList<Usuarios>>(Usuarios);

        public Task<Usuarios?> ObterPorLoginAsync(string login)
            => Task.FromResult(Usuarios.FirstOrDefault(u => u.Login == login));
        public Task<IReadOnlyList<Usuarios>> ListarPorPlantaAsync(int plantaId)
            => Task.FromResult<IReadOnlyList<Usuarios>>(Array.Empty<Usuarios>());
        public Task<IReadOnlyList<Plantas>> ObterPlantasDoUsuarioAsync(int usuarioId)
            => Task.FromResult<IReadOnlyList<Plantas>>(Array.Empty<Plantas>());
        public Task<Usuarios?> ObterComPlantasAsync(int id)
            => Task.FromResult(Usuarios.FirstOrDefault(u => u.Id == id));
        public Task AddAsync(Usuarios entidade) { Usuarios.Add(entidade); return Task.CompletedTask; }
        public void Update(Usuarios entidade) { }
        public void Delete(Usuarios entidade) { }
        public Task<Usuarios?> GetByIdAsync(int id)
            => Task.FromResult(Usuarios.FirstOrDefault(u => u.Id == id));
        public Task<IReadOnlyList<Usuarios>> GetAsync()
            => Task.FromResult<IReadOnlyList<Usuarios>>(Usuarios);
    }
}
