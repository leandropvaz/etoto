using EToto.Application.Dto;
using EToto.Application.Interfaces;
using EToto.Domain.Enums;
using EToto.Domain.Interfaces;

namespace EToto.Application.Services
{
    /// <summary>
    /// Monta a visão do painel de alertas (mural) de bloqueio/desbloqueio por planta.
    /// Reaproveita a mesma regra de bloqueio do restante do sistema.
    /// </summary>
    public class PainelAlertasService
    {
        private readonly IEquipamentoAppService _equipamentos;
        private readonly IPleRepository _ples;
        private readonly PlantasService _plantas;

        public PainelAlertasService(
            IEquipamentoAppService equipamentos,
            IPleRepository ples,
            PlantasService plantas)
        {
            _equipamentos = equipamentos;
            _ples = ples;
            _plantas = plantas;
        }

        /// <summary>
        /// Resolve a planta pelo código (link de TV) e monta o painel apenas visual
        /// (sem requisições nem líderes). Null se a planta não existir.
        /// </summary>
        public async Task<PainelAlertaDto?> GetPainelPorCodigoAsync(string codigo, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(codigo)) return null;

            var planta = (await _plantas.ListarAsync())
                .FirstOrDefault(p => string.Equals(p.Codigo, codigo.Trim(), StringComparison.OrdinalIgnoreCase));

            return planta is null
                ? null
                : await GetPainelAsync(planta.Id, incluirRequisicoes: false, incluirLideres: false, ct);
        }

        /// <param name="incluirRequisicoes">Carrega o(s) PLE(s) por equipamento (nº + id p/ navegação).</param>
        /// <param name="incluirLideres">Carrega os nomes dos líderes (dado sensível — só perfis autorizados).</param>
        public async Task<PainelAlertaDto> GetPainelAsync(int plantaId, bool incluirRequisicoes, bool incluirLideres, CancellationToken ct = default)
        {
            var planta = (await _plantas.ListarAsync()).FirstOrDefault(p => p.Id == plantaId);

            var equipamentos = (await _equipamentos.GetByPlantAsync(plantaId, ct))
                .Where(e => !e.IsDeleted)
                .OrderBy(e => e.Tag)
                .ToList();

            var statusMap = await _ples.GetEquipamentosComPleAtivoStatusAsync(plantaId, ct);

            // Mapa equipamento -> requisições (nº/id p/ navegação; líderes só se autorizado).
            var detalheMap = new Dictionary<Guid, List<PainelAlertaRequisicaoDto>>();
            if (incluirRequisicoes)
            {
                var plesAtivos = await _ples.GetPlesAtivosComDetalheAsync(plantaId, ct);
                foreach (var ple in plesAtivos)
                {
                    var lideres = new List<string>();
                    if (incluirLideres)
                    {
                        if (ple.CriadoPor is not null && !string.IsNullOrWhiteSpace(ple.CriadoPor.NomeCompleto))
                            lideres.Add(ple.CriadoPor.NomeCompleto);

                        foreach (var up in ple.UsuariosPermitidos)
                        {
                            var nome = up.Usuario?.NomeCompleto;
                            if (!string.IsNullOrWhiteSpace(nome) && !lideres.Contains(nome))
                                lideres.Add(nome!);
                        }
                    }

                    var req = new PainelAlertaRequisicaoDto
                    {
                        PleId = ple.Id,
                        Numero = ple.Numero,
                        Status = ple.Status,
                        Lideres = lideres
                    };

                    foreach (var pe in ple.Equipamentos)
                    {
                        if (!detalheMap.TryGetValue(pe.EquipamentoId, out var lista))
                        {
                            lista = new List<PainelAlertaRequisicaoDto>();
                            detalheMap[pe.EquipamentoId] = lista;
                        }
                        lista.Add(req);
                    }
                }
            }

            var dto = new PainelAlertaDto
            {
                PlantaId = plantaId,
                PlantaNome = planta?.Nome ?? string.Empty,
                PlantaCodigo = planta?.Codigo ?? string.Empty
            };

            foreach (var e in equipamentos)
            {
                // "Livre" não entra. Entram Em Andamento, Início do Desbloqueio e Criado
                // (este último = "Em processo de bloqueio", exibido assim que o bloqueio é criado).
                // statusMap só contém PLE ativo (Criado/EmAndamento/InicioDesbloqueio).
                if (!statusMap.TryGetValue(e.Id, out var s)) continue;

                var item = new PainelAlertaEquipamentoDto
                {
                    EquipamentoId = e.Id,
                    Tag = e.Tag,
                    Nome = e.EquipmentName,
                    Status = s
                };

                if (incluirRequisicoes && detalheMap.TryGetValue(e.Id, out var reqs))
                    item.Requisicoes = reqs;

                dto.Equipamentos.Add(item);
            }

            // A base tem uma linha de equipamento por fonte de energia; agrupa por TAG para
            // não repetir cards (mesmo critério de DISTINCT usado na tela /ple).
            dto.Equipamentos = dto.Equipamentos
                .GroupBy(x => x.Tag, StringComparer.OrdinalIgnoreCase)
                .Select(g => new PainelAlertaEquipamentoDto
                {
                    EquipamentoId = g.First().EquipamentoId,
                    Tag = g.Key,
                    Nome = g.Select(x => x.Nome).FirstOrDefault(n => !string.IsNullOrWhiteSpace(n)) ?? string.Empty,
                    // Status mais avançado entre as linhas do mesmo equipamento.
                    Status = g.Max(x => x.Status),
                    // Uma requisição pode cobrir várias linhas do mesmo equipamento — desduplica por PLE.
                    Requisicoes = g.SelectMany(x => x.Requisicoes)
                                   .GroupBy(r => r.PleId)
                                   .Select(r => r.First())
                                   .OrderBy(r => r.Numero)
                                   .ToList()
                })
                .OrderBy(x => x.Status switch
                {
                    StatusPle.EmAndamento => 0,
                    StatusPle.InicioDesbloqueio => 1,
                    _ => 2 // Criado (em processo)
                })
                .ThenBy(x => x.Tag, StringComparer.OrdinalIgnoreCase)
                .ToList();

            dto.TotalBloqueados = dto.Equipamentos.Count(x => x.Bloqueado);
            dto.TotalEmProcesso = dto.Equipamentos.Count(x => x.EmProcesso);
            dto.TotalLivres = 0;

            return dto;
        }
    }
}
