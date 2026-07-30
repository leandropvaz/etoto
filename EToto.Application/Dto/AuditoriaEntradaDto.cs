namespace EToto.Application.Dto
{
    public class AuditoriaEntradaDto
    {
        public Guid Id { get; set; }
        public string NomeTabela { get; set; } = string.Empty;
        public string ChaveRegistro { get; set; } = string.Empty;
        public int Acao { get; set; }
        public string AcaoNome { get; set; } = string.Empty;

        public int? UsuarioId { get; set; }
        public string? UsuarioNome { get; set; }

        public DateTime ExecutadoEm { get; set; }

        // JSON cru — a UI pode renderizar diff campo a campo.
        public string? ValoresAntes { get; set; }
        public string? ValoresDepois { get; set; }
    }

    public class AuditoriaConsultaFiltro
    {
        public DateTime? PeriodoInicio { get; set; }
        public DateTime? PeriodoFim { get; set; }
        public int? UsuarioId { get; set; }
        public string? NomeTabela { get; set; }
        public int? Acao { get; set; }

        public int Pagina { get; set; } = 1;
        public int TamanhoPagina { get; set; } = 20;
    }

    public class AuditoriaConsultaResultadoDto
    {
        public List<AuditoriaEntradaDto> Itens { get; set; } = new();
        public int Total { get; set; }
        public int Pagina { get; set; }
        public int TamanhoPagina { get; set; }
    }
}
