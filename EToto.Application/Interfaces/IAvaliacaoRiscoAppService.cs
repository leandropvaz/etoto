using EToto.Application.Dto;
using EToto.Domain.Enums;

namespace EToto.Application.Interfaces
{
    public interface IAvaliacaoRiscoAppService
    {
        Task<List<AvaliacaoRiscoDto>> GetAllAsync(CancellationToken ct = default);
        Task<AvaliacaoRiscoDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<AvaliacaoRiscoDto?> GetByIdWithHistoricoAsync(Guid id, CancellationToken ct = default);
        Task<AvaliacaoRiscoDto> CreateAsync(AvaliacaoRiscoCreateDto dto, CancellationToken ct = default);
        Task<AvaliacaoRiscoDto> UpdateAsync(AvaliacaoRiscoUpdateDto dto, CancellationToken ct = default);
        Task AlterarStatusAsync(Guid id, StatusAvaliacaoRisco novoStatus, int usuarioId, CancellationToken ct = default);
        Task SoftDeleteAsync(Guid id, int usuarioId, CancellationToken ct = default);
    }
}
