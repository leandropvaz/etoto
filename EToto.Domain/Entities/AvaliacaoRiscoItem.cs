namespace EToto.Domain.Entities
{
    public class AvaliacaoRiscoItem
    {
        public Guid Id { get; set; }

        public Guid AvaliacaoRiscoId { get; set; }
        public AvaliacaoRisco AvaliacaoRisco { get; set; } = null!;

        public int Ordem { get; set; }

        public string Tarefa { get; set; } = string.Empty;
        public string Perigo { get; set; } = string.Empty;
        public int NumeroExpostos { get; set; }

        // Antes da redução de riscos
        public string GravidadeAntes { get; set; } = string.Empty;
        public string ProbabilidadeAntes { get; set; } = string.Empty;
        public string NivelRiscoAntes { get; set; } = string.Empty;

        // Medidas de proteção (Métodos Alternativos)
        public string MedidasProtecao { get; set; } = string.Empty;

        // Após a redução do risco
        public string GravidadeDepois { get; set; } = string.Empty;
        public string ProbabilidadeDepois { get; set; } = string.Empty;
        public string NivelRiscoDepois { get; set; } = string.Empty;
    }
}
