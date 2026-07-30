using EToto.Domain.Entities;
using EToto.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EToto.Infrastructure.Data.Repositories
{
    public class CampanhaRepository : ICampanhaRepository
    {
        private readonly LototoContext _ctx;

        public CampanhaRepository(LototoContext ctx) => _ctx = ctx;

        public Task<IReadOnlyList<CampanhaRevalidacao>> ListarAsync(CancellationToken ct = default)
            => _ctx.CampanhasRevalidacao
                .Include(c => c.CriadoPor)
                .OrderByDescending(c => c.DataInicio)
                .ToListAsync(ct)
                .ContinueWith<IReadOnlyList<CampanhaRevalidacao>>(t => t.Result, ct);

        public Task<CampanhaRevalidacao?> ObterComItensAsync(int id, CancellationToken ct = default)
            => _ctx.CampanhasRevalidacao
                .Include(c => c.CriadoPor)
                .Include(c => c.Itens)
                    .ThenInclude(i => i.Usuario)
                .Include(c => c.Itens)
                    .ThenInclude(i => i.DecididoPor)
                .FirstOrDefaultAsync(c => c.Id == id, ct);

        public async Task AdicionarAsync(CampanhaRevalidacao campanha, CancellationToken ct = default)
        {
            await _ctx.CampanhasRevalidacao.AddAsync(campanha, ct);
        }

        public void Atualizar(CampanhaRevalidacao campanha)
        {
            _ctx.CampanhasRevalidacao.Update(campanha);
        }

        public Task<ItemCampanhaRevalidacao?> ObterItemAsync(int itemId, CancellationToken ct = default)
            => _ctx.ItensCampanhaRevalidacao
                .Include(i => i.Usuario)
                .Include(i => i.Campanha)
                .FirstOrDefaultAsync(i => i.Id == itemId, ct);

        public void AtualizarItem(ItemCampanhaRevalidacao item)
        {
            _ctx.ItensCampanhaRevalidacao.Update(item);
        }
    }
}
