using EToto.Application.Dto;
using EToto.Domain.Entities;
using EToto.Domain.Enums;
using EToto.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace EToto.Application.Services
{
    public class UsuariosService
    {
        private readonly IUsuarioRepository _repo;
        private readonly IUnitOfWork _uow;

        public UsuariosService(IUsuarioRepository repo, IUnitOfWork uow)
        {
            _repo = repo;
            _uow = uow;
        }

        public async Task<List<UsuariosDto>> ListarAsync()
        {
            var usuarios = await _repo.ListarComPlantasAsync();
            return usuarios.Select(MapToDto).ToList();
        }

        public async Task CriarAsync(UsuariosDto dto)
        {
            var perfis = ResolverPerfis(dto);
            var agora = DateTime.UtcNow;
            var tipoVinculo = (TipoVinculo)dto.TipoVinculo;

            // #2: regra de domínio — Terceiro exige NomeEmpresa e DataValidadeAcesso.
            VinculoValidacao.ValidarTerceiro(tipoVinculo, dto.NomeEmpresa, dto.DataValidadeAcesso);
            // #2: validade de acesso do Terceiro limitada a 6 meses (regra da tela de cadastro).
            VinculoValidacao.ValidarLimiteValidadeAcesso(tipoVinculo, dto.DataValidadeAcesso);

            var entity = new Usuarios
            {
                Login = dto.Login,
                NomeCompleto = dto.NomeCompleto,
                Ativa = dto.Ativa,
                PlantaId = dto.PlantaId,
                TreinamentoConcluido = dto.TreinamentoConcluido,
                DataTreinamento = dto.DataTreinamento,
                DataValidadeTreinamento = dto.DataValidadeTreinamento,
                SenhaHash = GerarHash(dto.Senha),
                DataCriacao = agora,
                CriadoPorId = dto.ExecutadoPorId,
                CriadoEm = agora,
                TipoVinculo = tipoVinculo,
                NomeEmpresa = tipoVinculo == TipoVinculo.Terceiro ? dto.NomeEmpresa?.Trim() : null,
                DataValidadeAcesso = tipoVinculo == TipoVinculo.Terceiro ? dto.DataValidadeAcesso : null
            };

            // Aplica regra de domínio (SuperGestor exclusivo) e sincroniza campo legado.
            entity.DefinirPerfis(perfis);

            await _repo.AddAsync(entity);
            await _uow.CommitAsync();

            if (dto.PlantasIds?.Any() == true)
            {
                foreach (var plantaId in dto.PlantasIds)
                {
                    entity.PlantasAssociadas.Add(new UsuarioPlanta
                    {
                        UsuarioId = entity.Id,
                        PlantaId = plantaId,
                        DataAssociacao = agora
                    });
                }
                await _uow.CommitAsync();
            }

            // Propaga Id e auditoria de volta para o caller.
            dto.Id = entity.Id;
            dto.CriadoEm = entity.CriadoEm;
            dto.CriadoPorId = entity.CriadoPorId;
        }

        public async Task AtualizarAsync(UsuariosDto dto)
        {
            var usuario = await _repo.ObterComPlantasAsync(dto.Id);
            if (usuario == null)
                throw new Exception("Usuário não encontrado.");

            var perfis = ResolverPerfis(dto);
            var agora = DateTime.UtcNow;
            var tipoVinculo = (TipoVinculo)dto.TipoVinculo;

            // #2: regra de domínio — Terceiro exige NomeEmpresa e DataValidadeAcesso.
            VinculoValidacao.ValidarTerceiro(tipoVinculo, dto.NomeEmpresa, dto.DataValidadeAcesso);
            // #2: validade de acesso do Terceiro limitada a 6 meses (regra da tela de cadastro).
            VinculoValidacao.ValidarLimiteValidadeAcesso(tipoVinculo, dto.DataValidadeAcesso);

            usuario.Login = dto.Login;
            usuario.NomeCompleto = dto.NomeCompleto;
            usuario.Ativa = dto.Ativa;
            usuario.PlantaId = dto.PlantaId;
            usuario.TreinamentoConcluido = dto.TreinamentoConcluido;
            usuario.DataTreinamento = dto.DataTreinamento;
            usuario.DataValidadeTreinamento = dto.DataValidadeTreinamento;
            usuario.DataAtualizacao = agora;
            usuario.AlteradoPorId = dto.ExecutadoPorId;
            usuario.AlteradoEm = agora;
            usuario.TipoVinculo = tipoVinculo;
            usuario.NomeEmpresa = tipoVinculo == TipoVinculo.Terceiro ? dto.NomeEmpresa?.Trim() : null;
            usuario.DataValidadeAcesso = tipoVinculo == TipoVinculo.Terceiro ? dto.DataValidadeAcesso : null;

            usuario.DefinirPerfis(perfis);

            if (!string.IsNullOrWhiteSpace(dto.Senha))
                usuario.SenhaHash = GerarHash(dto.Senha);

            usuario.PlantasAssociadas.Clear();

            if (dto.PlantasIds?.Any() == true)
            {
                foreach (var plantaId in dto.PlantasIds)
                {
                    usuario.PlantasAssociadas.Add(new UsuarioPlanta
                    {
                        UsuarioId = usuario.Id,
                        PlantaId = plantaId,
                        DataAssociacao = agora
                    });
                }
            }

            _repo.Update(usuario);
            await _uow.CommitAsync();

            dto.AlteradoEm = usuario.AlteradoEm;
            dto.AlteradoPorId = usuario.AlteradoPorId;
        }

        public async Task RemoverAsync(int id)
        {
            var usuario = await _repo.GetByIdAsync(id);
            if (usuario == null)
                return;

            usuario.Ativa = false;
            usuario.DataAtualizacao = DateTime.UtcNow;

            _repo.Update(usuario);
            await _uow.CommitAsync();
        }

        private static List<PerfilUsuario> ResolverPerfis(UsuariosDto dto)
        {
            var fromDto = dto.Perfis?
                .Select(p => (PerfilUsuario)p)
                .Distinct()
                .ToList() ?? new List<PerfilUsuario>();

            if (fromDto.Count > 0)
                return fromDto;

            // Fallback ao campo legado quando o caller não preencheu a lista.
            return new List<PerfilUsuario> { (PerfilUsuario)dto.Perfil };
        }

        private static UsuariosDto MapToDto(Usuarios u)
        {
            var perfisIds = u.Perfis?.Select(p => (int)p.Perfil).ToList()
                            ?? new List<int>();

            return new UsuariosDto
            {
                Id = u.Id,
                Login = u.Login,
                NomeCompleto = u.NomeCompleto,
                Perfil = (int)u.Perfil,
                Perfis = perfisIds,
                Ativa = u.Ativa,
                PlantaId = u.PlantaId,
                PlantasIds = u.PlantasAssociadas?.Select(pa => pa.PlantaId).ToList()
                              ?? new List<int>(),
                TreinamentoConcluido = u.TreinamentoConcluido,
                DataTreinamento = u.DataTreinamento,
                DataValidadeTreinamento = u.DataValidadeTreinamento,
                TipoVinculo = (int)u.TipoVinculo,
                NomeEmpresa = u.NomeEmpresa,
                DataValidadeAcesso = u.DataValidadeAcesso,
                CriadoPorId = u.CriadoPorId,
                CriadoPorNome = u.CriadoPor?.NomeCompleto,
                CriadoEm = u.CriadoEm,
                AlteradoPorId = u.AlteradoPorId,
                AlteradoPorNome = u.AlteradoPor?.NomeCompleto,
                AlteradoEm = u.AlteradoEm
            };
        }

        private string GerarHash(string senha)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(senha));
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
