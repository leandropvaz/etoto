using EToto.Application.Dto;
using EToto.Application.Services;
using EToto.Domain.Entities;
using EToto.Domain.Enums;
using EToto.Domain.Interfaces;
using Xunit;

namespace EToto.Application.Tests;

public class UsuariosServiceTests
{
    [Fact]
    public async Task CriarAsync_ComMultiplosPerfis_ChamaDefinirPerfis_ESincronizaCampoLegado()
    {
        var (repo, uow, svc) = CriarServico();

        var dto = new UsuariosDto
        {
            Login = "joao",
            NomeCompleto = "João",
            Senha = "x",
            Perfis = new() { (int)PerfilUsuario.Administrador, (int)PerfilUsuario.UsuarioFinal },
            ExecutadoPorId = 99,
            Ativa = true
        };

        await svc.CriarAsync(dto);

        Assert.Single(repo.Adicionados);
        var entidade = repo.Adicionados[0];
        Assert.Equal(2, entidade.Perfis.Count);
        Assert.Contains(entidade.Perfis, p => p.Perfil == PerfilUsuario.Administrador);
        Assert.Contains(entidade.Perfis, p => p.Perfil == PerfilUsuario.UsuarioFinal);
        Assert.Equal(99, entidade.CriadoPorId);
        Assert.NotNull(entidade.CriadoEm);
        Assert.True(entidade.Perfil == PerfilUsuario.Administrador || entidade.Perfil == PerfilUsuario.UsuarioFinal);
    }

    [Fact]
    public async Task CriarAsync_SuperGestorComOutroPerfil_LancaInvalidOperation()
    {
        var (_, _, svc) = CriarServico();

        var dto = new UsuariosDto
        {
            Login = "x",
            NomeCompleto = "X",
            Senha = "x",
            Perfis = new() { (int)PerfilUsuario.SuperGestor, (int)PerfilUsuario.Administrador }
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => svc.CriarAsync(dto));
    }

    [Fact]
    public async Task CriarAsync_SuperGestorSozinho_PerfilLegadoIgualSuperGestor()
    {
        var (repo, _, svc) = CriarServico();

        await svc.CriarAsync(new UsuariosDto
        {
            Login = "chefe",
            NomeCompleto = "Chefe",
            Senha = "x",
            Perfis = new() { (int)PerfilUsuario.SuperGestor }
        });

        Assert.Equal(PerfilUsuario.SuperGestor, repo.Adicionados[0].Perfil);
    }

    [Fact]
    public async Task CriarAsync_SemPerfis_CaiParaPerfilLegado()
    {
        var (repo, _, svc) = CriarServico();

        await svc.CriarAsync(new UsuariosDto
        {
            Login = "compat",
            NomeCompleto = "Compat",
            Senha = "x",
            Perfil = (int)PerfilUsuario.Usuario,
            Perfis = new() // vazio
        });

        Assert.Single(repo.Adicionados[0].Perfis);
        Assert.Equal(PerfilUsuario.Usuario, repo.Adicionados[0].Perfis.First().Perfil);
    }

    [Fact]
    public async Task AtualizarAsync_RegistraAlteradoPorIdEEm()
    {
        var existente = new Usuarios
        {
            Id = 10,
            Login = "joao",
            NomeCompleto = "João",
            SenhaHash = "h",
            Perfil = PerfilUsuario.Usuario
        };
        existente.DefinirPerfis(new[] { PerfilUsuario.Usuario });

        var (_, _, svc) = CriarServico(comUsuarioExistente: existente);

        var dto = new UsuariosDto
        {
            Id = 10,
            Login = "joao",
            NomeCompleto = "João Atualizado",
            Senha = "",
            Perfis = new() { (int)PerfilUsuario.UsuarioFinal, (int)PerfilUsuario.ComandoCentral },
            ExecutadoPorId = 5,
            Ativa = true
        };

        await svc.AtualizarAsync(dto);

        Assert.Equal(5, existente.AlteradoPorId);
        Assert.NotNull(existente.AlteradoEm);
        Assert.Equal(2, existente.Perfis.Count);
        Assert.Contains(existente.Perfis, p => p.Perfil == PerfilUsuario.UsuarioFinal);
        Assert.Contains(existente.Perfis, p => p.Perfil == PerfilUsuario.ComandoCentral);
    }

