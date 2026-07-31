using EToto.Domain.Entities;
using EToto.Domain.Enums;
using EToto.Domain.Interfaces;
using EToto.ImportTool;
using Microsoft.Extensions.Logging.Abstractions;
using OfficeOpenXml;
using Xunit;

namespace EToto.Application.Tests;

public class LideresLototoImportServiceTests
{
    public LideresLototoImportServiceTests()
    {
        ExcelPackage.License.SetNonCommercialPersonal("EToto.Tests");
    }

    [Fact]
    public async Task AbaFuncionarios_CriaUsuarioComPlantaCorreta()
    {
        var (svc, repoUser, _) = CriarServico(plantas: new[]
        {
            new Plantas { Id = 1, Codigo = "FARC", Nome = "Arcos", Ativa = true }
        });

        var path = GerarXlsx(("FARC", new object?[][]
        {
            new object?[] { "LÍDERES LOTOTO", null, null, null, null },
            new object?[] { "NOME DO COLABORADOR", "ÁREA", "EMAIL", "DATA DO TREINAMENTO", "Perfil de acesso" },
            new object?[] { "Cláudio Luiz Rosa", "Mecânica", "claudio.silva@etoto.com.br", new DateTime(2026, 1, 10), "Lider" }
        }));

        await svc.ImportFromXlsxAsync(path, dryRun: false);

        var u = Assert.Single(repoUser.Usuarios);
        Assert.Equal("claudio.silva", u.Login);
        Assert.Equal("Cláudio Luiz Rosa", u.NomeCompleto);
        Assert.Equal(TipoVinculo.Funcionario, u.TipoVinculo);
        Assert.Equal(1, u.PlantaId);
        Assert.Single(u.Perfis);
        Assert.Equal(PerfilUsuario.UsuarioFinal, u.Perfis.First().Perfil);
    }

    [Fact]
    public async Task PerfilComandoCentral_GeraDoisPerfis()
    {
        var (svc, repoUser, _) = CriarServico(plantas: new[]
        {
            new Plantas { Id = 1, Codigo = "FPIT", Nome = "Pitimbu", Ativa = true }
        });

        var path = GerarXlsx(("FPIT", new object?[][]
        {
            new object?[] { "LÍDERES LOTOTO", null, null, null, null },
            new object?[] { "NOME DO COLABORADOR", "ÁREA", "EMAIL", "DATA DO TREINAMENTO", "Perfil de acesso" },
            new object?[] { "Niedison Henrique", "Ensacadeira", "niedison.felismino@etoto.com.br", new DateTime(2026, 1, 5), "Comando Central e Lider" }
        }));

        await svc.ImportFromXlsxAsync(path, dryRun: false);

        var u = Assert.Single(repoUser.Usuarios);
        Assert.Equal(2, u.Perfis.Count);
        Assert.Contains(u.Perfis, p => p.Perfil == PerfilUsuario.UsuarioFinal);
        Assert.Contains(u.Perfis, p => p.Perfil == PerfilUsuario.ComandoCentral);
    }

    [Fact]
    public async Task AbaParceiros_GeraTerceiroComLoginNomeMeioESobrenome()
    {
        var (svc, repoUser, _) = CriarServico(plantas: new[]
        {
            new Plantas { Id = 1, Codigo = "FCTG", Nome = "Catanduvas", Ativa = true }
        });

        var path = GerarXlsx(("Parceiros FCTG", new object?[][]
        {
            new object?[] { "LÍDERES LOTOTO", null, null, null, null },
            new object?[] { "NOME DO COLABORADOR", "ÁREA", "Empresa", "DATA DO TREINAMENTO", "Perfil de acesso" },
            new object?[] { "ALEXANDRO MARTINS DA CONCEICAO", "Expedição", "Movex Movimentação", new DateTime(2026, 1, 11), "Lider" }
        }));

        var antes = DateTime.UtcNow;
        await svc.ImportFromXlsxAsync(path, dryRun: false);
        var depois = DateTime.UtcNow;

        var u = Assert.Single(repoUser.Usuarios);
        // Login = primeiro + segundo + último (sem DA): "alexandro.martins.conceicao"
        Assert.Equal("alexandro.martins.conceicao", u.Login);
        Assert.Equal(TipoVinculo.Terceiro, u.TipoVinculo);
        Assert.Equal("Movex Movimentação", u.NomeEmpresa);
        // Validade de ACESSO do Terceiro = data de criação + 6 meses (independe do treinamento).
        Assert.NotNull(u.DataValidadeAcesso);
        Assert.InRange(u.DataValidadeAcesso!.Value,
            antes.Date.AddMonths(6), depois.Date.AddMonths(6));
        // Validade de TREINAMENTO = data do treinamento + 2 anos (Parceiro e Funcionário).
        Assert.Equal(new DateTime(2028, 1, 11, 0, 0, 0, DateTimeKind.Utc), u.DataValidadeTreinamento);
    }

