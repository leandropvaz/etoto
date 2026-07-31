using EToto.Application.Dto;
using EToto.Application.Helpers;
using EToto.Application.Interfaces;
using EToto.Domain.Entities;
using EToto.Domain.Enums;
using EToto.Domain.Interfaces;

namespace EToto.Application.Services
{
    public class PleAppService : IPleAppService
    {
        private readonly IPleRepository _repo;
        private readonly IPlantaRepository _plantaRepo;
        private readonly IUsuarioRepository _usuarioRepo;

        public PleAppService(IPleRepository repo, IPlantaRepository plantaRepo, IUsuarioRepository usuarioRepo)
        {
            _repo = repo;
            _plantaRepo = plantaRepo;
            _usuarioRepo = usuarioRepo;
        }

        public async Task<List<PleDto>> GetByPlantAsync(int plantId, CancellationToken ct = default)
        {
            var list = await _repo.GetByPlantAsync(plantId, ct);
            return list.Select(MapToDto).ToList();
        }

        public async Task<PleDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var ple = await _repo.GetByIdAsync(id, ct);
            return ple is null ? null : MapToDto(ple);
        }

        public async Task<PleDto?> GetByIdWithHistoricoAsync(Guid id, CancellationToken ct = default)
        {
            var ple = await _repo.GetByIdWithHistoricoAsync(id, ct);
            return ple is null ? null : MapToDto(ple);
        }

        public Task<List<Guid>> GetEquipamentosComPleAtivoAsync(int plantId, CancellationToken ct = default)
        {
            return _repo.GetEquipamentosComPleAtivoAsync(plantId, ct);
        }

        public Task<Dictionary<Guid, StatusPle>> GetEquipamentosComPleAtivoStatusAsync(int plantId, CancellationToken ct = default)
        {
            return _repo.GetEquipamentosComPleAtivoStatusAsync(plantId, ct);
        }

        public async Task<PleDto> CreateAsync(PleCreateDto dto, CancellationToken ct = default)
        {
            var planta = await _plantaRepo.GetByIdAsync(dto.PlantaId);
            var codigoPlanta = planta?.Codigo?.Trim().ToUpperInvariant() ?? dto.PlantaId.ToString();
            var seq = await _repo.GetProximoNumeroAsync(codigoPlanta, ct);
            var numero = $"PLE-{codigoPlanta}-{seq:D3}";

            var ple = new Ple
            {
                Id = Guid.NewGuid(),
                Numero = numero,
                PlantaId = dto.PlantaId,
                DataInicio = dto.DataInicio,
                Status = StatusPle.Criado,
                CriadoPorId = dto.CriadoPorId,
                DataCriacao = DateTimeHelper.AgoraBrasilia(),
                Equipamentos = dto.EquipamentoIds.Select(eqId => new PleEquipamento
                {
                    EquipamentoId = eqId
                }).ToList(),
                UsuariosPermitidos = dto.UsuariosPermitidosIds.Select(uid => new PleUsuarioPermitido
                {
                    UsuarioId = uid
                }).ToList()
            };

            await _repo.AddAsync(ple, ct);

            await _repo.AddHistoricoAsync(new PleHistorico
            {
                Id = Guid.NewGuid(),
                PleId = ple.Id,
                UsuarioId = dto.CriadoPorId,
                Acao = "Criado",
                StatusNovo = StatusPle.Criado.ToString(),
                Detalhes = $"PLE {numero} criado",
                DataAcao = DateTimeHelper.AgoraBrasilia(),
            }, ct);

            await _repo.SaveChangesAsync(ct);
            return (await GetByIdAsync(ple.Id, ct))!;
        }

        public async Task<PleDto> UpdateAsync(PleUpdateDto dto, CancellationToken ct = default)
        {
            var ple = await _repo.GetByIdAsync(dto.Id, ct)
                ?? throw new InvalidOperationException("PLE não encontrado.");

            if (ple.Status != StatusPle.Criado)
                throw new InvalidOperationException("Só é possível editar um PLE com status Criado.");

            ple.DataInicio = dto.DataInicio;
            ple.ModificadoPorId = dto.ModificadoPorId;
            ple.DataModificacao = DateTimeHelper.AgoraBrasilia();

            // Atualiza equipamentos
            ple.Equipamentos.Clear();
            foreach (var eqId in dto.EquipamentoIds)
                ple.Equipamentos.Add(new PleEquipamento { PleId = ple.Id, EquipamentoId = eqId });

            // Atualiza delegados
            ple.UsuariosPermitidos.Clear();
            foreach (var uid in dto.UsuariosPermitidosIds)
                ple.UsuariosPermitidos.Add(new PleUsuarioPermitido { PleId = ple.Id, UsuarioId = uid });

            await _repo.UpdateAsync(ple, ct);

            await _repo.AddHistoricoAsync(new PleHistorico
            {
                Id = Guid.NewGuid(),
                PleId = ple.Id,
                UsuarioId = dto.ModificadoPorId,
                Acao = "Modificado",
                StatusAnterior = ple.Status.ToString(),
                StatusNovo = ple.Status.ToString(),
                Detalhes = "PLE editado",
                DataAcao = DateTimeHelper.AgoraBrasilia(),
            }, ct);

            await _repo.SaveChangesAsync(ct);
            return (await GetByIdAsync(ple.Id, ct))!;
        }

