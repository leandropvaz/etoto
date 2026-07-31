namespace EToto.Domain.Enums
{
    // Resultado do cálculo de proximidade do vencimento (#2).
    public enum StatusValidadeAcesso
    {
        SemValidade = 0,   // Funcionário (sem data) ou Terceiro com data ainda nula.
        Vigente = 1,       // Faltam mais de 30 dias.
        Vencendo = 2,      // Faltam até 30 dias (inclusive hoje).
        Vencido = 3        // Data anterior a "hoje".
    }
}
