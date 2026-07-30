using System.Text.Json;
using EToto.Application.Dto;
using EToto.Application.Interfaces;
using EToto.Domain.Entities;
using EToto.Domain.Enums;
using EToto.Domain.Interfaces;

namespace EToto.Application.Services
{
    public class CampanhaRevalidacaoService
    {
        private readonly ICampanhaRepository _repo;
        private readonly IUsuarioRepository _usuarios;
        private readonly IUnitOfWork _uow;
        private readonly IEmailService _email;

        public CampanhaRevalidacaoService(
            ICampanhaRepository repo,
            IUsuarioRepository usuarios,
            IUnitOfWork uow,
            IEmailService email)
        {
            _repo = repo;
            _usuarios = usuarios;
            _uow = uow;
            _email = email;
        }

        public async Task<List<CampanhaRevalidacaoDto>> ListarAsync(CancellationToken ct = default)
        {
            var campanhas = await _repo.ListarAsync(ct);
            return campanhas.Select(MapResumo).ToList();
        }

        public async Task<(CampanhaRevalidacaoDto? Campanha, List<ItemCampanhaDto> Itens)> ObterDetalheAsync(
            int id, CancellationToken ct = default)
        {
            var c = await _repo.ObterComItensAsync(id, ct);
            if (c is null) return (null, new());

            var dto = MapResumo(c);
            var itens = c.Itens
                .OrderBy(i => i.Usuario?.NomeCompleto)
                .Select(MapItem)
                .ToList();
            return (dto, itens);
        }

        public async Task<int> CriarCampanhaAsync(CriarCampanhaDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Nome))
                throw new InvalidOperationException("Informe o nome da campanha.");
            if (dto.PlantaIds is null || dto.PlantaIds.Count == 0)
                throw new InvalidOperationException("Selecione ao menos uma planta.");

            var inicio = DateTime.UtcNow;
            if (!dto.DataFimPrevista.HasValue)
                throw new InvalidOperationException("Informe a data de conclusão prevista.");
            if (dto.DataFimPrevista.Value.Date <= inicio.Date)
                throw new InvalidOperationException("A conclusão prevista deve ser uma data futura.");

            var campanha = new CampanhaRevalidacao
            {
                Nome = dto.Nome.Trim(),
                // Periodicidade deixou de ser usada; mantida como placeholder pois a coluna é NOT NULL
                // (evita migration). O prazo agora vem de DataFimPrevista informada pelo usuário.
                Periodicidade = Periodicidade.Mensal,
                Status = StatusCampanha.EmAndamento,
                DataInicio = inicio,
                DataFimPrevista = dto.DataFimPrevista.Value,
                CriadoPorId = dto.ExecutadoPorId,
                Notas = dto.Notas
            };

            // Entram apenas usuários ativos associados a alguma das plantas selecionadas.
            var plantaIds = dto.PlantaIds.ToHashSet();
            var usuariosAtivos = (await _usuarios.ListarComPlantasAsync())
                .Where(u => u.Ativa)
                .Where(u => u.PlantasAssociadas.Any(pa => plantaIds.Contains(pa.PlantaId))
                         || (u.PlantaId.HasValue && plantaIds.Contains(u.PlantaId.Value)))
                .ToList();

            foreach (var u in usuariosAtivos)
            {
                campanha.Itens.Add(new ItemCampanhaRevalidacao
                {
                    UsuarioId = u.Id,
                    SnapshotUsuarioJson = SerializeSnapshot(u)
                });
            }

            await _repo.AdicionarAsync(campanha, ct);
            await _uow.CommitAsync(ct);

            // Notifica gestores (Admin/SuperGestor) — stub via LoggerEmailService até SMTP/SendGrid.
            await NotificarInicioAsync(campanha, ct);

