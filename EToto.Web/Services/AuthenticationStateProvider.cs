using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Claims;

namespace EToto.Web.Services
{
    public class LototoAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ProtectedSessionStorage _session;
        private const string SessionKey = "lototo.auth.user";

        private ClaimsPrincipal _currentUser = new ClaimsPrincipal(new ClaimsIdentity());

        public LototoAuthenticationStateProvider(ProtectedSessionStorage session)
        {
            _session = session;
        }

        // Snapshot síncrono do UserId em memória — usado pelo IExecutorContext da auditoria (#5a/#6a)
        // para evitar deadlock ao bloquear na Task de GetAuthenticationStateAsync.
        // Retorna null se ainda não houve SignInAsync neste circuito.
        public int? UsuarioIdAtualEmMemoria
        {
            get
            {
                if (_currentUser.Identity?.IsAuthenticated != true) return null;
                var idClaim = _currentUser.FindFirst("UserId")?.Value;
                return int.TryParse(idClaim, out var id) ? id : null;
            }
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // Se já temos em memória, devolve
            if (_currentUser.Identity?.IsAuthenticated == true)
                return new AuthenticationState(_currentUser);

            try
            {
                // Tenta carregar do SessionStorage
                var result = await _session.GetAsync<SerializableUser>(SessionKey);

                if (result.Success && result.Value != null)
                {
                    _currentUser = LototoClaimsBuilder.Build(result.Value);
                }
                else
                {
                    _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
                }
            }
            catch (InvalidOperationException)
            {
                // Durante pré-render, JS ainda não está disponível
                _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
            }

            return new AuthenticationState(_currentUser);
        }

        public async Task SignInAsync(SerializableUser user)
        {
            _currentUser = LototoClaimsBuilder.Build(user);
            await _session.SetAsync(SessionKey, user);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
        }

        public async Task SignOutAsync()
        {
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
            await _session.DeleteAsync(SessionKey);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
        }

        // ✅ Classe serializável para guardar no SessionStorage
        public class SerializableUser
        {
            public string Nome { get; set; } = "";

            // Perfil "primário" (legado): SuperGestor quando presente, senão o primeiro da coleção.
            // Mantido para retrocompat com claims/leituras antigas (`FindFirst("Perfil")`).
            public int Perfil { get; set; }
            public string PerfilNome { get; set; } = "";

            // Múltiplos perfis (#1b)
            public List<int> Perfis { get; set; } = new();
            public List<string> PerfisNomes { get; set; } = new();

            public int UserId { get; set; }
            public int? PlantaId { get; set; }
            public string? PlantaNome { get; set; }
            public List<int> PlantasIds { get; set; } = new();
            public bool TreinamentoConcluido { get; set; }
        }
    }
}
