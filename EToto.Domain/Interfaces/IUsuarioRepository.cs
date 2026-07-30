using EToto.Domain.Entities;

namespace EToto.Domain.Interfaces
{
    public interface IUsuarioRepository : IRepository<Usuarios>
    {
        Task<Usuarios?> ObterPorLoginAsync(string login);
        Task<IReadOnlyList<Usuarios>> ListarPorPlantaAsync(int plantaId);
        Task<IReadOnlyList<Plantas>> ObterPlantasDoUsuarioAsync(int usuarioId);
        Task<Usuarios?> ObterComPlantasAsync(int id);

        Task<IReadOnlyList<Usuarios>> ListarComPlantasAsync();
    }
}
