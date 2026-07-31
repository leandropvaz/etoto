using EToto.Application.Dto;
using EToto.Application.Interfaces;
using EToto.Application.Services;
using EToto.Domain.Entities;
using EToto.Domain.Enums;
using EToto.Domain.Interfaces;
using Xunit;

namespace EToto.Application.Tests;

public class CampanhaRevalidacaoServiceTests
{
    [Fact]
    public async Task CriarCampanha_PopulaItensComUsuariosAtivos_ENotificaGestores()
    {
        var ana = NovoUsuario(1, "ana", ativa: true);
        ana.DefinirPerfis(new[] { PerfilUsuario.UsuarioFinal });
        var bia = NovoUsuario(2, "bia", ativa: true);
        bia.DefinirPerfis(new[] { PerfilUsuario.Administrador });
        var inactiva = NovoUsuario(3, "ina", ativa: false);

        var (svc, repoCamp, _, email) = NovoServico(ana, bia, inactiva);

        var id = await svc.CriarCampanhaAsync(new CriarCampanhaDto
        {
            Nome = "Q3 2026", PlantaIds = new() { 1 }, DataFimPrevista = PrazoFuturo, ExecutadoPorId = 99
        });

        var (campanhaDto, itens) = await svc.ObterDetalheAsync(id);
        Assert.NotNull(campanhaDto);
        Assert.Equal((int)StatusCampanha.EmAndamento, campanhaDto!.Status);
        Assert.Equal(2, itens.Count); // só 2 ativos
        Assert.Contains(itens, i => i.UsuarioLogin == "ana");
        Assert.Contains(itens, i => i.UsuarioLogin == "bia");

        // bia é Administradora → recebeu o e-mail.
        var msg = Assert.Single(email.Enviados);
        Assert.Contains("bia", msg.Destinatarios);
    }

    [Fact]
    public async Task CriarCampanha_UsaDataFimPrevistaInformada()
    {
        var (svc, _, _, _) = NovoServico(NovoUsuario(1, "ana", true));

        await svc.CriarCampanhaAsync(new CriarCampanhaDto
        {
            Nome = "Com prazo", PlantaIds = new() { 1 }, DataFimPrevista = PrazoFuturo, ExecutadoPorId = 1
        });

        var detalhes = await svc.ObterDetalheAsync(1);
        Assert.Equal(PrazoFuturo, detalhes.Campanha!.DataFimPrevista);
    }

    [Fact]
    public async Task CriarCampanha_FiltraUsuariosPelaPlanta()
    {
        var aDaPlanta1 = NovoUsuario(1, "ana", ativa: true, plantaId: 1);
        var bDaPlanta2 = NovoUsuario(2, "bia", ativa: true, plantaId: 2);
        var (svc, _, _, _) = NovoServico(aDaPlanta1, bDaPlanta2);

        var id = await svc.CriarCampanhaAsync(new CriarCampanhaDto
        {
            Nome = "Só planta 1", PlantaIds = new() { 1 }, DataFimPrevista = PrazoFuturo, ExecutadoPorId = 1
        });

        var (_, itens) = await svc.ObterDetalheAsync(id);
        var item = Assert.Single(itens);
        Assert.Equal("ana", item.UsuarioLogin);
    }

