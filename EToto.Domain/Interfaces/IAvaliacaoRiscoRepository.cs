using EToto.Domain.Entities;

namespace EToto.Domain.Interfaces
{
    public interface IAvaliacaoRiscoRepository
    {
        Task<List<AvaliacaoRisco>> GetAllAsync(CancellationToken ct = default);
        Task<AvaliacaoRisco?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<AvaliacaoRisco?> GetByIdWithHistoricoAsync(Guid id, CancellationToken ct = default);
        Task<int> GetProximoNumeroAsync(CancellationToken ct = default);
        Task RemoveItensAsync(Guid avaliacaoRiscoId, CancellationToken ct = default);
        Task RemoveEquipamentosAsync(Guid avaliacaoRiscoId, CancellationToken ct = default);
        Task AddEquipamentosAsync(List<AvaliacaoRiscoEquipamento> equipamentos, CancellationToken ct = default);
        Task AddItensAsync(List<AvaliacaoRiscoItem> itens, CancellationToken ct = default);
        Task UpdateCabecalhoAsync(Guid id, string departamento, string operacao, string tarefa, DateTime data, string? observacoes, int modificadoPorId, CancellationToken ct = default);
        Task AddAsync(AvaliacaoRisco entity, CancellationToken ct = default);
        Task AddHistoricoAsync(AvaliacaoRiscoHistorico historico, CancellationToken ct = default);
        Task UpdateAsync(AvaliacaoRisco entity, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
