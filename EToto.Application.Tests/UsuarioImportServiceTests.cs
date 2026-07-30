using EToto.Domain.Entities;
using EToto.Domain.Enums;
using EToto.Domain.Interfaces;
using EToto.ImportTool;
using Microsoft.Extensions.Logging.Abstractions;
using OfficeOpenXml;
using Xunit;

namespace EToto.Application.Tests;

public class UsuarioImportServiceTests
{
    public UsuarioImportServiceTests()
    {
        ExcelPackage.License.SetNonCommercialPersonal("EToto.Tests");
    }

    [Fact]
    public async Task DryRun_ContaSemGravar()
    {
        var (svc, repoUser, _) = CriarServico(
            plantas: new[] { new Plantas { Id = 1, Codigo = "PLA", Nome = "Planta A", Ativa = true } });

        var path = GerarXlsx(("PLA-Funcionarios", new[] { new[] { "Login", "NomeCompleto", "Perfil" },
                                                          new[] { "ana", "Ana Maria", "Usuario" } }));

        var r = await svc.ImportFromXlsxAsync(path, dryRun: true);

        Assert.True(r.DryRun);
        Assert.Equal(1, r.UsuariosCriados);
        Assert.Empty(repoUser.Usuarios); // nada gravado
    }

    [Fact]
    public async Task Gravacao_CriaUsuarioComPerfilEPlanta()
    {
        var (svc, repoUser, _) = CriarServico(
            plantas: new[] { new Plantas { Id = 1, Codigo = "PLA", Nome = "Planta A", Ativa = true } });

        var path = GerarXlsx(("PLA-Funcionarios", new[] { new[] { "Login", "NomeCompleto", "Perfil" },
                                                          new[] { "ana", "Ana Maria", "Administrador" } }));

        await svc.ImportFromXlsxAsync(path, dryRun: false);

        var u = Assert.Single(repoUser.Usuarios);
        Assert.Equal("ana", u.Login);
        Assert.Equal("Ana Maria", u.NomeCompleto);
        Assert.Equal(PerfilUsuario.Administrador, u.Perfil);
        Assert.Equal(1, u.PlantaId);
        Assert.Equal(TipoVinculo.Funcionario, u.TipoVinculo);
        Assert.Single(u.Perfis);
    }

    [Fact]
    public async Task AbaTerceiro_ExigeEmpresaEValidade()
    {
        var (svc, _, _) = CriarServico(
            plantas: new[] { new Plantas { Id = 1, Codigo = "PLA", Nome = "Planta A", Ativa = true } });

        var path = GerarXlsx(("PLA-Terceiros",
            new[]
            {
                new[] { "Login", "NomeCompleto", "Perfil", "NomeEmpresa", "DataValidadeAcesso" },
                new[] { "joao", "Joao", "Usuario", "", "" }
            }));

        var r = await svc.ImportFromXlsxAsync(path, dryRun: true);

        Assert.Equal(0, r.UsuariosCriados);
        Assert.True(r.LinhasInvalidas >= 1);
    }

    [Fact]
    public async Task AbaTerceiro_AceitaComEmpresaEValidade()
    {
        var (svc, repoUser, _) = CriarServico(
            plantas: new[] { new Plantas { Id = 1, Codigo = "PLA", Nome = "Planta A", Ativa = true } });

        var path = GerarXlsx(("PLA-Terceiros",
            new[]
            {
                new[] { "Login", "NomeCompleto", "Perfil", "NomeEmpresa", "DataValidadeAcesso" },
                new[] { "maria", "Maria T", "UsuarioFinal", "ACME", "2027-01-15" }
            }));

        await svc.ImportFromXlsxAsync(path, dryRun: false);

        var u = Assert.Single(repoUser.Usuarios);
        Assert.Equal(TipoVinculo.Terceiro, u.TipoVinculo);
        Assert.Equal("ACME", u.NomeEmpresa);
        Assert.NotNull(u.DataValidadeAcesso);
    }

    [Fact]
    public async Task DuplicidadeEntreAbas_MantemPrimeira()
    {
        var (svc, repoUser, _) = CriarServico(
            plantas: new[]
            {
                new Plantas { Id = 1, Codigo = "PLA", Nome = "A", Ativa = true },
                new Plantas { Id = 2, Codigo = "PLB", Nome = "B", Ativa = true }
            });

        var path = GerarXlsx(
            ("PLA-Funcionarios",
                new[] { new[] { "Login", "NomeCompleto" }, new[] { "joao", "Joao A" } }),
            ("PLB-Funcionarios",
                new[] { new[] { "Login", "NomeCompleto" }, new[] { "joao", "Joao B" } }));

        var r = await svc.ImportFromXlsxAsync(path, dryRun: false);

        Assert.Equal(1, r.UsuariosCriados);
        Assert.Equal(1, r.DuplicidadesEntreAbas);
        var u = Assert.Single(repoUser.Usuarios);
        Assert.Equal("Joao A", u.NomeCompleto);
        Assert.Equal(1, u.PlantaId); // veio da PLA
    }