    [Fact]
    public async Task CriarAsync_PropagaDataValidadeTreinamento()
    {
        var (repo, _, svc) = CriarServico();
        var validade = new DateTime(2027, 5, 10, 0, 0, 0, DateTimeKind.Utc);

        await svc.CriarAsync(new UsuariosDto
        {
            Login = "lider",
            NomeCompleto = "Líder",
            Senha = "x",
            Perfis = new() { (int)PerfilUsuario.UsuarioFinal },
            TreinamentoConcluido = true,
            DataTreinamento = new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc),
            DataValidadeTreinamento = validade
        });

        Assert.Equal(validade, repo.Adicionados[0].DataValidadeTreinamento);
    }

    [Fact]
    public async Task AtualizarAsync_AtualizaDataValidadeTreinamento()
    {
        var existente = new Usuarios
        {
            Id = 7, Login = "lider", NomeCompleto = "Líder",
            SenhaHash = "h", Perfil = PerfilUsuario.UsuarioFinal,
            TreinamentoConcluido = true,
            DataValidadeTreinamento = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        existente.DefinirPerfis(new[] { PerfilUsuario.UsuarioFinal });

        var (_, _, svc) = CriarServico(comUsuarioExistente: existente);

        var novaValidade = new DateTime(2027, 7, 1, 0, 0, 0, DateTimeKind.Utc);
        await svc.AtualizarAsync(new UsuariosDto
        {
            Id = 7, Login = "lider", NomeCompleto = "Líder", Senha = "",
            Perfis = new() { (int)PerfilUsuario.UsuarioFinal },
            TreinamentoConcluido = true,
            DataValidadeTreinamento = novaValidade
        });

        Assert.Equal(novaValidade, existente.DataValidadeTreinamento);
    }

    [Fact]
    public async Task AtualizarAsync_SemSenhaNova_NaoTrocaSenhaHash()
    {
        var existente = new Usuarios
        {
            Id = 1, Login = "x", NomeCompleto = "X",
            SenhaHash = "HASH-ORIGINAL", Perfil = PerfilUsuario.Usuario
        };
        existente.DefinirPerfis(new[] { PerfilUsuario.Usuario });

        var (_, _, svc) = CriarServico(comUsuarioExistente: existente);

        await svc.AtualizarAsync(new UsuariosDto
        {
            Id = 1, Login = "x", NomeCompleto = "X2",
            Senha = "", Perfis = new() { (int)PerfilUsuario.Usuario }
        });

        Assert.Equal("HASH-ORIGINAL", existente.SenhaHash);
    }

    private static (FakeUsuarioRepo, FakeUow, UsuariosService) CriarServico(Usuarios? comUsuarioExistente = null)
    {
        var repo = new FakeUsuarioRepo();
        if (comUsuarioExistente is not null)
            repo.Existente = comUsuarioExistente;
        var uow = new FakeUow();
        var svc = new UsuariosService(repo, uow);
        return (repo, uow, svc);
    }

    private sealed class FakeUsuarioRepo : IUsuarioRepository
    {
        public List<Usuarios> Adicionados { get; } = new();
        public Usuarios? Existente { get; set; }

        public Task AddAsync(Usuarios entidade) { Adicionados.Add(entidade); return Task.CompletedTask; }
        public void Update(Usuarios entidade) { }
        public void Delete(Usuarios entidade) { }

        public Task<Usuarios?> GetByIdAsync(int id) => Task.FromResult<Usuarios?>(Existente?.Id == id ? Existente : null);
        public Task<Usuarios?> ObterComPlantasAsync(int id) => Task.FromResult<Usuarios?>(Existente?.Id == id ? Existente : null);
        public Task<Usuarios?> ObterPorLoginAsync(string login) => Task.FromResult<Usuarios?>(Existente?.Login == login ? Existente : null);

        public Task<IReadOnlyList<Usuarios>> GetAsync() => Task.FromResult<IReadOnlyList<Usuarios>>(Array.Empty<Usuarios>());
        public Task<IReadOnlyList<Usuarios>> ListarComPlantasAsync() => Task.FromResult<IReadOnlyList<Usuarios>>(Array.Empty<Usuarios>());
        public Task<IReadOnlyList<Usuarios>> ListarPorPlantaAsync(int plantaId) => Task.FromResult<IReadOnlyList<Usuarios>>(Array.Empty<Usuarios>());
        public Task<IReadOnlyList<Plantas>> ObterPlantasDoUsuarioAsync(int usuarioId) => Task.FromResult<IReadOnlyList<Plantas>>(Array.Empty<Plantas>());
    }

    private sealed class FakeUow : IUnitOfWork
    {
        public int Commits { get; private set; }
        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            Commits++;
            return Task.CompletedTask;
        }
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
