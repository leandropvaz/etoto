using EToto.Application.Dto;
using EToto.Domain.Enums;
using EToto.Domain.Interfaces;

namespace EToto.Application.Services
{
    public class AuditoriaService
    {
        private readonly IAuditoriaRepository _repo;

        public AuditoriaService(IAuditoriaRepository repo)
        {
            _repo = repo;
        }

        public async Task<AuditoriaConsultaResultadoDto> ConsultarAsync(
            AuditoriaConsultaFiltro filtro,
            CancellationToken ct = default)
        {
            var criterio = new AuditoriaConsultaCriterio
            {
                PeriodoInicio = filtro.PeriodoInicio,
                PeriodoFim = filtro.PeriodoFim,
                UsuarioId = filtro.UsuarioId,
                NomeTabela = filtro.NomeTabela,
                Acao = filtro.Acao,
                Pagina = filtro.Pagina,
                TamanhoPagina = filtro.TamanhoPagina
            };

            var (itens, total) = await _repo.ConsultarAsync(criterio, ct);

            return new AuditoriaConsultaResultadoDto
            {
                Total = total,
                Pagina = Math.Max(1, filtro.Pagina),
                TamanhoPagina = filtro.TamanhoPagina,
                Itens = itens.Select(a => new AuditoriaEntradaDto
                {
                    Id = a.Id,
                    NomeTabela = a.NomeTabela,
                    ChaveRegistro = a.ChaveRegistro,
                    Acao = (int)a.Acao,
                    AcaoNome = a.Acao.ToString(),
                    UsuarioId = a.UsuarioId,
                    UsuarioNome = a.Usuario?.NomeCompleto,
                    ExecutadoEm = a.ExecutadoEm,
                    ValoresAntes = a.ValoresAntes,
                    ValoresDepois = a.ValoresDepois
                }).ToList()
            };
        }
    }
}
