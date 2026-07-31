using EToto.Domain.Entities;
using EToto.Domain.Enums;
using EToto.Domain.Interfaces;
using EToto.ImportTool;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace EToto.Application.Tests;

public class UsuarioVinculoImportServiceTests
{
    [Fact]
    public async Task ImportFromCsv_AtualizaTerceiroComEmpresaEValidade()
    {
        var maria = NovoUsuario(1, "maria");
        var (svc, _, _, csv) = CriarComCsv(new[]
        {
            "Login;TipoVinculo;NomeEmpresa;DataValidadeAcesso",
            "maria;Terceiro;ACME;2027-01-15"
        }, usuarios: new[] { maria });

        var r = await svc.ImportFromCsvAsync(csv);

        Assert.Equal(1, r.UsuariosAtualizados);
        Assert.Equal(0, r.LinhasInvalidas);
        Assert.Equal(TipoVinculo.Terceiro, maria.TipoVinculo);
        Assert.Equal("ACME", maria.NomeEmpresa);
        Assert.Equal(new DateTime(2027, 1, 15, 0, 0, 0, DateTimeKind.Utc), maria.DataValidadeAcesso);
    }

    [Fact]
    public async Task ImportFromCsv_TerceiroSemEmpresa_NaoAlteraBanco()
    {
        var joao = NovoUsuario(1, "joao");
        joao.TipoVinculo = TipoVinculo.Funcionario;

        var (svc, _, uow, csv) = CriarComCsv(new[]
        {
            "Login;TipoVinculo;NomeEmpresa;DataValidadeAcesso",
            "joao;Terceiro;;2027-01-15"
        }, usuarios: new[] { joao });

        var r = await svc.ImportFromCsvAsync(csv);

        Assert.Equal(1, r.LinhasInvalidas);
        Assert.Equal(0, r.UsuariosAtualizados);
        Assert.Equal(0, uow.Commits);
        Assert.Equal(TipoVinculo.Funcionario, joao.TipoVinculo);
    }

    [Fact]
    public async Task ImportFromCsv_Funcionario_LimpaCamposDeTerceiro()
    {
        var joao = NovoUsuario(1, "joao");
        joao.TipoVinculo = TipoVinculo.Terceiro;
        joao.NomeEmpresa = "Antiga";
        joao.DataValidadeAcesso = DateTime.UtcNow.AddDays(30);

        var (svc, _, _, csv) = CriarComCsv(new[]
        {
            "Login;TipoVinculo;NomeEmpresa;DataValidadeAcesso",
            "joao;Funcionario;;"
        }, usuarios: new[] { joao });

        var r = await svc.ImportFromCsvAsync(csv);

        Assert.Equal(1, r.UsuariosAtualizados);
        Assert.Equal(TipoVinculo.Funcionario, joao.TipoVinculo);
        Assert.Null(joao.NomeEmpresa);
        Assert.Null(joao.DataValidadeAcesso);
    }

    [Fact]
    public async Task ImportFromCsv_Idempotente_QuandoJaIgual()
    {
        var maria = NovoUsuario(1, "maria");
        maria.TipoVinculo = TipoVinculo.Terceiro;
        maria.NomeEmpresa = "ACME";
        maria.DataValidadeAcesso = new DateTime(2027, 1, 15, 0, 0, 0, DateTimeKind.Utc);

        var (svc, _, uow, csv) = CriarComCsv(new[]
        {
            "Login;TipoVinculo;NomeEmpresa;DataValidadeAcesso",
            "maria;Terceiro;ACME;2027-01-15"
        }, usuarios: new[] { maria });

        var r = await svc.ImportFromCsvAsync(csv);

        Assert.Equal(0, r.UsuariosAtualizados);
        Assert.Equal(0, uow.Commits);
    }

    [Fact]
    public async Task ImportFromCsv_TipoInvalido_RegistraErro()
    {
        var joao = NovoUsuario(1, "joao");
        var (svc, _, _, csv) = CriarComCsv(new[]
        {
            "Login;TipoVinculo;NomeEmpresa;DataValidadeAcesso",
            "joao;Estagiario;;"
        }, usuarios: new[] { joao });

        var r = await svc.ImportFromCsvAsync(csv);

        Assert.Equal(1, r.LinhasInvalidas);
        Assert.Equal(0, r.UsuariosAtualizados);
    }

    private static Usuarios NovoUsuario(int id, string login) => new()
    {
        Id = id, Login = login, NomeCompleto = login, SenhaHash = "h"
    };

    private static (UsuarioVinculoImportService svc, FakeRepo repo, FakeUow uow, string csvPath) CriarComCsv(
        IEnumerable<string> linhas, Usuarios[] usuarios)
    {
        var csvPath = Path.Combine(Path.GetTempPath(), $"vinculo_{Guid.NewGuid():N}.csv");
        File.WriteAllLines(csvPath, linhas);

        var repo = new FakeRepo();
        foreach (var u in usuarios) repo.Usuarios.Add(u);
        var uow = new FakeUow();
        var svc = new UsuarioVinculoImportService(repo, uow, NullLogger<UsuarioVinculoImportService>.Instance);
        return (svc, repo, uow, csvPath);
    }

    private sealed class FakeRepo : IUsuarioRepository
    {
        public List<Usuarios> Usuarios { get; } = new();
        public Task<Usuarios?> ObterPorLoginAsync(string login)
            => Task.FromResult(Usuarios.FirstOrDefault(u => string.Equals(u.Login, login, StringComparison.OrdinalIgnoreCase)));
        public void Update(Usuarios entidade) { }
        public Task AddAsync(Usuarios entidade) { Usuarios.Add(entidade); return Task.CompletedTask; }
        public void Delete(Usuarios entidade) { }
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
