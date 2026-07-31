using EToto.Domain.Enums;

namespace EToto.Domain.Entities
{
    public class AvaliacaoRisco
    {
        public Guid Id { get; set; }

        public string Numero { get; set; } = string.Empty;

        // Sem vínculo com planta — global
        public string Departamento { get; set; } = string.Empty;
        public string Operacao { get; set; } = string.Empty;
        public string Tarefa { get; set; } = string.Empty;
        public DateTime Data { get; set; }

        public StatusAvaliacaoRisco Status { get; set; } = StatusAvaliacaoRisco.Ativa;
        public string? Observacoes { get; set; }

        // Auditoria
        public int CriadoPorId { get; set; }
        public Usuarios CriadoPor { get; set; } = null!;

        public int? ModificadoPorId { get; set; }
        public Usuarios? ModificadoPor { get; set; }

        public DateTime DataCriacao { get; set; }
        public DateTime? DataModificacao { get; set; }

        public bool IsDeleted { get; set; }

        // Navegação
        public ICollection<AvaliacaoRiscoEquipamento> Equipamentos { get; set; } = new List<AvaliacaoRiscoEquipamento>();
        public ICollection<AvaliacaoRiscoItem> Itens { get; set; } = new List<AvaliacaoRiscoItem>();
        public ICollection<AvaliacaoRiscoHistorico> Historico { get; set; } = new List<AvaliacaoRiscoHistorico>();
    }
}
