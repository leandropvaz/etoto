using EToto.Domain.Entities;

namespace EToto.Domain.Interfaces
{
    public class AuditoriaConsultaCriterio
    {
        public DateTime? PeriodoInicio { get; set; }
        public DateTime? PeriodoFim { get; set; }
        public int? UsuarioId { get; set; }
        public string? NomeTabela { get; set; }
        public int? Acao { get; set; }
        public int Pagina { get; set; } = 1;
        public int TamanhoPagina { get; set; } = 20;
    }

    public interface IAuditoriaRepository
    {
        Task<(IReadOnlyList<AuditoriaEntrada> Itens, int Total)> ConsultarAsync(
            AuditoriaConsultaCriterio criterio,
            CancellationToken ct = default);
    }
}
