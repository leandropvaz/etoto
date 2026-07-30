using EToto.Domain.Enums;

namespace EToto.Domain.Entities
{
    public class CampanhaRevalidacao
    {
        public int Id { get; set; }
        public string Nome { get; set; } = default!;
        public Periodicidade Periodicidade { get; set; }
        public StatusCampanha Status { get; set; } = StatusCampanha.Planejada;

        public DateTime DataInicio { get; set; } = DateTime.UtcNow;
        public DateTime DataFimPrevista { get; set; }
        public DateTime? DataFimReal { get; set; }

        public int CriadoPorId { get; set; }
        public Usuarios? CriadoPor { get; set; }

        public string? Notas { get; set; }

        public ICollection<ItemCampanhaRevalidacao> Itens { get; set; } = new List<ItemCampanhaRevalidacao>();
    }

    public class ItemCampanhaRevalidacao
    {
        public int Id { get; set; }

        public int CampanhaId { get; set; }
        public CampanhaRevalidacao? Campanha { get; set; }

        public int UsuarioId { get; set; }
        public Usuarios? Usuario { get; set; }

        public DecisaoRevisao? Decisao { get; set; }
        public int? DecididoPorId { get; set; }
        public Usuarios? DecididoPor { get; set; }
        public DateTime? DecididoEm { get; set; }

        public string? Observacao { get; set; }

        // Snapshot JSON dos dados do usuário no momento em que a campanha foi criada
        // (perfis, plantas, vínculo, validades). Útil para o gestor enxergar o estado
        // ao tomar a decisão sem depender do estado atual da entidade Usuários.
        public string? SnapshotUsuarioJson { get; set; }
    }
}