        public bool UsuarioTemPermissao(PleDto ple, int usuarioId)
        {
            return ple.CriadoPorId == usuarioId || ple.UsuariosPermitidos.Any(u => u.UsuarioId == usuarioId);
        }

        public async Task AlterarStatusAsync(Guid pleId, StatusPle novoStatus, int usuarioId, string? motivo = null, CancellationToken ct = default)
        {
            var ple = await _repo.GetByIdAsync(pleId, ct)
                ?? throw new InvalidOperationException("PLE não encontrado.");

            // Permissoes por novo status:
            // - Finalizado: EXCLUSIVO do Comando Central com acesso a planta (nem Admin/SuperGestor finalizam).
            // - Cancelado: criador, delegado, Admin, SuperGestor ou Lider de Bloqueio (UsuarioFinal+treino)
            //              com acesso a planta. SOMENTE a partir de EmAndamento (não em InicioDesbloqueio).
            // - InicioDesbloqueio: criador, delegado, Admin, SuperGestor ou Lider de Bloqueio (UsuarioFinal+treino) com acesso a planta.
            // - EmAndamento (iniciar): criador, delegado, Admin, SuperGestor.
            if (novoStatus == StatusPle.Finalizado)
            {
                var usuario = await _usuarioRepo.ObterComPlantasAsync(usuarioId);
                var plantasIds = usuario?.PlantasAssociadas.Select(pa => pa.PlantaId).ToHashSet() ?? new HashSet<int>();
                if (usuario?.PlantaId.HasValue == true) plantasIds.Add(usuario.PlantaId.Value);

                var podeFinalizar = usuario != null
                    && usuario.EhComandoCentral
                    && plantasIds.Contains(ple.PlantaId);

                if (!podeFinalizar)
                    throw new InvalidOperationException("Somente o Comando Central pode finalizar este bloqueio.");
            }
            else
            {
                var temPermissao = TemPermissao(ple, usuarioId);

                if (!temPermissao)
                {
                    var usuario = await _usuarioRepo.ObterComPlantasAsync(usuarioId);
                    if (usuario != null)
                    {
                        // Admin / SuperGestor podem iniciar, iniciar desbloqueio e cancelar
                        if (usuario.EhAdministrador || usuario.EhSuperGestor)
                        {
                            temPermissao = true;
                        }
                        // Lider de Bloqueio (com treinamento) com acesso a planta pode iniciar desbloqueio e cancelar
                        else if ((novoStatus == StatusPle.InicioDesbloqueio || novoStatus == StatusPle.Cancelado)
                                 && usuario.EhUsuarioFinal && usuario.TreinamentoConcluido)
                        {
                            var plantasIds = usuario.PlantasAssociadas.Select(pa => pa.PlantaId).ToHashSet();
                            if (usuario.PlantaId.HasValue) plantasIds.Add(usuario.PlantaId.Value);
                            if (plantasIds.Contains(ple.PlantaId)) temPermissao = true;
                        }
                    }
                }

                if (!temPermissao)
                    throw new InvalidOperationException("Somente o criador ou usuários autorizados podem alterar este bloqueio.");
            }

            if (ple.Status == StatusPle.Finalizado)
                throw new InvalidOperationException("Não é possível alterar um PLE finalizado.");

            if (ple.Status == StatusPle.Cancelado)
                throw new InvalidOperationException("Não é possível alterar um PLE cancelado.");

            // Transicoes validas
            if (novoStatus == StatusPle.Finalizado && ple.Status != StatusPle.InicioDesbloqueio)
                throw new InvalidOperationException("Só é possível finalizar um PLE em \"Início do Desbloqueio\".");

            if (novoStatus == StatusPle.InicioDesbloqueio && ple.Status != StatusPle.EmAndamento)
                throw new InvalidOperationException("Só é possível iniciar o desbloqueio de um PLE em andamento.");

            if (novoStatus == StatusPle.Cancelado && ple.Status != StatusPle.EmAndamento)
                throw new InvalidOperationException("Só é possível cancelar um PLE em andamento. Após o início do desbloqueio, o fluxo segue para finalização.");

            var statusAnterior = ple.Status;
            ple.Status = novoStatus;
            ple.ModificadoPorId = usuarioId;
            ple.DataModificacao = DateTimeHelper.AgoraBrasilia();

            if (novoStatus == StatusPle.Cancelado)
                ple.MotivoCancelamento = motivo;

            // FinalizadoPor passa a registrar quem concluiu definitivamente (Comando Central).
            if (novoStatus == StatusPle.Finalizado)
            {
                ple.FinalizadoPorId = usuarioId;
                ple.DataFinalizacao = DateTimeHelper.AgoraBrasilia();
                ple.DataFim ??= DateTimeHelper.AgoraBrasilia();
            }

            string acao = novoStatus switch
            {
                StatusPle.EmAndamento => "Iniciado",
                StatusPle.Cancelado => "Cancelado",
                StatusPle.InicioDesbloqueio => "InicioDesbloqueio",
                StatusPle.Finalizado => "Finalizado",
                _ => "StatusAlterado"
            };

            await _repo.UpdateAsync(ple, ct);

            await _repo.AddHistoricoAsync(new PleHistorico
            {
                Id = Guid.NewGuid(),
                PleId = ple.Id,
                UsuarioId = usuarioId,
                Acao = acao,
                StatusAnterior = statusAnterior.ToString(),
                StatusNovo = novoStatus.ToString(),
                Detalhes = motivo,
                DataAcao = DateTimeHelper.AgoraBrasilia(),
            }, ct);

            await _repo.SaveChangesAsync(ct);
        }

