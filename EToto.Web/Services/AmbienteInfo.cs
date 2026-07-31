namespace EToto.Web.Services;

/// <summary>
/// Identifica o ambiente de execução a partir da App Setting "Ambiente".
/// O valor é definido por App Service no Azure (ex.: "Homologacao").
/// Em produção a setting fica ausente, então nenhum aviso é exibido.
/// </summary>
public class AmbienteInfo
{
    private readonly string _ambiente;

    public AmbienteInfo(IConfiguration configuration)
    {
        _ambiente = configuration["Ambiente"]?.Trim() ?? string.Empty;
    }

    public string Nome => _ambiente;

    public bool EhHomologacao =>
        _ambiente.Equals("Homologacao", StringComparison.OrdinalIgnoreCase) ||
        _ambiente.Equals("Homologação", StringComparison.OrdinalIgnoreCase) ||
        _ambiente.Equals("Staging", StringComparison.OrdinalIgnoreCase) ||
        _ambiente.Equals("HML", StringComparison.OrdinalIgnoreCase);
}