    [Theory]
    [InlineData("nâo possui e-mail")]
    [InlineData("Não possui email")]
    [InlineData("sem email")]
    [InlineData("n/a")]
    [InlineData("-")]
    [InlineData("sem arroba aqui")]
    public async Task FuncionarioComEmailInvalido_FazFallbackParaNome(string emailInvalido)
    {
        var (svc, repoUser, _) = CriarServico(plantas: new[]
        {
            new Plantas { Id = 1, Codigo = "FPIT", Nome = "Pitimbu", Ativa = true }
        });

        var path = GerarXlsx(("FPIT", new object?[][]
        {
            new object?[] { "LÍDERES LOTOTO", null, null, null, null },
            new object?[] { "NOME DO COLABORADOR", "ÁREA", "EMAIL", "DATA DO TREINAMENTO", "Perfil de acesso" },
            new object?[] { "MAIKY ALMEIDA GUIMARAES", "OP II", emailInvalido, new DateTime(2026, 1, 14), "Lider" }
        }));

        await svc.ImportFromXlsxAsync(path, dryRun: false);

        var u = Assert.Single(repoUser.Usuarios);
        Assert.Equal("maiky.almeida.guimaraes", u.Login);
    }

    [Fact]
    public async Task DuplicidadeEntreAbas_AvisoIncluiAbaLinhaNome()
    {
        var (svc, _, _) = CriarServico(plantas: new[]
        {
            new Plantas { Id = 1, Codigo = "FARC", Nome = "Arcos", Ativa = true },
            new Plantas { Id = 2, Codigo = "FPIT", Nome = "Pitimbu", Ativa = true }
        });

        var path = GerarXlsx(
            ("FARC", new object?[][]
            {
                new object?[] { "LÍDERES LOTOTO", null, null, null, null },
                new object?[] { "NOME DO COLABORADOR", "ÁREA", "EMAIL", "DATA DO TREINAMENTO", "Perfil de acesso" },
                new object?[] { "Joao Silva", "M", "joao.silva@etoto.com.br", new DateTime(2026,1,1), "Lider" }
            }),
            ("FPIT", new object?[][]
            {
                new object?[] { "LÍDERES LOTOTO", null, null, null, null },
                new object?[] { "NOME DO COLABORADOR", "ÁREA", "EMAIL", "DATA DO TREINAMENTO", "Perfil de acesso" },
                new object?[] { "João Silva", "M", "joao.silva@etoto.com.br", new DateTime(2026,1,1), "Lider" }
            }));

        var r = await svc.ImportFromXlsxAsync(path, dryRun: true);

        var aviso = Assert.Single(r.Avisos);
        Assert.Contains("FPIT", aviso);
        Assert.Contains("linha 3", aviso);
        Assert.Contains("João Silva", aviso);
        Assert.Contains("FARC", aviso);
        Assert.Contains("Joao Silva", aviso);
        Assert.Contains("joao.silva", aviso);
    }

