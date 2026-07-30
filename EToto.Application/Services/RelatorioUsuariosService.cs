using EToto.Application.Dto;
using EToto.Domain.Entities;
using EToto.Domain.Enums;
using EToto.Domain.Interfaces;

namespace EToto.Application.Services
{
    // #6a: relatório consolidado de usuários ativos. Filtros e mapeamento centralizados
    // aqui; as exportações (Excel/PDF) consomem RelatorioUsuarioItemDto.
    public class RelatorioUsuariosService
    {
        private readonly IUsuarioRepository _repo;

        public RelatorioUsuariosService(IUsuarioRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<RelatorioUsuarioItemDto>> GerarAsync(RelatorioUsuariosFiltro filtro)
        {
            var usuarios = await _repo.ListarComPlantasAsync();

            var nomePlantaPorId = usuarios
                .SelectMany(u => u.PlantasAssociadas ?? Enumerable.Empty<UsuarioPlanta>())
                .Where(pa => pa.Planta != null)
                .GroupBy(pa => pa.PlantaId)
                .ToDictionary(g => g.Key, g => g.First().Planta!.Nome);

            IEnumerable<RelatorioUsuarioItemDto> itens = usuarios
                .Where(u => u.Ativa)
                .Select(u =>
                {
                    var perfis = u.Perfis is { Count: > 0 }
                        ? u.Perfis.Select(p => p.Perfil).ToList()
                        : new List<PerfilUsuario> { u.Perfil };

                    var statusAcesso = VinculoValidacao.AvaliarStatus(u.DataValidadeAcesso);
                    var statusTreinamento = VinculoValidacao.AvaliarStatus(u.DataValidadeTreinamento);

                    return new RelatorioUsuarioItemDto
                    {
                        Id = u.Id,
                        Login = u.Login,
                        NomeCompleto = u.NomeCompleto,
                        Ativa = u.Ativa,
                        PerfisNomes = perfis.Select(p => p.ToString()).ToList(),
                        PlantasNomes = u.PlantasAssociadas?
                            .Where(pa => pa.Planta != null)
                            .Select(pa => pa.Planta!.Nome)
                            .OrderBy(n => n)
                            .ToList() ?? new List<string>(),
                        TipoVinculo = (int)u.TipoVinculo,
                        TipoVinculoNome = (int)u.TipoVinculo == 2 ? "Parceiro" : "Funcionário",
                        NomeEmpresa = u.NomeEmpresa,
                        DataValidadeAcesso = u.DataValidadeAcesso,
                        StatusValidadeAcesso = (int)statusAcesso,
                        TreinamentoConcluido = u.TreinamentoConcluido,
                        DataValidadeTreinamento = u.DataValidadeTreinamento,
                        StatusValidadeTreinamento = (int)statusTreinamento,
                        ExigeTreinamento = TreinamentoValidacao.AlgumPerfilExigeTreinamento(perfis),
                        DataUltimoLogin = u.DataUltimoLogin,
                        CriadoPorNome = u.CriadoPor?.NomeCompleto,
                        CriadoEm = u.CriadoEm,
                        AlteradoPorNome = u.AlteradoPor?.NomeCompleto,
                        AlteradoEm = u.AlteradoEm
                    };
                });

            if (filtro.PlantaId.HasValue)
            {
                if (nomePlantaPorId.TryGetValue(filtro.PlantaId.Value, out var nome))
                    itens = itens.Where(i => i.PlantasNomes.Contains(nome));
                else
                    itens = Enumerable.Empty<RelatorioUsuarioItemDto>();
            }

            if (filtro.Perfil.HasValue)
            {
                var alvo = ((PerfilUsuario)filtro.Perfil.Value).ToString();
                itens = itens.Where(i => i.PerfisNomes.Contains(alvo));
            }

            if (filtro.TipoVinculo.HasValue)
                itens = itens.Where(i => i.TipoVinculo == filtro.TipoVinculo.Value);

            if (filtro.StatusValidade.HasValue)
            {
                var alvo = filtro.StatusValidade.Value;
                itens = itens.Where(i =>
                    i.StatusValidadeAcesso == alvo
                    || (i.ExigeTreinamento && i.StatusValidadeTreinamento == alvo));
            }

            return itens
                .OrderBy(i => i.NomeCompleto, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
