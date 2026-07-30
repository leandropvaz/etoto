namespace EToto.Web.Dto
{
    public class LoginResult
    {
        public bool Success { get; init; }
        public string? ErrorMessage { get; init; }

        // Sinaliza falha por acesso vencido (#2). Permite à UI traduzir e formatar a mensagem.
        public bool AcessoVencido { get; init; }

        // Sinaliza falha por treinamento vencido (#3).
        public bool TreinamentoVencido { get; init; }

        // Dias restantes quando a validade do ACESSO está dentro da janela de aviso (#2),
        // 0 quando vence hoje. Quando preenchido, a UI deve mostrar um banner de aviso.
        public int? DiasParaVencer { get; init; }

        // Dias restantes quando a validade do TREINAMENTO está na janela de aviso (#3).
        public int? DiasParaVencerTreinamento { get; init; }

        public static LoginResult Ok(int? diasParaVencer = null, int? diasParaVencerTreinamento = null) => new()
        {
            Success = true,
            DiasParaVencer = diasParaVencer,
            DiasParaVencerTreinamento = diasParaVencerTreinamento
        };

        public static LoginResult Fail(string message) => new()
        {
            Success = false,
            ErrorMessage = message
        };

        public static LoginResult Vencido() => new()
        {
            Success = false,
            AcessoVencido = true
        };

        public static LoginResult TreinamentoExpirado() => new()
        {
            Success = false,
            TreinamentoVencido = true
        };
    }
}
