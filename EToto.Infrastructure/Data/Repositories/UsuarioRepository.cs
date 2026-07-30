using EToto.Domain.Entities;
using EToto.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EToto.Infrastructure.Data.Repositories
{
    public class UsuarioRepository : BaseRepository<Usuarios>, IUsuarioRepository
    {
        public UsuarioRepository(LototoContext contexto) : base(contexto) { }

        public Task<Usuarios?> ObterPorLoginAsync(string login)
            => Context.Usuarios
                .Include(u => u.Planta)
                .Include(u => u.PlantasAssociadas)
                    .ThenInclude(up => up.Planta) // ✅ Carrega plantas associadas
                .Include(u => u.Perfis) // múltiplos perfis (#1b)
                .FirstOrDefaultAsync(u => u.Login == login);

        public async Task<IReadOnlyList<Usuarios>> ListarPorPlantaAsync(int plantaId)
            => await Context.Usuarios
                .Where(u => u.PlantaId == plantaId)
                .OrderBy(u => u.NomeCompleto)
                .ToListAsync();
        public async Task<IReadOnlyList<Plantas>> ObterPlantasDoUsuarioAsync(int usuarioId)
            => await Context.UsuarioPlantas
                .Where(up => up.UsuarioId == usuarioId)
                .Include(up => up.Planta)
                .Select(up => up.Planta)
                .Where(p => p.Ativa)
                .OrderBy(p => p.Nome)
                .ToListAsync();

        public async Task<Usuarios?> ObterComPlantasAsync(int id)
        {
            return await Context.Usuarios
                .Include(u => u.PlantasAssociadas)
                .Include(u => u.Perfis) // múltiplos perfis (#1c)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<IReadOnlyList<Usuarios>> ListarComPlantasAsync()
        {
            return await Context.Usuarios
                .Include(u => u.PlantasAssociadas)
                    .ThenInclude(pa => pa.Planta)
                .Include(u => u.Planta)
                .Include(u => u.Perfis) // múltiplos perfis (#1b)
                .Include(u => u.CriadoPor) // auditoria de cadastro (#1c)
                .Include(u => u.AlteradoPor)
                .OrderBy(u => u.NomeCompleto)
                .ToListAsync();
        }
    }
}