    [Fact]
    public async Task CriarCampanha_SemPlanta_Lanca()
    {
        var (svc, _, _, _) = NovoServico(NovoUsuario(1, "ana", true));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.CriarCampanhaAsync(new CriarCampanhaDto { Nome = "X", DataFimPrevista = PrazoFuturo }));
    }

    [Fact]
    public async Task DecidirItem_Revogar_InativaUsuario()
    {
        var ana = NovoUsuario(1, "ana", ativa: true);
        var (svc, _, repoUser, _) = NovoServico(ana);
        var id = await svc.CriarCampanhaAsync(new CriarCampanhaDto
        {
            Nome = "Camp", PlantaIds = new() { 1 }, DataFimPrevista = PrazoFuturo, ExecutadoPorId = 99
        });
        var (_, itens) = await svc.ObterDetalheAsync(id);
        var item = itens.Single();

        await svc.DecidirItemAsync(new DecidirItemDto
        {
            CampanhaId = id,
            ItemId = item.Id,
            Decisao = (int)DecisaoRevisao.Revogar,
            ExecutadoPorId = 99,
            Observacao = "desligado da empresa"
        });

        Assert.False(repoUser.Usuarios.Single(u => u.Login == "ana").Ativa);
    }

    [Fact]
    public async Task DecidirItem_Manter_NaoInativaUsuario()
    {
        var ana = NovoUsuario(1, "ana", ativa: true);
        var (svc, _, repoUser, _) = NovoServico(ana);
        var id = await svc.CriarCampanhaAsync(new CriarCampanhaDto
        {
            Nome = "Camp", PlantaIds = new() { 1 }, DataFimPrevista = PrazoFuturo, ExecutadoPorId = 99
        });
        var item = (await svc.ObterDetalheAsync(id)).Itens.Single();

        await svc.DecidirItemAsync(new DecidirItemDto
        {
            CampanhaId = id, ItemId = item.Id, Decisao = (int)DecisaoRevisao.Manter, ExecutadoPorId = 99
        });

        Assert.True(repoUser.Usuarios.Single(u => u.Login == "ana").Ativa);
        var atualizado = (await svc.ObterDetalheAsync(id)).Itens.Single();
        Assert.Equal((int)DecisaoRevisao.Manter, atualizado.Decisao);
        Assert.Equal(99, atualizado.DecididoPorId);
        Assert.NotNull(atualizado.DecididoEm);
    }

    [Fact]
    public async Task Concluir_ChangeStatus_ESalvaDataFimReal()
    {
        var (svc, _, _, _) = NovoServico(NovoUsuario(1, "ana", true));
        var id = await svc.CriarCampanhaAsync(new CriarCampanhaDto
        {
            Nome = "C", PlantaIds = new() { 1 }, DataFimPrevista = PrazoFuturo, ExecutadoPorId = 1
        });

        await svc.ConcluirAsync(id);

        var c = (await svc.ObterDetalheAsync(id)).Campanha!;
        Assert.Equal((int)StatusCampanha.Concluida, c.Status);
        Assert.NotNull(c.DataFimReal);
    }

    [Fact]
    public async Task Cancelar_MudaStatusParaCancelada()
    {
        var (svc, _, _, _) = NovoServico(NovoUsuario(1, "ana", true));
        var id = await svc.CriarCampanhaAsync(new CriarCampanhaDto
        {
            Nome = "C", PlantaIds = new() { 1 }, DataFimPrevista = PrazoFuturo, ExecutadoPorId = 1
        });

        await svc.CancelarAsync(id);

        var c = (await svc.ObterDetalheAsync(id)).Campanha!;
        Assert.Equal((int)StatusCampanha.Cancelada, c.Status);
        Assert.NotNull(c.DataFimReal);
    }

    [Fact]
    public async Task Cancelar_CampanhaConcluida_Lanca()
    {
        var (svc, _, _, _) = NovoServico(NovoUsuario(1, "ana", true));
        var id = await svc.CriarCampanhaAsync(new CriarCampanhaDto
        {
            Nome = "C", PlantaIds = new() { 1 }, DataFimPrevista = PrazoFuturo, ExecutadoPorId = 1
        });
        await svc.ConcluirAsync(id);

        await Assert.ThrowsAsync<InvalidOperationException>(() => svc.CancelarAsync(id));
    }

    [Fact]
    public async Task CriarCampanha_NomeVazio_Lanca()
    {
        var (svc, _, _, _) = NovoServico(NovoUsuario(1, "ana", true));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.CriarCampanhaAsync(new CriarCampanhaDto { Nome = "" }));
    }

    private static Usuarios NovoUsuario(int id, string login, bool ativa, int plantaId = 1) => new()
    {
        Id = id, Login = login, NomeCompleto = login, SenhaHash = "h", Ativa = ativa,
        PlantasAssociadas = new List<UsuarioPlanta>
        {
            new() { UsuarioId = id, PlantaId = plantaId }
        }
    };

    // Data de conclusão futura padrão para os testes.
    private static readonly DateTime PrazoFuturo = new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static (CampanhaRevalidacaoService svc,
                    FakeCampanhaRepo repoCamp,
                    FakeUsuarioRepo repoUser,
                    FakeEmail email) NovoServico(params Usuarios[] usuarios)
    {
        var repoCamp = new FakeCampanhaRepo();
        var repoUser = new FakeUsuarioRepo();
        repoUser.Usuarios.AddRange(usuarios);
        repoCamp.UsuarioRepo = repoUser;
        var uow = new FakeUow();
        var email = new FakeEmail();
        var svc = new CampanhaRevalidacaoService(repoCamp, repoUser, uow, email);
        return (svc, repoCamp, repoUser, email);
    }

    private sealed class FakeCampanhaRepo : ICampanhaRepository
    {
        public List<CampanhaRevalidacao> Campanhas { get; } = new();
        public FakeUsuarioRepo? UsuarioRepo { get; set; }
        private int _nextCampId = 1;
        private int _nextItemId = 1;

        public Task AdicionarAsync(CampanhaRevalidacao campanha, CancellationToken ct = default)
        {
            campanha.Id = _nextCampId++;
            foreach (var it in campanha.Itens)
            {
                it.Id = _nextItemId++;
                it.CampanhaId = campanha.Id;
                if (UsuarioRepo is not null)
                    it.Usuario = UsuarioRepo.Usuarios.FirstOrDefault(u => u.Id == it.UsuarioId);
            }
            Campanhas.Add(campanha);
            return Task.CompletedTask;
        }

        public void Atualizar(CampanhaRevalidacao campanha) { }
        public void AtualizarItem(ItemCampanhaRevalidacao item) { }

        public Task<IReadOnlyList<CampanhaRevalidacao>> ListarAsync(CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<CampanhaRevalidacao>>(Campanhas);

        public Task<CampanhaRevalidacao?> ObterComItensAsync(int id, CancellationToken ct = default)
            => Task.FromResult(Campanhas.FirstOrDefault(c => c.Id == id));

        public Task<ItemCampanhaRevalidacao?> ObterItemAsync(int itemId, CancellationToken ct = default)
            => Task.FromResult(Campanhas.SelectMany(c => c.Itens).FirstOrDefault(i => i.Id == itemId));
    }

    private sealed class FakeUsuarioRepo : IUsuarioRepository
    {
        public List<Usuarios> Usuarios { get; } = new();
        public Task<IReadOnlyList<Usuarios>> ListarComPlantasAsync()
            => Task.FromResult<IReadOnlyList<Usuarios>>(Usuarios);
        public Task<Usuarios?> GetByIdAsync(int id)
            => Task.FromResult(Usuarios.FirstOrDefault(u => u.Id == id));
        public void Update(Usuarios entidade) { }

        public Task<Usuarios?> ObterPorLoginAsync(string login)
            => Task.FromResult(Usuarios.FirstOrDefault(u => u.Login == login));
        public Task<IReadOnlyList<Usuarios>> ListarPorPlantaAsync(int plantaId)
            => Task.FromResult<IReadOnlyList<Usuarios>>(Array.Empty<Usuarios>());
        public Task<IReadOnlyList<Plantas>> ObterPlantasDoUsuarioAsync(int usuarioId)
            => Task.FromResult<IReadOnlyList<Plantas>>(Array.Empty<Plantas>());
        public Task<Usuarios?> ObterComPlantasAsync(int id)
            => Task.FromResult(Usuarios.FirstOrDefault(u => u.Id == id));
        public Task AddAsync(Usuarios entidade) { Usuarios.Add(entidade); return Task.CompletedTask; }
        public void Delete(Usuarios entidade) { }
        public Task<IReadOnlyList<Usuarios>> GetAsync()
            => Task.FromResult<IReadOnlyList<Usuarios>>(Usuarios);
    }

    private sealed class FakeUow : IUnitOfWork
    {
        public Task CommitAsync(CancellationToken ct = default) => Task.CompletedTask;
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private sealed class FakeEmail : IEmailService
    {
        public List<EmailMensagem> Enviados { get; } = new();
        public Task<bool> EnviarAsync(EmailMensagem msg, CancellationToken ct = default)
        {
            Enviados.Add(msg);
            return Task.FromResult(true);
        }
    }
}