    [Fact]
    public async Task SenhaColF_VaiComoHashSha256()
    {
        var (svc, repoUser, _) = CriarServico(plantas: new[]
        {
            new Plantas { Id = 1, Codigo = "FARC", Nome = "Arcos", Ativa = true }
        });

        var path = GerarXlsx(("FARC", new object?[][]
        {
            new object?[] { "LÍDERES LOTOTO", null, null, null, null, null },
            new object?[] { "NOME DO COLABORADOR", "ÁREA", "EMAIL", "DATA DO TREINAMENTO", "Perfil de acesso", "Senha" },
            new object?[] { "Cláudio Luiz Rosa", "Mec.", "claudio.silva@etoto.com.br", new DateTime(2026, 1, 10), "Lider", "C.ROSA" }
        }));

        await svc.ImportFromXlsxAsync(path, dryRun: false);

        var u = Assert.Single(repoUser.Usuarios);
        // SHA-256 hex lowercase de "C.ROSA"
        var esperado = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("C.ROSA"))).ToLowerInvariant();
        Assert.Equal(esperado, u.SenhaHash);
    }

    [Fact]
    public async Task SenhaVazia_MantemHashVazio()
    {
        var (svc, repoUser, _) = CriarServico(plantas: new[]
        {
            new Plantas { Id = 1, Codigo = "FARC", Nome = "Arcos", Ativa = true }
        });

        var path = GerarXlsx(("FARC", new object?[][]
        {
            new object?[] { "LÍDERES LOTOTO", null, null, null, null, null },
            new object?[] { "NOME DO COLABORADOR", "ÁREA", "EMAIL", "DATA DO TREINAMENTO", "Perfil de acesso", "Senha" },
            new object?[] { "Cláudio Luiz Rosa", "Mec.", "claudio.silva@etoto.com.br", new DateTime(2026, 1, 10), "Lider", null }
        }));

        await svc.ImportFromXlsxAsync(path, dryRun: false);

        var u = Assert.Single(repoUser.Usuarios);
        Assert.Equal("", u.SenhaHash);
    }

    [Fact]
    public async Task CriadoPorId_EhGravado_QuandoInformado()
    {
        var (svc, repoUser, _) = CriarServico(plantas: new[]
        {
            new Plantas { Id = 1, Codigo = "FARC", Nome = "Arcos", Ativa = true }
        });

        var path = GerarXlsx(("FARC", new object?[][]
        {
            new object?[] { "LÍDERES LOTOTO", null, null, null, null },
            new object?[] { "NOME DO COLABORADOR", "ÁREA", "EMAIL", "DATA DO TREINAMENTO", "Perfil de acesso" },
            new object?[] { "Cláudio Luiz Rosa", "Mec.", "claudio.silva@etoto.com.br", new DateTime(2026, 1, 10), "Lider" }
        }));

        await svc.ImportFromXlsxAsync(path, dryRun: false, criadoPorId: 42);

        var u = Assert.Single(repoUser.Usuarios);
        Assert.Equal(42, u.CriadoPorId);
    }

    [Fact]
    public async Task FuncionarioSemEmail_FazFallbackParaNome()
    {
        var (svc, repoUser, _) = CriarServico(plantas: new[]
        {
            new Plantas { Id = 1, Codigo = "FCTG", Nome = "Catanduvas", Ativa = true }
        });

        var path = GerarXlsx(("FCTG", new object?[][]
        {
            new object?[] { "LÍDERES LOTOTO", null, null, null, null },
            new object?[] { "NOME DO COLABORADOR", "ÁREA", "EMAIL", "DATA DO TREINAMENTO", "Perfil de acesso" },
            new object?[] { "THIAGO TEIXEIRA BONAFE", "PCM", null, new DateTime(2026, 1, 14), "Lider" }
        }));

        await svc.ImportFromXlsxAsync(path, dryRun: false);

        var u = Assert.Single(repoUser.Usuarios);
        Assert.Equal("thiago.teixeira.bonafe", u.Login);
        Assert.Equal(TipoVinculo.Funcionario, u.TipoVinculo);
    }

    [Fact]
    public async Task ValidadeTreinamentoFuncionario_DataMaisDoisAnos_SemValidadeAcesso()
    {
        var (svc, repoUser, _) = CriarServico(plantas: new[]
        {
            new Plantas { Id = 1, Codigo = "FMTZ", Nome = "Mtz", Ativa = true }
        });

        var data = new DateTime(2026, 3, 15);
        var path = GerarXlsx(("FMTZ", new object?[][]
        {
            new object?[] { "LÍDERES LOTOTO", null, null, null, null },
            new object?[] { "NOME DO COLABORADOR", "ÁREA", "EMAIL", "DATA DO TREINAMENTO", "Perfil de acesso" },
            new object?[] { "Adilson Xavier", "Mecanica", "adilson.xavier@etoto.com.br", data, "Lider" }
        }));

        await svc.ImportFromXlsxAsync(path, dryRun: false);

        var u = Assert.Single(repoUser.Usuarios);
        Assert.Equal(data.Date, u.DataTreinamento?.Date);
        // Funcionário: validade de treinamento = data + 2 anos; sem validade de acesso.
        Assert.Equal(data.AddYears(2).Date, u.DataValidadeTreinamento?.Date);
        Assert.Null(u.DataValidadeAcesso);
    }

    [Fact]
    public async Task TerceiroSemTreinamento_AindaCriaComAcesso6Meses()
    {
        var (svc, repoUser, _) = CriarServico(plantas: new[]
        {
            new Plantas { Id = 1, Codigo = "FCTG", Nome = "Catanduvas", Ativa = true }
        });

        // Sem data de treinamento (col D nula): Terceiro continua válido — exige só a empresa.
        var path = GerarXlsx(("Parceiros FCTG", new object?[][]
        {
            new object?[] { "LÍDERES LOTOTO", null, null, null, null },
            new object?[] { "NOME DO COLABORADOR", "ÁREA", "Empresa", "DATA DO TREINAMENTO", "Perfil de acesso" },
            new object?[] { "Carlos Souza Lima", "Expedição", "Movex", null, "Lider" }
        }));

        var antes = DateTime.UtcNow;
        var r = await svc.ImportFromXlsxAsync(path, dryRun: false);
        var depois = DateTime.UtcNow;

        Assert.Equal(0, r.LinhasInvalidas);
        var u = Assert.Single(repoUser.Usuarios);
        Assert.Equal(TipoVinculo.Terceiro, u.TipoVinculo);
        Assert.False(u.TreinamentoConcluido);
        Assert.Null(u.DataValidadeTreinamento);
        Assert.NotNull(u.DataValidadeAcesso);
        Assert.InRange(u.DataValidadeAcesso!.Value,
            antes.Date.AddMonths(6), depois.Date.AddMonths(6));
    }

    [Fact]
    public async Task SerialExcelComoNumero_ConverteCorretamente()
    {
        var (svc, repoUser, _) = CriarServico(plantas: new[]
        {
            new Plantas { Id = 1, Codigo = "FARC", Nome = "Arcos", Ativa = true }
        });

        // 46042 = 2026-01-20 no calendário do Excel (1900-base)
        var path = GerarXlsx(("FARC", new object?[][]
        {
            new object?[] { "LÍDERES LOTOTO", null, null, null, null },
            new object?[] { "NOME DO COLABORADOR", "ÁREA", "EMAIL", "DATA DO TREINAMENTO", "Perfil de acesso" },
            new object?[] { "Cláudio Luiz Rosa", "Mecânica", "claudio.silva@etoto.com.br", 46042.0, "Lider" }
        }));

        await svc.ImportFromXlsxAsync(path, dryRun: false);

        var u = Assert.Single(repoUser.Usuarios);
        Assert.Equal(new DateTime(2026, 1, 20).Date, u.DataTreinamento?.Date);
    }

    [Fact]
    public async Task DuplicidadeEntreAbas_MantemPrimeira()
    {
        var (svc, repoUser, _) = CriarServico(plantas: new[]
        {
            new Plantas { Id = 1, Codigo = "FARC", Nome = "Arcos", Ativa = true },
            new Plantas { Id = 2, Codigo = "FPIT", Nome = "Pitimbu", Ativa = true }
        });

        var path = GerarXlsx(
            ("FARC", new object?[][]
            {
                new object?[] { "LÍDERES LOTOTO", null, null, null, null },
                new object?[] { "NOME DO COLABORADOR", "ÁREA", "EMAIL", "DATA DO TREINAMENTO", "Perfil de acesso" },
                new object?[] { "João Silva", "M", "joao.silva@etoto.com.br", new DateTime(2026, 1, 1), "Lider" }
            }),
            ("FPIT", new object?[][]
            {
                new object?[] { "LÍDERES LOTOTO", null, null, null, null },
                new object?[] { "NOME DO COLABORADOR", "ÁREA", "EMAIL", "DATA DO TREINAMENTO", "Perfil de acesso" },
                new object?[] { "Joao Silva", "M", "joao.silva@etoto.com.br", new DateTime(2026, 1, 1), "Lider" }
            }));

        var r = await svc.ImportFromXlsxAsync(path, dryRun: false);

        Assert.Equal(1, r.UsuariosCriados);
        Assert.Equal(1, r.DuplicidadesEntreAbas);
        var u = Assert.Single(repoUser.Usuarios);
        Assert.Equal(1, u.PlantaId); // veio da FARC
    }

    [Fact]
    public async Task Idempotente_RerodarNaoAlteraNada()
    {
        var (svc, _, _) = CriarServico(plantas: new[]
        {
            new Plantas { Id = 1, Codigo = "FARC", Nome = "Arcos", Ativa = true }
        });

        var path = GerarXlsx(("FARC", new object?[][]
        {
            new object?[] { "LÍDERES LOTOTO", null, null, null, null },
            new object?[] { "NOME DO COLABORADOR", "ÁREA", "EMAIL", "DATA DO TREINAMENTO", "Perfil de acesso" },
            new object?[] { "Cláudio Luiz Rosa", "Mecânica", "claudio.silva@etoto.com.br", new DateTime(2026, 1, 10), "Lider" }
        }));

        await svc.ImportFromXlsxAsync(path, dryRun: false);
        var r2 = await svc.ImportFromXlsxAsync(path, dryRun: false);

        Assert.Equal(0, r2.UsuariosCriados);
        Assert.Equal(0, r2.UsuariosAtualizados);
        Assert.Equal(1, r2.UsuariosSemAlteracao);
    }

    [Fact]
    public async Task PlantaInexistente_RegistraErro()
    {
        var (svc, repoUser, _) = CriarServico(plantas: Array.Empty<Plantas>());

        var path = GerarXlsx(("FARC", new object?[][]
        {
            new object?[] { "LÍDERES LOTOTO", null, null, null, null },
            new object?[] { "NOME DO COLABORADOR", "ÁREA", "EMAIL", "DATA DO TREINAMENTO", "Perfil de acesso" },
            new object?[] { "X", "M", "x@etoto.com.br", new DateTime(2026, 1, 1), "Lider" }
        }));

        var r = await svc.ImportFromXlsxAsync(path, dryRun: false);

        Assert.Empty(repoUser.Usuarios);
        Assert.NotEmpty(r.Erros);
    }

    private static string GerarXlsx(params (string aba, object?[][] linhas)[] abas)
    {
        var path = Path.Combine(Path.GetTempPath(), $"lideres_{Guid.NewGuid():N}.xlsx");
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

    private static (LideresLototoImportService svc, FakeUsuarioRepo repoUser, FakePlantaRepo repoPlanta) CriarServico(
        Plantas[] plantas)
    {
        var repoUser = new FakeUsuarioRepo();
        var repoPlanta = new FakePlantaRepo();
        repoPlanta.Plantas.AddRange(plantas);
        var uow = new FakeUow();
        var svc = new LideresLototoImportService(repoUser, repoPlanta, uow, NullLogger<LideresLototoImportService>.Instance);
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
