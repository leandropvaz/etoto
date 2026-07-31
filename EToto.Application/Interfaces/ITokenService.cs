namespace EToto.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(Guid tenantId, string email, string perfil);

    // #1b: gera token com múltiplas claims ClaimTypes.Role (uma por perfil).
    string GenerateToken(Guid tenantId, string email, IEnumerable<string> perfis);
}
