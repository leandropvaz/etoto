using EToto.Domain.Entities;
using EToto.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EToto.Infrastructure.Data.Repositories
{
    public class AuditoriaRepository : IAuditoriaRepository
    {
        private readonly LototoContext _ctx;

        public AuditoriaRepository(LototoContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<(IReadOnlyList<AuditoriaEntrada> Itens, int Total)> ConsultarAsync(
            AuditoriaConsultaCriterio criterio,
            CancellationToken ct = default)
        {
            var query = _ctx.AuditoriaEntradas
                .Include(a => a.Usuario)
                .AsQueryable();

            if (criterio.PeriodoInicio.HasValue)
            {
                var inicio = criterio.PeriodoInicio.Value.Date;
                query = query.Where(a => a.ExecutadoEm >= inicio);
            }
            if (criterio.PeriodoFim.HasValue)
            {
                // Fim inclusivo no dia: +1 dia, < limite.
                var fim = criterio.PeriodoFim.Value.Date.AddDays(1);
                query = query.Where(a => a.ExecutadoEm < fim);
            }
            if (criterio.UsuarioId.HasValue)
                query = query.Where(a => a.UsuarioId == criterio.UsuarioId.Value);
            if (!string.IsNullOrWhiteSpace(criterio.NomeTabela))
                query = query.Where(a => a.NomeTabela == criterio.NomeTabela);
            if (criterio.Acao.HasValue)
                query = query.Where(a => (int)a.Acao == criterio.Acao.Value);

            var total = await query.CountAsync(ct);

            var pagina = Math.Max(1, criterio.Pagina);
            var tamanho = Math.Clamp(criterio.TamanhoPagina, 5, 200);

            var itens = await query
                .OrderByDescending(a => a.ExecutadoEm)
                .Skip((pagina - 1) * tamanho)
                .Take(tamanho)
                .ToListAsync(ct);

            return (itens, total);
        }
    }
}