            return campanha.Id;
        }

        public async Task DecidirItemAsync(DecidirItemDto dto, CancellationToken ct = default)
        {
            if (!Enum.IsDefined(typeof(DecisaoRevisao), dto.Decisao))
                throw new InvalidOperationException("Decisão inválida.");

            var item = await _repo.ObterItemAsync(dto.ItemId, ct);
            if (item is null) throw new InvalidOperationException("Item não encontrado.");
            if (item.CampanhaId != dto.CampanhaId)
                throw new InvalidOperationException("Item não pertence à campanha informada.");

            var decisao = (DecisaoRevisao)dto.Decisao;
            item.Decisao = decisao;
            item.DecididoPorId = dto.ExecutadoPorId;
            item.DecididoEm = DateTime.UtcNow;
            item.Observacao = dto.Observacao;
            _repo.AtualizarItem(item);

            // Revogar = inativar o usuário (preserva histórico e auditoria).
            if (decisao == DecisaoRevisao.Revogar)
            {
                var usuario = await _usuarios.GetByIdAsync(item.UsuarioId);
                if (usuario is not null && usuario.Ativa)
                {
                    usuario.Ativa = false;
                    usuario.DataAtualizacao = DateTime.UtcNow;
                    usuario.AlteradoPorId = dto.ExecutadoPorId;
                    usuario.AlteradoEm = DateTime.UtcNow;
                    _usuarios.Update(usuario);
                }
            }

            await _uow.CommitAsync(ct);
        }

        public async Task ConcluirAsync(int campanhaId, CancellationToken ct = default)
        {
            var c = await _repo.ObterComItensAsync(campanhaId, ct);
            if (c is null) throw new InvalidOperationException("Campanha não encontrada.");
            if (c.Status == StatusCampanha.Concluida) return;

            c.Status = StatusCampanha.Concluida;
            c.DataFimReal = DateTime.UtcNow;
            _repo.Atualizar(c);
            await _uow.CommitAsync(ct);
        }

        public async Task CancelarAsync(int campanhaId, CancellationToken ct = default)
        {
            var c = await _repo.ObterComItensAsync(campanhaId, ct);
            if (c is null) throw new InvalidOperationException("Campanha não encontrada.");
            if (c.Status == StatusCampanha.Cancelada) return;
            if (c.Status == StatusCampanha.Concluida)
                throw new InvalidOperationException("Campanha concluída não pode ser cancelada.");

            c.Status = StatusCampanha.Cancelada;
            c.DataFimReal = DateTime.UtcNow;
            _repo.Atualizar(c);
            await _uow.CommitAsync(ct);
        }

        private async Task NotificarInicioAsync(CampanhaRevalidacao campanha, CancellationToken ct)
        {
            // Lista os perfis com permissão de revisão (Administradores/SuperGestores).
            var todos = await _usuarios.ListarComPlantasAsync();
            var gestores = todos
                .Where(u => u.Ativa)
                .Where(u =>
                {
                    var perfis = u.Perfis is { Count: > 0 }
                        ? u.Perfis.Select(p => p.Perfil)
                        : new[] { u.Perfil };
                    return perfis.Any(p => p == PerfilUsuario.Administrador || p == PerfilUsuario.SuperGestor);
                })
                .Select(u => u.Login)
                .ToList();

            if (gestores.Count == 0) return;

            await _email.EnviarAsync(new EmailMensagem
            {
                Destinatarios = gestores,
                Assunto = $"[Lototo] Nova campanha de revalidação: {campanha.Nome}",
                Corpo =
                    $"Campanha '{campanha.Nome}' aberta em {campanha.DataInicio:dd/MM/yyyy HH:mm}. " +
                    $"Conclusão prevista até {campanha.DataFimPrevista:dd/MM/yyyy}. " +
                    $"Acesse a aba Revalidação em Usuários para revisar os acessos.",
                CorpoEhHtml = false
            }, ct);
        }

        private static CampanhaRevalidacaoDto MapResumo(CampanhaRevalidacao c) => new()
        {
            Id = c.Id,
            Nome = c.Nome,
            Periodicidade = (int)c.Periodicidade,
            PeriodicidadeNome = c.Periodicidade.ToString(),
            Status = (int)c.Status,
            StatusNome = c.Status.ToString(),
            DataInicio = c.DataInicio,
            DataFimPrevista = c.DataFimPrevista,
            DataFimReal = c.DataFimReal,
            CriadoPorId = c.CriadoPorId,
            CriadoPorNome = c.CriadoPor?.NomeCompleto,
            Notas = c.Notas,
            TotalItens = c.Itens.Count,
            ItensDecididos = c.Itens.Count(i => i.Decisao.HasValue)
        };

        private static ItemCampanhaDto MapItem(ItemCampanhaRevalidacao i) => new()
        {
            Id = i.Id,
            CampanhaId = i.CampanhaId,
            UsuarioId = i.UsuarioId,
            UsuarioLogin = i.Usuario?.Login ?? "",
            UsuarioNome = i.Usuario?.NomeCompleto ?? "",
            SnapshotResumo = ResumirSnapshot(i.SnapshotUsuarioJson),
            Decisao = i.Decisao.HasValue ? (int)i.Decisao.Value : null,
            DecisaoNome = i.Decisao?.ToString(),
            DecididoPorId = i.DecididoPorId,
            DecididoPorNome = i.DecididoPor?.NomeCompleto,
            DecididoEm = i.DecididoEm,
            Observacao = i.Observacao
        };

        private static string SerializeSnapshot(Usuarios u)
        {
            var snap = new
            {
                u.Login,
                u.NomeCompleto,
                Perfis = (u.Perfis ?? Enumerable.Empty<UsuarioPerfil>()).Select(p => p.Perfil.ToString()).ToList(),
                Plantas = (u.PlantasAssociadas ?? Enumerable.Empty<UsuarioPlanta>())
                    .Where(pa => pa.Planta != null).Select(pa => pa.Planta!.Nome).OrderBy(n => n).ToList(),
                TipoVinculo = u.TipoVinculo.ToString(),
                u.NomeEmpresa,
                u.DataValidadeAcesso,
                u.DataValidadeTreinamento,
                u.TreinamentoConcluido,
                u.DataUltimoLogin
            };
            return JsonSerializer.Serialize(snap);
        }

        private static string ResumirSnapshot(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return "";
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                var perfis = root.TryGetProperty("Perfis", out var pe)
                    ? string.Join(",", pe.EnumerateArray().Select(e => e.GetString()))
                    : "";
                var plantas = root.TryGetProperty("Plantas", out var pl)
                    ? string.Join(",", pl.EnumerateArray().Select(e => e.GetString()))
                    : "";
                var vinculo = root.TryGetProperty("TipoVinculo", out var v) ? v.GetString() : "";
                return $"Perfis: {perfis} · Plantas: {plantas} · Vínculo: {vinculo}";
            }
            catch
            {
                return "";
            }
        }
    }
}