        public async Task SoftDeleteAsync(Guid id, int usuarioId, CancellationToken ct = default)
        {
            var ple = await _repo.GetByIdAsync(id, ct)
                ?? throw new InvalidOperationException("PLE não encontrado.");

            if (ple.Status == StatusPle.Finalizado)
                throw new InvalidOperationException("Não é possível excluir um PLE finalizado.");

            ple.IsDeleted = true;
            ple.ModificadoPorId = usuarioId;
            ple.DataModificacao = DateTimeHelper.AgoraBrasilia();

            await _repo.UpdateAsync(ple, ct);
            await _repo.AddHistoricoAsync(new PleHistorico
            {
                Id = Guid.NewGuid(),
                PleId = ple.Id,
                UsuarioId = usuarioId,
                Acao = "Excluído",
                StatusAnterior = ple.Status.ToString(),
                DataAcao = DateTimeHelper.AgoraBrasilia(),
            }, ct);
            await _repo.SaveChangesAsync(ct);
        }

        private static bool TemPermissao(Ple ple, int usuarioId)
        {
            return ple.CriadoPorId == usuarioId || ple.UsuariosPermitidos.Any(u => u.UsuarioId == usuarioId);
        }

        private static PleDto MapToDto(Ple ple)
        {
            return new PleDto
            {
                Id = ple.Id,
                Numero = ple.Numero,
                PlantaId = ple.PlantaId,
                PlantaNome = ple.Planta?.Nome ?? "",
                Equipamentos = ple.Equipamentos?.Select(pe => new PleEquipamentoDto
                {
                    EquipamentoId = pe.EquipamentoId,
                    Tag = pe.Equipamento?.Tag ?? "",
                    Nome = pe.Equipamento?.EquipmentName ?? "",
                    TipoEnergia = pe.Equipamento?.EnergyType ?? "",
                    DispositivoBloqueio = pe.Equipamento?.IsolationDeviceDescription ?? pe.Equipamento?.LockoutType ?? "",
                    TipoBloqueio = pe.Equipamento?.LockoutType ?? "",
                    TagDispositivo = pe.Equipamento?.IsolationDeviceTag ?? "",
                    DescEnergia = pe.Equipamento?.HazardDescription ?? "",
                }).ToList() ?? new(),
                DataInicio = ple.DataInicio,
                DataFim = ple.DataFim,
                Status = ple.Status,
                MotivoCancelamento = ple.MotivoCancelamento,
                CriadoPorNome = ple.CriadoPor?.NomeCompleto ?? "",
                CriadoPorId = ple.CriadoPorId,
                ModificadoPorNome = ple.ModificadoPor?.NomeCompleto,
                FinalizadoPorNome = ple.FinalizadoPor?.NomeCompleto,
                DataCriacao = ple.DataCriacao,
                DataModificacao = ple.DataModificacao,
                DataFinalizacao = ple.DataFinalizacao,
                UsuariosPermitidos = ple.UsuariosPermitidos?.Select(up => new PleUsuarioPermitidoDto
                {
                    UsuarioId = up.UsuarioId,
                    NomeCompleto = up.Usuario?.NomeCompleto ?? "",
                }).ToList() ?? new(),
                Historico = ple.Historico?.Select(h => new PleHistoricoDto
                {
                    Id = h.Id,
                    Acao = h.Acao,
                    StatusAnterior = h.StatusAnterior,
                    StatusNovo = h.StatusNovo,
                    Detalhes = h.Detalhes,
                    UsuarioNome = h.Usuario?.NomeCompleto ?? "",
                    DataAcao = h.DataAcao,
                }).ToList() ?? new()
            };
        }
    }
}
