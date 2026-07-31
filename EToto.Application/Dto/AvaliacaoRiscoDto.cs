using EToto.Domain.Enums;

namespace EToto.Application.Dto
{
    public class AvaliacaoRiscoDto
    {
        public Guid Id { get; set; }
        public string Numero { get; set; } = string.Empty;

        public string Departamento { get; set; } = string.Empty;
        public string Operacao { get; set; } = string.Empty;
        public string Tarefa { get; set; } = string.Empty;
        public DateTime Data { get; set; }

        public StatusAvaliacaoRisco Status { get; set; }
        public string? Observacoes { get; set; }

        public string CriadoPorNome { get; set; } = string.Empty;
        public int CriadoPorId { get; set; }
        public string? ModificadoPorNome { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataModificacao { get; set; }

        public List<ArEquipamentoDto> Equipamentos { get; set; } = new();
        public string EquipamentosTags => string.Join(", ", Equipamentos.Select(e => e.Tag));

        public List<AvaliacaoRiscoItemDto> Itens { get; set; } = new();
        public List<AvaliacaoRiscoHistoricoDto> Historico { get; set; } = new();
    }

    public class ArEquipamentoDto
    {
        public Guid EquipamentoId { get; set; }
        public string Tag { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public int PlantaId { get; set; }
        public string PlantaNome { get; set; } = string.Empty;
    }

    public class AvaliacaoRiscoItemDto
    {
        public Guid Id { get; set; }
        public int Ordem { get; set; }
        public string Tarefa { get; set; } = string.Empty;
        public string Perigo { get; set; } = string.Empty;
        public int NumeroExpostos { get; set; }
        public string GravidadeAntes { get; set; } = string.Empty;
        public string ProbabilidadeAntes { get; set; } = string.Empty;
        public string NivelRiscoAntes { get; set; } = string.Empty;
        public string MedidasProtecao { get; set; } = string.Empty;
        public string GravidadeDepois { get; set; } = string.Empty;
        public string ProbabilidadeDepois { get; set; } = string.Empty;
        public string NivelRiscoDepois { get; set; } = string.Empty;
    }

    public class AvaliacaoRiscoHistoricoDto
    {
        public Guid Id { get; set; }
        public string Acao { get; set; } = string.Empty;
        public string? Detalhes { get; set; }
        public string UsuarioNome { get; set; } = string.Empty;
        public DateTime DataAcao { get; set; }
    }

    public class AvaliacaoRiscoCreateDto
    {
        public List<Guid> EquipamentoIds { get; set; } = new();
        public string Departamento { get; set; } = string.Empty;
        public string Operacao { get; set; } = string.Empty;
        public string Tarefa { get; set; } = string.Empty;
        public DateTime Data { get; set; }
        public string? Observacoes { get; set; }
        public int CriadoPorId { get; set; }
        public List<AvaliacaoRiscoItemDto> Itens { get; set; } = new();
    }

    public class AvaliacaoRiscoUpdateDto
    {
        public Guid Id { get; set; }
        public List<Guid> EquipamentoIds { get; set; } = new();
        public string Departamento { get; set; } = string.Empty;
        public string Operacao { get; set; } = string.Empty;
        public string Tarefa { get; set; } = string.Empty;
        public DateTime Data { get; set; }
        public string? Observacoes { get; set; }
        public int ModificadoPorId { get; set; }
        public List<AvaliacaoRiscoItemDto> Itens { get; set; } = new();
    }
}
