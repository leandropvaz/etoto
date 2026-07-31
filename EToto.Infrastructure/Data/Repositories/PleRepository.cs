using EToto.Domain.Entities;
using EToto.Domain.Enums;
using EToto.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EToto.Infrastructure.Data.Repositories
{
    public class PleRepository : IPleRepository
    {
        private readonly LototoContext _context;

        public PleRepository(LototoContext context)
        {
            _context = context;
        }

        public Task<List<Ple>> GetByPlantAsync(int plantId, CancellationToken ct = default)
        {
            return _context.Ples
                .Include(p => p.Equipamentos).ThenInclude(pe => pe.Equipamento)
                .Include(p => p.CriadoPor)
                .Where(p => p.PlantaId == plantId && !p.IsDeleted)
                .OrderByDescending(p => p.DataCriacao)
                .ToListAsync(ct);
        }

        public Task<List<Ple>> GetByEquipamentoAsync(Guid equipamentoId, CancellationToken ct = default)
        {
            return _context.Ples
                .Include(p => p.CriadoPor)
                .Include(p => p.Equipamentos).ThenInclude(pe => pe.Equipamento)
                .Where(p => p.Equipamentos.Any(pe => pe.EquipamentoId == equipamentoId) && !p.IsDeleted)
                .OrderByDescending(p => p.DataCriacao)
                .ToListAsync(ct);
        }

        public Task<Ple?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return _context.Ples
                .Include(p => p.Planta)
                .Include(p => p.Equipamentos).ThenInclude(pe => pe.Equipamento)
                .Include(p => p.CriadoPor)
                .Include(p => p.ModificadoPor)
                .Include(p => p.FinalizadoPor)
                .Include(p => p.UsuariosPermitidos).ThenInclude(up => up.Usuario)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);
        }

        public Task<Ple?> GetByIdWithHistoricoAsync(Guid id, CancellationToken ct = default)
        {
            return _context.Ples
                .Include(p => p.Planta)
                .Include(p => p.Equipamentos).ThenInclude(pe => pe.Equipamento)
                .Include(p => p.CriadoPor)
                .Include(p => p.ModificadoPor)
                .Include(p => p.FinalizadoPor)
                .Include(p => p.UsuariosPermitidos).ThenInclude(up => up.Usuario)
                .Include(p => p.Historico.OrderByDescending(h => h.DataAcao))
                    .ThenInclude(h => h.Usuario)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);
        }

        public Task<bool> EquipamentoTemPleAtivoAsync(Guid equipamentoId, CancellationToken ct = default)
        {
            return _context.PleEquipamentos
                .AnyAsync(pe => pe.EquipamentoId == equipamentoId
                             && !pe.Ple.IsDeleted
                             && (pe.Ple.Status == StatusPle.Criado
                              || pe.Ple.Status == StatusPle.EmAndamento
                              || pe.Ple.Status == StatusPle.InicioDesbloqueio), ct);
        }

        public Task<List<Guid>> GetEquipamentosComPleAtivoAsync(int plantId, CancellationToken ct = default)
        {
            return _context.PleEquipamentos
                .Where(pe => pe.Ple.PlantaId == plantId
                          && !pe.Ple.IsDeleted
                          && (pe.Ple.Status == StatusPle.Criado
                           || pe.Ple.Status == StatusPle.EmAndamento
                           || pe.Ple.Status == StatusPle.InicioDesbloqueio))
                .Select(pe => pe.EquipamentoId)
                .Distinct()
                .ToListAsync(ct);
        }

        public async Task<Dictionary<Guid, StatusPle>> GetEquipamentosComPleAtivoStatusAsync(int plantId, CancellationToken ct = default)
        {
            var items = await _context.PleEquipamentos
                .Where(pe => pe.Ple.PlantaId == plantId
                          && !pe.Ple.IsDeleted
                          && (pe.Ple.Status == StatusPle.Criado
                           || pe.Ple.Status == StatusPle.EmAndamento
                           || pe.Ple.Status == StatusPle.InicioDesbloqueio))
                .Select(pe => new { pe.EquipamentoId, pe.Ple.Status })
                .ToListAsync(ct);

            // Se um equipamento tem múltiplos PLEs, pega o status mais avançado (EmAndamento > Criado)
            return items
                .GroupBy(x => x.EquipamentoId)
                .ToDictionary(g => g.Key, g => g.Max(x => x.Status));
        }

        public Task<List<Ple>> GetPlesAtivosComDetalheAsync(int plantId, CancellationToken ct = default)
        {
            return _context.Ples
                .Include(p => p.Equipamentos)
                .Include(p => p.CriadoPor)
                .Include(p => p.UsuariosPermitidos).ThenInclude(up => up.Usuario)
                .Where(p => p.PlantaId == plantId
                         && !p.IsDeleted
                         && (p.Status == StatusPle.Criado
                          || p.Status == StatusPle.EmAndamento
                          || p.Status == StatusPle.InicioDesbloqueio))
                .ToListAsync(ct);
        }

        public async Task<int> GetProximoNumeroAsync(string codigoPlanta, CancellationToken ct = default)
        {
            var prefix = $"PLE-{codigoPlanta}-";
            var numeros = await _context.Ples
                .Where(p => p.Numero.StartsWith(prefix))
                .Select(p => p.Numero)
                .ToListAsync(ct);

            if (numeros.Count == 0) return 1;

            return numeros
                .Select(n => int.TryParse(n.Split('-').Last(), out var num) ? num : 0)
                .Max() + 1;
        }

        public async Task AddAsync(Ple ple, CancellationToken ct = default)
        {
            await _context.Ples.AddAsync(ple, ct);
        }

        public Task UpdateAsync(Ple ple, CancellationToken ct = default)
        {
            _context.Ples.Update(ple);
            return Task.CompletedTask;
        }

        public async Task AddHistoricoAsync(PleHistorico historico, CancellationToken ct = default)
        {
            await _context.PleHistoricos.AddAsync(historico, ct);
        }

        public Task SaveChangesAsync(CancellationToken ct = default)
        {
            return _context.SaveChangesAsync(ct);
        }
    }
}
