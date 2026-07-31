using EToto.Domain.Enums;
using EToto.Domain.Interfaces;
using EToto.Web.Dto;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using static EToto.Web.Services.LototoAuthenticationStateProvider;

namespace EToto.Web.Services
{
    public class AuthService
    {
        private readonly IUsuarioRepository _usuarios;
        private readonly IUnitOfWork _uow;
        private readonly LototoAuthenticationStateProvider _authProvider;
        private readonly LototoConfig _config;

        public AuthService(
            IUsuarioRepository usuarios,
            IUnitOfWork uow,
            AuthenticationStateProvider authProvider,
            IOptions<LototoConfig> config)
        {
            _usuarios = usuarios;
            _uow = uow;
            _authProvider = (LototoAuthenticationStateProvider)authProvider;
            _config = config.Value;
        }

        /// <summary>
        /// Login permitindo Super Gestor sem planta.
        /// Para Administrador/Usuário, planta continua obrigatória.
        /// Permite login sem senha para usuários específicos se configurado.
        /// </summary>
        public async Task<LoginResult> LoginAsync(string login, string senha, int? plantaId)
        {
            login = (login ?? string.Empty).Trim();
            bool permiteLoginSemSenha = _config.PermiteSemSenha(login);

            if (!permiteLoginSemSenha && string.IsNullOrWhiteSpace(senha))
                return LoginResult.Fail("Informe o login e a senha.");

            if (string.IsNullOrWhiteSpace(login))
                return LoginResult.Fail("Informe o login.");

            var usuario = await _usuarios.ObterPorLoginAsync(login);
            if (usuario == null)
                return LoginResult.Fail("Credenciais inválidas.");

            if (!permiteLoginSemSenha || !string.IsNullOrWhiteSpace(senha))
            {
                if (usuario.SenhaHash != GerarHash(senha))
                    return LoginResult.Fail("Credenciais inválidas.");
            }

            // #2: validade do acesso (vínculo Terceiro).
            int? diasParaVencer = null;
            var statusValidade = usuario.StatusValidade();
            if (statusValidade == StatusValidadeAcesso.Vencido)
                return LoginResult.Vencido();

            if (statusValidade == StatusValidadeAcesso.Vencendo)
            {
                diasParaVencer = Math.Max(0,
                    (int)Math.Ceiling((usuario.DataValidadeAcesso!.Value.Date - DateTime.UtcNow.Date).TotalDays));
            }

            // #3: validade do treinamento — apenas para perfis que exigem.
            int? diasParaVencerTreinamento = null;
            if (usuario.ExigeTreinamentoValido())
            {
                var statusTreinamento = usuario.StatusValidadeTreinamento();
                if (statusTreinamento == StatusValidadeAcesso.Vencido)
                    return LoginResult.TreinamentoExpirado();

                if (statusTreinamento == StatusValidadeAcesso.Vencendo)
                {
                    diasParaVencerTreinamento = Math.Max(0,
                        (int)Math.Ceiling((usuario.DataValidadeTreinamento!.Value.Date - DateTime.UtcNow.Date).TotalDays));
                }
            }

            // Coleção de perfis (#1b); fallback ao campo legado quando vazia (compat #1a→#1b).
            var perfisColecao = usuario.Perfis?.Select(p => p.Perfil).Distinct().ToList()
                                ?? new List<PerfilUsuario>();
            if (perfisColecao.Count == 0)
                perfisColecao.Add(usuario.Perfil);

            // Perfil "primário" para retrocompat: SuperGestor quando presente, senão o primeiro.
            var perfil = perfisColecao.Contains(PerfilUsuario.SuperGestor)
                ? PerfilUsuario.SuperGestor
                : perfisColecao[0];

            // ✅ Obter todas as plantas do usuário
            var plantasIds = usuario.PlantasAssociadas?.Select(pa => pa.PlantaId).ToList()
                             ?? new List<int>();

            // Se tem PlantaId fixa (retrocompat), adicionar
            if (usuario.PlantaId.HasValue && !plantasIds.Contains(usuario.PlantaId.Value))
            {
                plantasIds.Add(usuario.PlantaId.Value);
            }

            // Validação de acesso para não-SuperGestor
            if (perfil != PerfilUsuario.SuperGestor)
            {
                if (plantaId is null || plantaId == 0)
                    return LoginResult.Fail("Selecione a planta.");

                // Verificar se tem acesso à planta selecionada
                bool temAcesso = plantasIds.Contains(plantaId.Value);

                if (!temAcesso)
                    return LoginResult.Fail("Você não tem acesso a esta planta.");
            }
            else
            {
                plantaId = null;
            }

            // ✅ Obter nomes das plantas
            var plantaNome = perfil == PerfilUsuario.SuperGestor
                ? "Todas as plantas"
                : (usuario.PlantasAssociadas?.Any() == true
                    ? string.Join(" / ", usuario.PlantasAssociadas
                        .Where(pa => pa.Planta != null)
                        .Select(pa => pa.Planta!.Nome)
                        .OrderBy(n => n))
                    : usuario.Planta?.Nome ?? "Sem planta");

            var userInfo = new SerializableUser
            {
                Nome = usuario.NomeCompleto,
                UserId = usuario.Id,
                Perfil = (int)perfil,
                PerfilNome = perfil.ToString(),
                Perfis = perfisColecao.Select(p => (int)p).ToList(),
                PerfisNomes = perfisColecao.Select(p => p.ToString()).ToList(),
                PlantaId = plantaId, // Planta selecionada no login
                PlantaNome = plantaNome,
                PlantasIds = plantasIds,
                TreinamentoConcluido = usuario.TreinamentoConcluido
            };

            // #6a: registra último login bem-sucedido.
            usuario.DataUltimoLogin = DateTime.UtcNow;
            _usuarios.Update(usuario);
            await _uow.CommitAsync();

            await _authProvider.SignInAsync(userInfo);
            return LoginResult.Ok(diasParaVencer, diasParaVencerTreinamento);
        }
        public async Task LogoutAsync()
        {
            await _authProvider.SignOutAsync();
        }

        private string GerarHash(string senha)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(senha));
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }
    }
}