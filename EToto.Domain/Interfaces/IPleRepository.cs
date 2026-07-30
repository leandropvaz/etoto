using EToto.Domain.Entities;
using EToto.Domain.Enums;

namespace EToto.Domain.Interfaces
{
    public interface IPleRepository
    {
        Task<List<Ple>> GetByPlantAsync(int plantId, CancellationToken ct = default);
        Task<List<Ple>> GetByEquipamentoAsync(Guid equipamentoId, CancellationToken ct = default);
        Task<Ple?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Ple?> GetByIdWithHistoricoAsync(Guid id, CancellationToken ct = default);

        Task<bool> EquipamentoTemPleAtivoAsync(Guid equipamentoId, CancellationToken ct = default);
        Task<List<Guid>> GetEquipamentosComPleAtivoAsync(int plantId, CancellationToken ct = default);
        Task<Dictionary<Guid, StatusPle>> GetEquipamentosComPleAtivoStatusAsync(int plantId, CancellationToken ct = default);

        Task<int> GetProximoNumeroAsync(string codigoPlanta, CancellationToken ct = default);

        Task AddAsync(Ple ple, CancellationToken ct = default);
        Task UpdateAsync(Ple ple, CancellationToken ct = default);
        Task AddHistoricoAsync(PleHistorico historico, CancellationToken ct = default);

        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
