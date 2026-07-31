namespace EToto.Application.Dto
{
    public class RelatorioUsuariosFiltro
    {
        public int? PlantaId { get; set; }
        public int? Perfil { get; set; }
        public int? TipoVinculo { get; set; }

        // 1=Vigente, 2=Vencendo (≤30d), 3=Vencido. Aplicado tanto à validade do acesso
        // quanto à validade do treinamento (qualquer um casando satisfaz).
        public int? StatusValidade { get; set; }
    }

    public class RelatorioUsuarioItemDto
    {
        public int Id { get; set; }
        public string Login { get; set; } = string.Empty;
        public string NomeCompleto { get; set; } = string.Empty;
        public bool Ativa { get; set; }

        public List<string> PerfisNomes { get; set; } = new();
        public List<string> PlantasNomes { get; set; } = new();

        public int TipoVinculo { get; set; }
        public string TipoVinculoNome { get; set; } = string.Empty;
        public string? NomeEmpresa { get; set; }

        public DateTime? DataValidadeAcesso { get; set; }
        public int StatusValidadeAcesso { get; set; }

        public bool TreinamentoConcluido { get; set; }
        public DateTime? DataValidadeTreinamento { get; set; }
        public int StatusValidadeTreinamento { get; set; }
        public bool ExigeTreinamento { get; set; }

        public DateTime? DataUltimoLogin { get; set; }

        public string? CriadoPorNome { get; set; }
        public DateTime? CriadoEm { get; set; }
        public string? AlteradoPorNome { get; set; }
        public DateTime? AlteradoEm { get; set; }
    }
}
