namespace EToto.Domain.Enums
{
    // Em meses — facilita calcular DataFimPrevista = DataInicio + Periodicidade.
    public enum Periodicidade
    {
        Mensal = 1,
        Trimestral = 3,
        Semestral = 6,
        Anual = 12
    }
}
