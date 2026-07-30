using EToto.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EToto.Application.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(Guid tenantId, string email, string perfil)
        => GenerateToken(tenantId, email, new[] { perfil });

    public string GenerateToken(Guid tenantId, string email, IEnumerable<string> perfis)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, email),
            new("TenantId", tenantId.ToString())
        };

        // 1 Claim ClaimTypes.Role por perfil (#1b).
        foreach (var p in perfis.Where(p => !string.IsNullOrWhiteSpace(p)).Distinct())
        {
            claims.Add(new Claim(ClaimTypes.Role, p));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