    [Fact]
    public async Task Idempotente_RerodarNaoAlteraNada()
    {
        var (svc, _, _) = CriarServico(
            plantas: new[] { new Plantas { Id = 1, Codigo = "PLA", Nome = "A", Ativa = true } });

        var path = GerarXlsx(("PLA-Funcionarios",
            new[] { new[] { "Login", "NomeCompleto", "Perfil" },
                    new[] { "ana", "Ana", "Usuario" } }));

        await svc.ImportFromXlsxAsync(path, dryRun: false);
        var r2 = await svc.ImportFromXlsxAsync(path, dryRun: false);

        Assert.Equal(0, r2.UsuariosCriados);
        Assert.Equal(0, r2.UsuariosAtualizados);
        Assert.Equal(1, r2.UsuariosSemAlteracao);
    }

    [Fact]
    public async Task PlantaInexistente_RegistraErroEPulaAba()
    {
        var (svc, repoUser, _) = CriarServico(plantas: Array.Empty<Plantas>());

        var path = GerarXlsx(("FANTASMA-Funcionarios",
            new[] { new[] { "Login", "NomeCompleto" }, new[] { "x", "X" } }));

        var r = await svc.ImportFromXlsxAsync(path, dryRun: false);

        Assert.Empty(repoUser.Usuarios);
        Assert.NotEmpty(r.Erros);
    }

    [Fact]
    public async Task NomeAba_InvalidoIgnorado()
    {
        var (svc, _, _) = CriarServico(plantas: Array.Empty<Plantas>());

        var path = GerarXlsx(("AbaSemSeparador",
            new[] { new[] { "Login", "NomeCompleto" }, new[] { "x", "X" } }));

        var r = await svc.ImportFromXlsxAsync(path, dryRun: true);

        Assert.Equal(0, r.LinhasLidas);
        Assert.NotEmpty(r.Avisos);
    }

    private static string GerarXlsx(params (string aba, string[][] linhas)[] abas)
    {
        var path = Path.Combine(Path.GetTempPath(), $"import_{Guid.NewGuid():N}.xlsx");
        using var pkg = new ExcelPackage(new FileInfo(path));
        foreach (var (aba, linhas) in abas)
        {
            var ws = pkg.Workbook.Worksheets.Add(aba);
            for (int r = 0; r < linhas.Length; r++)
                for (int c = 0; c < linhas[r].Length; c++)
                    ws.Cells[r + 1, c + 1].Value = linhas[r][c];
        }
        pkg.Save();
        return path;
    }

    private static (UsuarioImportService svc, FakeUsuarioRepo repoUser, FakePlantaRepo repoPlanta) CriarServico(
        Plantas[] plantas)
    {
        var repoUser = new FakeUsuarioRepo();
        var repoPlanta = new FakePlantaRepo();
        repoPlanta.Plantas.AddRange(plantas);
        var uow = new FakeUow();
        var svc = new UsuarioImportService(repoUser, repoPlanta, uow, NullLogger<UsuarioImportService>.Instance);
        return (svc, repoUser, repoPlanta);
    }

    private sealed class FakeUsuarioRepo : IUsuarioRepository
    {
        public List<Usuarios> Usuarios { get; } = new();
        public Task<Usuarios?> ObterPorLoginAsync(string login)
            => Task.FromResult(Usuarios.FirstOrDefault(u => string.Equals(u.Login, login, StringComparison.OrdinalIgnoreCase)));
        public Task AddAsync(Usuarios entidade) { Usuarios.Add(entidade); return Task.CompletedTask; }
        public void Update(Usuarios entidade) { }
        public void Delete(Usuarios entidade) { }
        public Task<Usuarios?> GetByIdAsync(int id) => Task.FromResult(Usuarios.FirstOrDefault(u => u.Id == id));
        public Task<IReadOnlyList<Usuarios>> GetAsync() => Task.FromResult<IReadOnlyList<Usuarios>>(Usuarios);
        public Task<Usuarios?> ObterComPlantasAsync(int id) => Task.FromResult(Usuarios.FirstOrDefault(u => u.Id == id));
        public Task<IReadOnlyList<Usuarios>> ListarComPlantasAsync() => Task.FromResult<IReadOnlyList<Usuarios>>(Usuarios);
        public Task<IReadOnlyList<Usuarios>> ListarPorPlantaAsync(int plantaId) => Task.FromResult<IReadOnlyList<Usuarios>>(Array.Empty<Usuarios>());
        public Task<IReadOnlyList<Plantas>> ObterPlantasDoUsuarioAsync(int usuarioId) => Task.FromResult<IReadOnlyList<Plantas>>(Array.Empty<Plantas>());
    }

    private sealed class FakePlantaRepo : IPlantaRepository
    {
        public List<Plantas> Plantas { get; } = new();

        public Task<Plantas?> ObterPorCodigoAsync(string codigo)
            => Task.FromResult(Plantas.FirstOrDefault(p => string.Equals(p.Codigo, codigo, StringComparison.OrdinalIgnoreCase)));
        public Task<IReadOnlyList<Plantas>> ListarAtivasAsync() => Task.FromResult<IReadOnlyList<Plantas>>(Plantas);
        public Task<Plantas?> GetByIdAsync(int id) => Task.FromResult(Plantas.FirstOrDefault(p => p.Id == id));
        public Task AddAsync(Plantas entidade) { Plantas.Add(entidade); return Task.CompletedTask; }
        public void Update(Plantas entidade) { }
        public void Delete(Plantas entidade) { }
        public Task<IReadOnlyList<Plantas>> GetAsync() => Task.FromResult<IReadOnlyList<Plantas>>(Plantas);
    }

    private sealed class FakeUow : IUnitOfWork
    {
        public Task CommitAsync(CancellationToken ct = default) => Task.CompletedTask;
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
