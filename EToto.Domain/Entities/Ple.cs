using EToto.Domain.Enums;

namespace EToto.Domain.Entities
{
    public class Ple
    {
        public Guid Id { get; set; }

        public string Numero { get; set; } = string.Empty;

        public int PlantaId { get; set; }
        public Plantas Planta { get; set; } = null!;

        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }

        public StatusPle Status { get; set; } = StatusPle.Criado;
        public string? MotivoCancelamento { get; set; }

        // Auditoria
        public int CriadoPorId { get; set; }
        public Usuarios CriadoPor { get; set; } = null!;

        public int? ModificadoPorId { get; set; }
        public Usuarios? ModificadoPor { get; set; }

        public int? FinalizadoPorId { get; set; }
        public Usuarios? FinalizadoPor { get; set; }

        public DateTime DataCriacao { get; set; }
        public DateTime? DataModificacao { get; set; }
        public DateTime? DataFinalizacao { get; set; }

        public bool IsDeleted { get; set; }

        // Navegação
        public ICollection<PleEquipamento> Equipamentos { get; set; } = new List<PleEquipamento>();
        public ICollection<PleUsuarioPermitido> UsuariosPermitidos { get; set; } = new List<PleUsuarioPermitido>();
        public ICollection<PleHistorico> Historico { get; set; } = new List<PleHistorico>();
    }
}
