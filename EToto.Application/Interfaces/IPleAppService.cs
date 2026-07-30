using EToto.Application.Dto;
using EToto.Domain.Enums;

namespace EToto.Application.Interfaces
{
    public interface IPleAppService
    {
        Task<List<PleDto>> GetByPlantAsync(int plantId, CancellationToken ct = default);
        Task<PleDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<PleDto?> GetByIdWithHistoricoAsync(Guid id, CancellationToken ct = default);

        Task<List<Guid>> GetEquipamentosComPleAtivoAsync(int plantId, CancellationToken ct = default);
        Task<Dictionary<Guid, StatusPle>> GetEquipamentosComPleAtivoStatusAsync(int plantId, CancellationToken ct = default);

        Task<PleDto> CreateAsync(PleCreateDto dto, CancellationToken ct = default);
        Task<PleDto> UpdateAsync(PleUpdateDto dto, CancellationToken ct = default);
        Task AlterarStatusAsync(Guid pleId, StatusPle novoStatus, int usuarioId, string? motivo = null, CancellationToken ct = default);
        Task SoftDeleteAsync(Guid id, int usuarioId, CancellationToken ct = default);
    }
}
