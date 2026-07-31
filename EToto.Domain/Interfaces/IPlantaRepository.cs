using EToto.Domain.Entities;

namespace EToto.Domain.Interfaces
{
    public interface IPlantaRepository : IRepository<Plantas>
    {
        Task<IReadOnlyList<Plantas>> ListarAtivasAsync();
        Task<Plantas?> ObterPorCodigoAsync(string codigo);
    }
}
