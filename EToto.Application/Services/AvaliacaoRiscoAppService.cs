using EToto.Application.Dto;
using EToto.Application.Helpers;
using EToto.Application.Interfaces;
using EToto.Domain.Entities;
using EToto.Domain.Enums;
using EToto.Domain.Interfaces;

namespace EToto.Application.Services
{
    public class AvaliacaoRiscoAppService : IAvaliacaoRiscoAppService
    {
        private readonly IAvaliacaoRiscoRepository _repo;

        public AvaliacaoRiscoAppService(IAvaliacaoRiscoRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<AvaliacaoRiscoDto>> GetAllAsync(CancellationToken ct = default)
        {
            var list = await _repo.GetAllAsync(ct);
            return list.Select(Map).ToList();
        }

        public async Task<AvaliacaoRiscoDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var ar = await _repo.GetByIdAsync(id, ct);
            return ar is null ? null : Map(ar);
        }

        public async Task<AvaliacaoRiscoDto?> GetByIdWithHistoricoAsync(Guid id, CancellationToken ct = default)
        {
            var ar = await _repo.GetByIdWithHistoricoAsync(id, ct);
            return ar is null ? null : Map(ar);
        }

        public async Task<AvaliacaoRiscoDto> CreateAsync(AvaliacaoRiscoCreateDto dto, CancellationToken ct = default)
        {
            var seq = await _repo.GetProximoNumeroAsync(ct);

            var ar = new AvaliacaoRisco
            {
                Id = Guid.NewGuid(),
                Numero = $"AR-{seq:D3}",
                Departamento = dto.Departamento,
                Operacao = dto.Operacao,
                Tarefa = dto.Tarefa,
                Data = dto.Data,
                Observacoes = dto.Observacoes,
                Status = StatusAvaliacaoRisco.Ativa,
                CriadoPorId = dto.CriadoPorId,
                DataCriacao = DateTimeHelper.AgoraBrasilia(),
                Equipamentos = dto.EquipamentoIds.Select(eqId => new AvaliacaoRiscoEquipamento
                {
                    EquipamentoId = eqId
                }).ToList(),
                Itens = dto.Itens.Select((item, idx) => new AvaliacaoRiscoItem
                {
                    Id = Guid.NewGuid(), Ordem = idx + 1,
                    Tarefa = item.Tarefa, Perigo = item.Perigo, NumeroExpostos = item.NumeroExpostos,
                    GravidadeAntes = item.GravidadeAntes, ProbabilidadeAntes = item.ProbabilidadeAntes, NivelRiscoAntes = item.NivelRiscoAntes,
                    MedidasProtecao = item.MedidasProtecao,
                    GravidadeDepois = item.GravidadeDepois, ProbabilidadeDepois = item.ProbabilidadeDepois, NivelRiscoDepois = item.NivelRiscoDepois,
                }).ToList()
            };

            await _repo.AddAsync(ar, ct);
            await _repo.AddHistoricoAsync(new AvaliacaoRiscoHistorico
            {
                Id = Guid.NewGuid(), AvaliacaoRiscoId = ar.Id, UsuarioId = dto.CriadoPorId,
                Acao = "Criado", Detalhes = $"Avaliação {ar.Numero} criada", DataAcao = DateTimeHelper.AgoraBrasilia(),
            }, ct);
            await _repo.SaveChangesAsync(ct);
            return (await GetByIdWithHistoricoAsync(ar.Id, ct))!;
        }

        public async Task<AvaliacaoRiscoDto> UpdateAsync(AvaliacaoRiscoUpdateDto dto, CancellationToken ct = default)
        {
            // 1. Atualiza campos do cabeçalho via SQL
            await _repo.UpdateCabecalhoAsync(dto.Id, dto.Departamento, dto.Operacao, dto.Tarefa, dto.Data, dto.Observacoes, dto.ModificadoPorId, ct);

            // 2. Remove itens e equipamentos antigos via SQL
            await _repo.RemoveItensAsync(dto.Id, ct);
            await _repo.RemoveEquipamentosAsync(dto.Id, ct);

            // 3. Insere novos equipamentos
            if (dto.EquipamentoIds.Any())
            {
                await _repo.AddEquipamentosAsync(dto.EquipamentoIds.Select(eqId => new AvaliacaoRiscoEquipamento
                {
                    AvaliacaoRiscoId = dto.Id, EquipamentoId = eqId
                }).ToList(), ct);
            }

            // 4. Insere novos itens
            var novosItens = dto.Itens.Select((item, idx) => new AvaliacaoRiscoItem
            {
                Id = Guid.NewGuid(), AvaliacaoRiscoId = dto.Id, Ordem = idx + 1,
                Tarefa = item.Tarefa, Perigo = item.Perigo, NumeroExpostos = item.NumeroExpostos,
                GravidadeAntes = item.GravidadeAntes, ProbabilidadeAntes = item.ProbabilidadeAntes, NivelRiscoAntes = item.NivelRiscoAntes,
                MedidasProtecao = item.MedidasProtecao,
                GravidadeDepois = item.GravidadeDepois, ProbabilidadeDepois = item.ProbabilidadeDepois, NivelRiscoDepois = item.NivelRiscoDepois,
            }).ToList();

            await _repo.AddItensAsync(novosItens, ct);

            // 4. Histórico
            await _repo.AddHistoricoAsync(new AvaliacaoRiscoHistorico
            {
                Id = Guid.NewGuid(), AvaliacaoRiscoId = dto.Id, UsuarioId = dto.ModificadoPorId,
                Acao = "Modificado", Detalhes = "Avaliação editada", DataAcao = DateTimeHelper.AgoraBrasilia(),
            }, ct);
            await _repo.SaveChangesAsync(ct);

            return (await GetByIdWithHistoricoAsync(dto.Id, ct))!;
        }

