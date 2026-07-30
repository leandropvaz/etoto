using EToto.Domain.Enums;

namespace EToto.Application.Dto
{
    public class PleDto
    {
        public Guid Id { get; set; }
        public string Numero { get; set; } = string.Empty;

        public int PlantaId { get; set; }
        public string PlantaNome { get; set; } = string.Empty;

        public List<PleEquipamentoDto> Equipamentos { get; set; } = new();
        public string EquipamentosTags => string.Join(", ", Equipamentos.Select(e => e.Tag));
        // Para a Lista de Bloqueios (com DISTINCT, evita repeticao quando ha varias linhas de energia para o mesmo equipamento)
        public string TagsDispDistinct => string.Join(", ",
            Equipamentos.Select(e => e.TagDispositivo).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct());
        public string EquipamentoTagPlusNomeDistinct => string.Join(", ",
            Equipamentos
                .Where(e => !string.IsNullOrWhiteSpace(e.Tag) || !string.IsNullOrWhiteSpace(e.Nome))
                .Select(e => string.IsNullOrWhiteSpace(e.Nome) ? e.Tag : $"{e.Tag} - {e.Nome}")
                .Distinct());

        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }

        public StatusPle Status { get; set; }
        public string? MotivoCancelamento { get; set; }

        public string CriadoPorNome { get; set; } = string.Empty;
        public int CriadoPorId { get; set; }
        public string? ModificadoPorNome { get; set; }
        public string? FinalizadoPorNome { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataModificacao { get; set; }
        public DateTime? DataFinalizacao { get; set; }

        // Delegação
        public List<PleUsuarioPermitidoDto> UsuariosPermitidos { get; set; } = new();

        public List<PleHistoricoDto> Historico { get; set; } = new();
    }

    public class PleEquipamentoDto
    {
        public Guid EquipamentoId { get; set; }
        public string Tag { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string TipoEnergia { get; set; } = string.Empty;
        public string DispositivoBloqueio { get; set; } = string.Empty;
        public string TipoBloqueio { get; set; } = string.Empty;
        // Para o PDF — TAG do dispositivo de isolamento e descrição da energia perigosa
        public string TagDispositivo { get; set; } = string.Empty;
        public string DescEnergia { get; set; } = string.Empty;
    }

    public class PleUsuarioPermitidoDto
    {
        public int UsuarioId { get; set; }
        public string NomeCompleto { get; set; } = string.Empty;
    }

    public class PleHistoricoDto
    {
        public Guid Id { get; set; }
        public string Acao { get; set; } = string.Empty;
        public string? StatusAnterior { get; set; }
        public string? StatusNovo { get; set; }
        public string? Detalhes { get; set; }
        public string UsuarioNome { get; set; } = string.Empty;
        public DateTime DataAcao { get; set; }
    }

    public class PleCreateDto
    {
        public int PlantaId { get; set; }
        public List<Guid> EquipamentoIds { get; set; } = new();
        public List<int> UsuariosPermitidosIds { get; set; } = new();
        public DateTime DataInicio { get; set; }
        public int CriadoPorId { get; set; }
    }

    public class PleUpdateDto
    {
        public Guid Id { get; set; }
        public List<Guid> EquipamentoIds { get; set; } = new();
        public List<int> UsuariosPermitidosIds { get; set; } = new();
        public DateTime DataInicio { get; set; }
        public int ModificadoPorId { get; set; }
    }
}
