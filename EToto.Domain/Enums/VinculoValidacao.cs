namespace EToto.Domain.Enums
{
    public static class VinculoValidacao
    {
        // Janela de aviso antes do vencimento (#2).
        public const int DiasJanelaAvisoVencimento = 30;

        // Limite máximo da validade de acesso de Terceiro (#2).
        public const int MesesMaximosValidadeAcesso = 6;

        public const string MensagemEmpresaObrigatoria =
            "Para Terceiro, o nome da empresa é obrigatório.";

        public const string MensagemValidadeObrigatoria =
            "Para Terceiro, a data de validade do acesso é obrigatória.";

        public const string MensagemValidadeMaxima =
            "Para Terceiro, a validade de acesso não pode exceder 6 meses.";

        // Data máxima permitida para a validade de acesso (hoje + 6 meses).
        public static DateTime DataValidadeMaxima(DateTime? referenciaUtc = null)
            => (referenciaUtc ?? DateTime.UtcNow).Date.AddMonths(MesesMaximosValidadeAcesso);

        public static void ValidarTerceiro(TipoVinculo tipo, string? nomeEmpresa, DateTime? dataValidade)
        {
            if (tipo != TipoVinculo.Terceiro)
                return;

            if (string.IsNullOrWhiteSpace(nomeEmpresa))
                throw new InvalidOperationException(MensagemEmpresaObrigatoria);

            if (!dataValidade.HasValue)
                throw new InvalidOperationException(MensagemValidadeObrigatoria);
        }

        // Limite de 6 meses aplicado no cadastro/edição de Terceiro pela tela de Usuários (#2).
        // Mantido separado de ValidarTerceiro para não restringir a importação em lote (ImportTool).
        public static void ValidarLimiteValidadeAcesso(TipoVinculo tipo, DateTime? dataValidade,
            DateTime? referenciaUtc = null)
        {
            if (tipo != TipoVinculo.Terceiro || !dataValidade.HasValue)
                return;

            if (dataValidade.Value.Date > DataValidadeMaxima(referenciaUtc))
                throw new InvalidOperationException(MensagemValidadeMaxima);
        }

        public static StatusValidadeAcesso AvaliarStatus(DateTime? dataValidade, DateTime? referenciaUtc = null)
        {
            if (!dataValidade.HasValue)
                return StatusValidadeAcesso.SemValidade;

            var hoje = (referenciaUtc ?? DateTime.UtcNow).Date;
            var validade = dataValidade.Value.Date;

            if (validade < hoje)
                return StatusValidadeAcesso.Vencido;

            var diasRestantes = (validade - hoje).TotalDays;
            return diasRestantes <= DiasJanelaAvisoVencimento
                ? StatusValidadeAcesso.Vencendo
                : StatusValidadeAcesso.Vigente;
        }
    }
}
