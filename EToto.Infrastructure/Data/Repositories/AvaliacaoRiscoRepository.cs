using EToto.Domain.Entities;
using EToto.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EToto.Infrastructure.Data.Repositories
{
    public class AvaliacaoRiscoRepository : IAvaliacaoRiscoRepository
    {
        private readonly LototoContext _context;

        public AvaliacaoRiscoRepository(LototoContext context) => _context = context;

        public Task<List<AvaliacaoRisco>> GetAllAsync(CancellationToken ct = default)
        {
            return _context.AvaliacoesRisco
                .Include(a => a.CriadoPor)
                .Include(a => a.Equipamentos).ThenInclude(e => e.Equipamento).ThenInclude(e => e!.Planta)
                .Include(a => a.Itens.OrderBy(i => i.Ordem))
                .Where(a => !a.IsDeleted)
                .OrderByDescending(a => a.DataCriacao)
                .ToListAsync(ct);
        }

        public Task<AvaliacaoRisco?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return _context.AvaliacoesRisco
                .Include(a => a.CriadoPor)
                .Include(a => a.Equipamentos).ThenInclude(e => e.Equipamento).ThenInclude(e => e!.Planta)
                .Include(a => a.ModificadoPor)
                .Include(a => a.Itens.OrderBy(i => i.Ordem))
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, ct);
        }

        public Task<AvaliacaoRisco?> GetByIdWithHistoricoAsync(Guid id, CancellationToken ct = default)
        {
            return _context.AvaliacoesRisco
                .Include(a => a.CriadoPor)
                .Include(a => a.Equipamentos).ThenInclude(e => e.Equipamento).ThenInclude(e => e!.Planta)
                .Include(a => a.ModificadoPor)
                .Include(a => a.Itens.OrderBy(i => i.Ordem))
                .Include(a => a.Historico.OrderBy(h => h.DataAcao))
                    .ThenInclude(h => h.Usuario)
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, ct);
        }

        public async Task<int> GetProximoNumeroAsync(CancellationToken ct = default)
        {
            var prefix = "AR-";
            var numeros = await _context.AvaliacoesRisco
                .Where(a => a.Numero.StartsWith(prefix))
                .Select(a => a.Numero)
                .ToListAsync(ct);

            if (numeros.Count == 0) return 1;
            return numeros.Select(n => int.TryParse(n.Split('-').Last(), out var num) ? num : 0).Max() + 1;
        }

        public async Task RemoveItensAsync(Guid avaliacaoRiscoId, CancellationToken ct = default)
        {
            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM AvaliacaoRiscoItem WHERE AvaliacaoRiscoId = {0}", avaliacaoRiscoId);
            _context.ChangeTracker.Clear();
        }

        public async Task RemoveEquipamentosAsync(Guid avaliacaoRiscoId, CancellationToken ct = default)
        {
            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM AvaliacaoRiscoEquipamento WHERE AvaliacaoRiscoId = {0}", avaliacaoRiscoId);
        }

        public async Task AddEquipamentosAsync(List<AvaliacaoRiscoEquipamento> equipamentos, CancellationToken ct = default)
        {
            await _context.AvaliacaoRiscoEquipamentos.AddRangeAsync(equipamentos, ct);
        }

        public async Task AddItensAsync(List<AvaliacaoRiscoItem> itens, CancellationToken ct = default)
        {
            await _context.AvaliacaoRiscoItens.AddRangeAsync(itens, ct);
        }

        public async Task UpdateCabecalhoAsync(Guid id, string departamento, string operacao, string tarefa, DateTime data, string? observacoes, int modificadoPorId, CancellationToken ct = default)
        {
            await _context.Database.ExecuteSqlRawAsync(
                "UPDATE AvaliacaoRisco SET Departamento={0}, Operacao={1}, Tarefa={2}, Data={3}, Observacoes={4}, ModificadoPorId={5}, DataModificacao={6} WHERE Id={7}",
                departamento, operacao, tarefa, data, (object?)observacoes ?? DBNull.Value, modificadoPorId,
                TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time")), id);
        }

        public async Task AddAsync(AvaliacaoRisco entity, CancellationToken ct = default)
            => await _context.AvaliacoesRisco.AddAsync(entity, ct);

        public async Task AddHistoricoAsync(AvaliacaoRiscoHistorico historico, CancellationToken ct = default)
            => await _context.AvaliacaoRiscoHistoricos.AddAsync(historico, ct);

        public Task UpdateAsync(AvaliacaoRisco entity, CancellationToken ct = default)
        {
            var entry = _context.Entry(entity);
            if (entry.State == Microsoft.EntityFrameworkCore.EntityState.Detached)
                _context.AvaliacoesRisco.Attach(entity);
            entry.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken ct = default)
            => _context.SaveChangesAsync(ct);
    }
}
