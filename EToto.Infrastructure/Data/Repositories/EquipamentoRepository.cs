using EToto.Domain.Entities;
using EToto.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EToto.Infrastructure.Data.Repositories
{
    public class EquipamentoRepository : IEquipamentoRepository
    {
        private readonly LototoContext _context;

        public EquipamentoRepository(LototoContext context)
        {
            _context = context;
        }

        public Task<List<Equipamento>> GetByPlantAsync(int plantId, CancellationToken ct = default)
        {
            return _context.Equipamentos
                .Include(e => e.CreatedByUser)  
                .Include(e => e.UpdatedByUser)
                .Where(x => x.PlantaId == plantId && !x.IsDeleted)
                .OrderBy(x => x.Tag)
                .ThenBy(x => x.LineNumber)
                .ToListAsync(ct);
        }

        public Task<Equipamento?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return _context.Equipamentos
                        .Include(e => e.CreatedByUser)
                        .Include(e => e.UpdatedByUser)
                        .FirstOrDefaultAsync(x => x.Id == id, ct);

        }

        public async Task AddRangeAsync(IEnumerable<Equipamento> records, CancellationToken ct = default)
        {
            await _context.Equipamentos.AddRangeAsync(records, ct);
        }

        public async Task AddAsync(Equipamento record, CancellationToken ct = default)
        {
            await _context.Equipamentos.AddAsync(record, ct);
        }

        public Task UpdateAsync(Equipamento record, CancellationToken ct = default)
        {
            _context.Equipamentos.Update(record);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken ct = default)
        {
            return _context.SaveChangesAsync(ct);
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
        {
            return await _context.Database.BeginTransactionAsync(ct);
        }
    }
}