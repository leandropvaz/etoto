using EToto.Domain.Enums;

namespace EToto.Application.Dto
{
    /// <summary>
    /// Visão consolidada do painel de alertas de bloqueio/desbloqueio de uma planta.
    /// </summary>
    public class PainelAlertaDto
    {
        public int PlantaId { get; set; }
        public string PlantaNome { get; set; } = string.Empty;
        public string PlantaCodigo { get; set; } = string.Empty;

        public List<PainelAlertaEquipamentoDto> Equipamentos { get; set; } = new();

        public int TotalBloqueados { get; set; }
        public int TotalEmProcesso { get; set; }
        public int TotalLivres { get; set; }
    }

    public class PainelAlertaEquipamentoDto
    {
        public Guid EquipamentoId { get; set; }
        public string Tag { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;

        /// <summary>Status do bloqueio ativo; null = equipamento livre.</summary>
        public StatusPle? Status { get; set; }

        // Bloqueado (cadeado ativo) inclui o passo intermediário InicioDesbloqueio,
        // consistente com a regra do restante do sistema.
        public bool Bloqueado => Status is StatusPle.EmAndamento or StatusPle.InicioDesbloqueio;
        public bool EmProcesso => Status is StatusPle.Criado;
        public bool Livre => Status is null;

        /// <summary>Detalhe (só preenchido para perfis autorizados / modo interno).</summary>
        public List<PainelAlertaRequisicaoDto> Requisicoes { get; set; } = new();
    }

    public class PainelAlertaRequisicaoDto
    {
        public Guid PleId { get; set; }
        public string Numero { get; set; } = string.Empty;
        public StatusPle Status { get; set; }
        public List<string> Lideres { get; set; } = new();
    }
}
