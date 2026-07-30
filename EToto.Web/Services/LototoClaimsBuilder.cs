using System.Security.Claims;
using static EToto.Web.Services.LototoAuthenticationStateProvider;

namespace EToto.Web.Services
{
    // Constrói o ClaimsPrincipal a partir do SerializableUser.
    // Emite UMA Claim ClaimTypes.Role POR PERFIL (#1b) — o que faz `user.IsInRole("...")` aceitar
    // qualquer combinação de perfis transparentemente nas telas/menu existentes.
    public static class LototoClaimsBuilder
    {
        public const string AuthenticationType = "LototoAuth";

        public static ClaimsPrincipal Build(SerializableUser user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Nome),
                new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new("UserId", user.UserId.ToString()),
                new("Perfil", user.Perfil.ToString()),
                new("TreinamentoConcluido", user.TreinamentoConcluido.ToString())
            };

            // Múltiplos perfis: 1 Claim Role por perfil. Fallback ao campo legado quando a lista estiver vazia.
            var perfisNomes = user.PerfisNomes is { Count: > 0 }
                ? user.PerfisNomes
                : new List<string> { user.PerfilNome };

            foreach (var nome in perfisNomes.Where(n => !string.IsNullOrWhiteSpace(n)).Distinct())
            {
                claims.Add(new Claim(ClaimTypes.Role, nome));
            }

            // Claim auxiliar com a lista (útil para depuração/exibição)
            foreach (var id in user.Perfis.Distinct())
            {
                claims.Add(new Claim("Perfis", id.ToString()));
            }

            // Planta selecionada no login (única claim para retrocompat)
            if (user.PlantaId.HasValue)
            {
                claims.Add(new Claim("PlantaId", user.PlantaId.Value.ToString()));
            }

            if (!string.IsNullOrWhiteSpace(user.PlantaNome))
            {
                claims.Add(new Claim("PlantaNome", user.PlantaNome));
            }

            // Todas as plantas associadas como claims separadas
            foreach (var plantaId in user.PlantasIds.Distinct())
            {
                claims.Add(new Claim("PlantasIds", plantaId.ToString()));
            }

            return new ClaimsPrincipal(new ClaimsIdentity(claims, AuthenticationType));
        }
    }
}
