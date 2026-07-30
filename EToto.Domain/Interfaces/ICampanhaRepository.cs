using EToto.Domain.Entities;

namespace EToto.Domain.Interfaces
{
    public interface ICampanhaRepository
    {
        Task<IReadOnlyList<CampanhaRevalidacao>> ListarAsync(CancellationToken ct = default);
        Task<CampanhaRevalidacao?> ObterComItensAsync(int id, CancellationToken ct = default);
        Task AdicionarAsync(CampanhaRevalidacao campanha, CancellationToken ct = default);
        void Atualizar(CampanhaRevalidacao campanha);
        Task<ItemCampanhaRevalidacao?> ObterItemAsync(int itemId, CancellationToken ct = default);
        void AtualizarItem(ItemCampanhaRevalidacao item);
    }
}
