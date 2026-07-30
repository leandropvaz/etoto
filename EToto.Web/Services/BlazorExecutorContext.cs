using EToto.Domain.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;

namespace EToto.Web.Services
{
    // IExecutorContext que lê o usuário corrente da memória do
    // LototoAuthenticationStateProvider (sem bloquear em Task), para evitar deadlock
    // quando o SaveChanges do interceptor de auditoria pergunta "quem é o executor"
    // dentro do próprio fluxo de login (#5a/#6a).
    //
    // Trade-off: no momento do PRIMEIRO login (antes do SignInAsync gravar o
    // ClaimsPrincipal em memória) ainda não há usuário identificado — entradas de
    // auditoria geradas exatamente nesse intervalo ficam anônimas. A linha alterada
    // (Usuarios.Id=X) já dá rastreabilidade suficiente.
    public class BlazorExecutorContext : IExecutorContext
    {
        private readonly LototoAuthenticationStateProvider _provider;

        public BlazorExecutorContext(AuthenticationStateProvider provider)
        {
            _provider = (LototoAuthenticationStateProvider)provider;
        }

        public int? UsuarioIdAtual => _provider.UsuarioIdAtualEmMemoria;
    }
}
