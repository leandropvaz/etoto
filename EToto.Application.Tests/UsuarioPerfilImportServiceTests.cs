using EToto.Domain.Entities;
using EToto.Domain.Enums;
using EToto.Domain.Interfaces;
using EToto.ImportTool;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace EToto.Application.Tests;

public class UsuarioPerfilImportServiceTests
{
    [Fact]
    public async Task ImportFromCsv_AtualizaPerfisDosUsuariosExistentes()
    {
        var joao = NovoUsuario(1, "joao");
        joao.DefinirPerfis(new[] { PerfilUsuario.Usuario });

        var maria = NovoUsuario(2, "maria");
        maria.DefinirPerfis(new[] { PerfilUsuario.Usuario });

        var (svc, repo, _, csvPath) = CriarComCsv(new[]
        {
            "Login;Perfis",
            "joao;Administrador,UsuarioFinal",
            "maria;SuperGestor"
        }, usuarios: new[] { joao, maria });

        var result = await svc.ImportFromCsvAsync(csvPath);

        Assert.Equal(2, result.TotalLinhas);
        Assert.Equal(2, result.UsuariosAtualizados);
        Assert.Equal(0, result.UsuariosNaoEncontrados);
        Assert.Equal(0, result.LinhasInvalidas);

        Assert.Equal(2, joao.Perfis.Count);
        Assert.Contains(joao.Perfis, p => p.Perfil == PerfilUsuario.Administrador);
        Assert.Contains(joao.Perfis, p => p.Perfil == PerfilUsuario.UsuarioFinal);

        Assert.Single(maria.Perfis);
        Assert.Equal(PerfilUsuario.SuperGestor, maria.Perfis.First().Perfil);
    }

    [Fact]
    public async Task ImportFromCsv_UsuarioInexistente_NaoAlteraOutroUsuario()
    {
        var joao = NovoUsuario(1, "joao");
        joao.DefinirPerfis(new[] { PerfilUsuario.Usuario });

        var (svc, _, _, csvPath) = CriarComCsv(new[]
        {
            "Login;Perfis",
            "fantasma;Administrador",
            "joao;UsuarioFinal"
        }, usuarios: new[] { joao });

        var result = await svc.ImportFromCsvAsync(csvPath);

        Assert.Equal(1, result.UsuariosAtualizados);
        Assert.Equal(1, result.UsuariosNaoEncontrados);
        Assert.Contains(joao.Perfis, p => p.Perfil == PerfilUsuario.UsuarioFinal);
    }

    [Fact]
    public async Task ImportFromCsv_LinhaInvalida_NaoQuebraOResto()
    {
        var joao = NovoUsuario(1, "joao");
        joao.DefinirPerfis(new[] { PerfilUsuario.Usuario });

        var (svc, _, _, csvPath) = CriarComCsv(new[]
        {
            "Login;Perfis",
            "linha_sem_separador",
            ";Administrador",
            "joao;PerfilDesconhecido",
            "joao;Administrador"
        }, usuarios: new[] { joao });

        var result = await svc.ImportFromCsvAsync(csvPath);

        Assert.Equal(4, result.TotalLinhas);
        Assert.Equal(1, result.UsuariosAtualizados);
        Assert.True(result.LinhasInvalidas >= 2);
        Assert.Single(joao.Perfis);
        Assert.Equal(PerfilUsuario.Administrador, joao.Perfis.First().Perfil);
    }

    [Fact]
    public async Task ImportFromCsv_SuperGestorComOutroPerfil_RegistraErroENaoAplica()
    {
        var joao = NovoUsuario(1, "joao");
        joao.DefinirPerfis(new[] { PerfilUsuario.Usuario });

        var (svc, _, uow, csvPath) = CriarComCsv(new[]
        {
            "Login;Perfis",
            "joao;SuperGestor,Administrador"
        }, usuarios: new[] { joao });

        var result = await svc.ImportFromCsvAsync(csvPath);

        Assert.Equal(1, result.LinhasInvalidas);
        Assert.Equal(0, result.UsuariosAtualizados);
        Assert.Single(joao.Perfis);
        Assert.Equal(PerfilUsuario.Usuario, joao.Perfis.First().Perfil);
        Assert.Equal(0, uow.Commits);
    }

    [Fact]
    public async Task ImportFromCsv_PerfisJaIguais_NaoExecutaCommit()
    {
        var joao = NovoUsuario(1, "joao");
        joao.DefinirPerfis(new[] { PerfilUsuario.Administrador });

        var (svc, _, uow, csvPath) = CriarComCsv(new[]
        {
            "Login;Perfis",
            "joao;Administrador"
        }, usuarios: new[] { joao });

        var result = await svc.ImportFromCsvAsync(csvPath);

        Assert.Equal(0, result.UsuariosAtualizados);
        Assert.Equal(0, uow.Commits);
    }

    private static Usuarios NovoUsuario(int id, string login) => new()
    {
        Id = id,
        Login = login,
        NomeCompleto = login,
        SenhaHash = "h"
    };

    private static (UsuarioPerfilImportService svc, FakeRepo repo, FakeUow uow, string csvPath) CriarComCsv(
        IEnumerable<string> linhas,
        Usuarios[] usuarios)
    {
        var csvPath = Path.Combine(Path.GetTempPath(), $"perfis_{Guid.NewGuid():N}.csv");
        File.WriteAllLines(csvPath, linhas);

        var repo = new FakeRepo();
        foreach (var u in usuarios) repo.Usuarios.Add(u);
        var uow = new FakeUow();
        var svc = new UsuarioPerfilImportService(repo, uow, NullLogger<UsuarioPerfilImportService>.Instance);
        return (svc, repo, uow, csvPath);
    }

    private sealed class FakeRepo : IUsuarioRepository
    {
        public List<Usuarios> Usuarios { get; } = new();
        public Task<Usuarios?> ObterPorLoginAsync(string login)
            => Task.FromResult(Usuarios.FirstOrDefault(u => string.Equals(u.Login, login, StringComparison.OrdinalIgnoreCase)));
        public void Update(Usuarios entidade) { }
        public Task AddAsync(Usuarios entidade) { Usuarios.Add(entidade); return Task.CompletedTask; }
        public void Delete(Usuarios entidade) { Usuarios.Remove(entidade); }
        public Task<Usuarios?> GetByIdAsync(int id) => Task.FromResult(Usuarios.FirstOrDefault(u => u.Id == id));
        public Task<IReadOnlyList<Usuarios>> GetAsync() => Task.FromResult<IReadOnlyList<Usuarios>>(Usuarios);
        public Task<Usuarios?> ObterComPlantasAsync(int id) => Task.FromResult(Usuarios.FirstOrDefault(u => u.Id == id));
        public Task<IReadOnlyList<Usuarios>> ListarComPlantasAsync() => Task.FromResult<IReadOnlyList<Usuarios>>(Usuarios);
        public Task<IReadOnlyList<Usuarios>> ListarPorPlantaAsync(int plantaId) => Task.FromResult<IReadOnlyList<Usuarios>>(Array.Empty<Usuarios>());
        public Task<IReadOnlyList<Plantas>> ObterPlantasDoUsuarioAsync(int usuarioId) => Task.FromResult<IReadOnlyList<Plantas>>(Array.Empty<Plantas>());
    }

    private sealed class FakeUow : IUnitOfWork
    {
        public int Commits { get; private set; }
        public Task CommitAsync(CancellationToken ct = default) { Commits++; return Task.CompletedTask; }
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
