namespace EToto.Application.Dto
{
    public class CampanhaRevalidacaoDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int Periodicidade { get; set; }
        public string PeriodicidadeNome { get; set; } = string.Empty;
        public int Status { get; set; }
        public string StatusNome { get; set; } = string.Empty;
        public DateTime DataInicio { get; set; }
        public DateTime DataFimPrevista { get; set; }
        public DateTime? DataFimReal { get; set; }
        public int CriadoPorId { get; set; }
        public string? CriadoPorNome { get; set; }
        public string? Notas { get; set; }

        public int TotalItens { get; set; }
        public int ItensDecididos { get; set; }
    }

    public class CriarCampanhaDto
    {
        public string Nome { get; set; } = string.Empty;
        // Plantas-alvo da campanha (uma ou N). Entram os usuários ativos dessas plantas.
        public List<int> PlantaIds { get; set; } = new();
        // Conclusão prevista informada pelo usuário (substitui a periodicidade).
        public DateTime? DataFimPrevista { get; set; }
        public int ExecutadoPorId { get; set; }
        public string? Notas { get; set; }
    }

    public class ItemCampanhaDto
    {
        public int Id { get; set; }
        public int CampanhaId { get; set; }

        public int UsuarioId { get; set; }
        public string UsuarioLogin { get; set; } = string.Empty;
        public string UsuarioNome { get; set; } = string.Empty;
        public string SnapshotResumo { get; set; } = string.Empty;

        public int? Decisao { get; set; }
        public string? DecisaoNome { get; set; }
        public int? DecididoPorId { get; set; }
        public string? DecididoPorNome { get; set; }
        public DateTime? DecididoEm { get; set; }

        public string? Observacao { get; set; }
    }

    public class DecidirItemDto
    {
        public int CampanhaId { get; set; }
        public int ItemId { get; set; }
        public int Decisao { get; set; }
        public int ExecutadoPorId { get; set; }
        public string? Observacao { get; set; }
    }
}