        public async Task AlterarStatusAsync(Guid id, StatusAvaliacaoRisco novoStatus, int usuarioId, CancellationToken ct = default)
        {
            var ar = await _repo.GetByIdAsync(id, ct)
                ?? throw new InvalidOperationException("Avaliação não encontrada.");

            var acao = novoStatus == StatusAvaliacaoRisco.Inativa ? "Inativado" : "Ativado";
            ar.Status = novoStatus;
            ar.ModificadoPorId = usuarioId;
            ar.DataModificacao = DateTimeHelper.AgoraBrasilia();

            await _repo.AddHistoricoAsync(new AvaliacaoRiscoHistorico
            {
                Id = Guid.NewGuid(), AvaliacaoRiscoId = ar.Id, UsuarioId = usuarioId,
                Acao = acao, DataAcao = DateTimeHelper.AgoraBrasilia(),
            }, ct);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task SoftDeleteAsync(Guid id, int usuarioId, CancellationToken ct = default)
        {
            var ar = await _repo.GetByIdAsync(id, ct)
                ?? throw new InvalidOperationException("Avaliação não encontrada.");

            ar.IsDeleted = true;
            await _repo.AddHistoricoAsync(new AvaliacaoRiscoHistorico
            {
                Id = Guid.NewGuid(), AvaliacaoRiscoId = ar.Id, UsuarioId = usuarioId,
                Acao = "Excluído", DataAcao = DateTimeHelper.AgoraBrasilia(),
            }, ct);
            await _repo.SaveChangesAsync(ct);
        }

        private static AvaliacaoRiscoDto Map(AvaliacaoRisco ar) => new()
        {
            Id = ar.Id, Numero = ar.Numero,
            Departamento = ar.Departamento, Operacao = ar.Operacao, Tarefa = ar.Tarefa, Data = ar.Data,
            Status = ar.Status, Observacoes = ar.Observacoes,
            CriadoPorNome = ar.CriadoPor?.NomeCompleto ?? "", CriadoPorId = ar.CriadoPorId,
            ModificadoPorNome = ar.ModificadoPor?.NomeCompleto,
            DataCriacao = ar.DataCriacao, DataModificacao = ar.DataModificacao,
            Equipamentos = ar.Equipamentos?.Select(e => new ArEquipamentoDto
            {
                EquipamentoId = e.EquipamentoId,
                Tag = e.Equipamento?.Tag ?? "",
                Nome = e.Equipamento?.EquipmentName ?? "",
                PlantaId = e.Equipamento?.PlantaId ?? 0,
                PlantaNome = e.Equipamento?.Planta?.Nome ?? "",
            }).ToList() ?? new(),
            Itens = ar.Itens.Select(i => new AvaliacaoRiscoItemDto
            {
                Id = i.Id, Ordem = i.Ordem, Tarefa = i.Tarefa, Perigo = i.Perigo, NumeroExpostos = i.NumeroExpostos,
                GravidadeAntes = i.GravidadeAntes, ProbabilidadeAntes = i.ProbabilidadeAntes, NivelRiscoAntes = i.NivelRiscoAntes,
                MedidasProtecao = i.MedidasProtecao,
                GravidadeDepois = i.GravidadeDepois, ProbabilidadeDepois = i.ProbabilidadeDepois, NivelRiscoDepois = i.NivelRiscoDepois,
            }).ToList(),
            Historico = ar.Historico?.Select(h => new AvaliacaoRiscoHistoricoDto
            {
                Id = h.Id, Acao = h.Acao, Detalhes = h.Detalhes, UsuarioNome = h.Usuario?.NomeCompleto ?? "", DataAcao = h.DataAcao,
            }).ToList() ?? new()
        };
    }
}
