using EToto.Application.Dto;
using EToto.Application.Services;
using EToto.Domain.Entities;
using EToto.Domain.Enums;
using EToto.Domain.Interfaces;
using Xunit;

namespace EToto.Application.Tests;

public class UsuariosServiceVinculoTests
{
    [Fact]
    public async Task CriarAsync_Terceiro_SemEmpresa_Lanca()
    {
        var (_, _, svc) = CriarServico();

        var dto = NovoDto(perfil: PerfilUsuario.UsuarioFinal);
        dto.TipoVinculo = (int)TipoVinculo.Terceiro;
        dto.NomeEmpresa = null;
        dto.DataValidadeAcesso = DateTime.UtcNow.AddDays(60);

        await Assert.ThrowsAsync<InvalidOperationException>(() => svc.CriarAsync(dto));
    }

    [Fact]
    public async Task CriarAsync_Terceiro_SemValidade_Lanca()
    {
        var (_, _, svc) = CriarServico();

        var dto = NovoDto(perfil: PerfilUsuario.UsuarioFinal);
        dto.TipoVinculo = (int)TipoVinculo.Terceiro;
        dto.NomeEmpresa = "ACME";
        dto.DataValidadeAcesso = null;

        await Assert.ThrowsAsync<InvalidOperationException>(() => svc.CriarAsync(dto));
    }

    [Fact]
    public async Task CriarAsync_Terceiro_PreencheCampos()
    {
        var (repo, _, svc) = CriarServico();
        var validade = DateTime.UtcNow.AddDays(60);

        var dto = NovoDto(perfil: PerfilUsuario.UsuarioFinal);
        dto.TipoVinculo = (int)TipoVinculo.Terceiro;
        dto.NomeEmpresa = "ACME";
        dto.DataValidadeAcesso = validade;

        await svc.CriarAsync(dto);

        var entidade = repo.Adicionados[0];
        Assert.Equal(TipoVinculo.Terceiro, entidade.TipoVinculo);
        Assert.Equal("ACME", entidade.NomeEmpresa);
        Assert.Equal(validade, entidade.DataValidadeAcesso);
    }

    [Fact]
    public async Task CriarAsync_Funcionario_LimpaCamposDeTerceiro()
    {
        var (repo, _, svc) = CriarServico();

        var dto = NovoDto(perfil: PerfilUsuario.Usuario);
        dto.TipoVinculo = (int)TipoVinculo.Funcionario;
        // Caller até preencheu, mas o serviço deve ignorar para Funcionário.
        dto.NomeEmpresa = "deveria-ser-ignorado";
        dto.DataValidadeAcesso = DateTime.UtcNow.AddDays(60);

        await svc.CriarAsync(dto);

        var entidade = repo.Adicionados[0];
        Assert.Equal(TipoVinculo.Funcionario, entidade.TipoVinculo);
        Assert.Null(entidade.NomeEmpresa);
        Assert.Null(entidade.DataValidadeAcesso);
    }

    [Fact]
    public async Task AtualizarAsync_Funcionario_LimpaCamposDeTerceiro()
    {
        var existente = new Usuarios
        {
            Id = 1, Login = "x", NomeCompleto = "X", SenhaHash = "h",
            TipoVinculo = TipoVinculo.Terceiro,
            NomeEmpresa = "Antiga",
            DataValidadeAcesso = DateTime.UtcNow.AddDays(60)
        };
        existente.DefinirPerfis(new[] { PerfilUsuario.UsuarioFinal });

        var (_, _, svc) = CriarServico(comUsuarioExistente: existente);

        await svc.AtualizarAsync(new UsuariosDto
        {
            Id = 1, Login = "x", NomeCompleto = "X", Senha = "",
            Perfis = new() { (int)PerfilUsuario.UsuarioFinal },
            TipoVinculo = (int)TipoVinculo.Funcionario
        });

        Assert.Equal(TipoVinculo.Funcionario, existente.TipoVinculo);
        Assert.Null(existente.NomeEmpresa);
        Assert.Null(existente.DataValidadeAcesso);
    }

    private static UsuariosDto NovoDto(PerfilUsuario perfil) => new()
    {
        Login = "user",
        NomeCompleto = "User",
        Senha = "x",
        Ativa = true,
        Perfis = new() { (int)perfil }
    };

    private static (FakeUsuarioRepo, FakeUow, UsuariosService) CriarServico(Usuarios? comUsuarioExistente = null)
    {
        var repo = new FakeUsuarioRepo();
        if (comUsuarioExistente is not null) repo.Existente = comUsuarioExistente;
        var uow = new FakeUow();
        return (repo, uow, new UsuariosService(repo, uow));
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
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
