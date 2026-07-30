using EToto.Domain.Entities;
using EToto.Domain.Interfaces;
using EToto.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace EToto.Infrastructure.Data
{
    public class LototoContext : DbContext
    {
        private readonly IExecutorContext? _executor;

        public LototoContext(DbContextOptions<LototoContext> options)
            : base(options)
        {
        }

        // Sobrecarga usada pelo DI quando IExecutorContext está registrado (#5a).
        public LototoContext(DbContextOptions<LototoContext> options, IExecutorContext executor)
            : base(options)
        {
            _executor = executor;
        }

        public DbSet<Plantas> Plantas => Set<Plantas>();
        public DbSet<Usuarios> Usuarios => Set<Usuarios>();
        public DbSet<Equipamento> Equipamentos => Set<Equipamento>();
        public DbSet<UsuarioPlanta> UsuarioPlantas => Set<UsuarioPlanta>();
        public DbSet<UsuarioPerfil> UsuarioPerfis => Set<UsuarioPerfil>();
        public DbSet<Ple> Ples => Set<Ple>();
        public DbSet<PleEquipamento> PleEquipamentos => Set<PleEquipamento>();
        public DbSet<PleHistorico> PleHistoricos => Set<PleHistorico>();
        public DbSet<AvaliacaoRisco> AvaliacoesRisco => Set<AvaliacaoRisco>();
        public DbSet<AvaliacaoRiscoItem> AvaliacaoRiscoItens => Set<AvaliacaoRiscoItem>();
        public DbSet<AvaliacaoRiscoHistorico> AvaliacaoRiscoHistoricos => Set<AvaliacaoRiscoHistorico>();
        public DbSet<AvaliacaoRiscoEquipamento> AvaliacaoRiscoEquipamentos => Set<AvaliacaoRiscoEquipamento>();
        public DbSet<PleUsuarioPermitido> PleUsuariosPermitidos => Set<PleUsuarioPermitido>();
        public DbSet<AuditoriaEntrada> AuditoriaEntradas => Set<AuditoriaEntrada>();
        public DbSet<CampanhaRevalidacao> CampanhasRevalidacao => Set<CampanhaRevalidacao>();
        public DbSet<ItemCampanhaRevalidacao> ItensCampanhaRevalidacao => Set<ItemCampanhaRevalidacao>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new PlantaConfiguration());
            modelBuilder.ApplyConfiguration(new EquipamentoConfiguration());
            modelBuilder.ApplyConfiguration(new UsuarioPlantaConfiguration());
            modelBuilder.ApplyConfiguration(new UsuarioPerfilConfiguration());
            modelBuilder.ApplyConfiguration(new UsuarioAuditoriaConfiguration());
            modelBuilder.ApplyConfiguration(new UsuarioVinculoConfiguration());
            modelBuilder.ApplyConfiguration(new PleConfiguration());
            modelBuilder.ApplyConfiguration(new PleEquipamentoConfiguration());
            modelBuilder.ApplyConfiguration(new PleHistoricoConfiguration());
            modelBuilder.ApplyConfiguration(new AvaliacaoRiscoConfiguration());
            modelBuilder.ApplyConfiguration(new AvaliacaoRiscoItemConfiguration());
            modelBuilder.ApplyConfiguration(new AvaliacaoRiscoHistoricoConfiguration());
            modelBuilder.ApplyConfiguration(new AvaliacaoRiscoEquipamentoConfiguration());
            modelBuilder.ApplyConfiguration(new PleUsuarioPermitidoConfiguration());
            modelBuilder.ApplyConfiguration(new AuditoriaEntradaConfiguration());
            modelBuilder.ApplyConfiguration(new CampanhaRevalidacaoConfiguration());
            modelBuilder.ApplyConfiguration(new ItemCampanhaRevalidacaoConfiguration());
            base.OnModelCreating(modelBuilder);
        }

        // #5a: intercepta SaveChanges para gravar trilha de auditoria automaticamente.
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            var pending = AuditoriaCapture.Snapshot(this, _executor?.UsuarioIdAtual);
            var affected = base.SaveChanges(acceptAllChangesOnSuccess);
            if (pending.Count > 0)
            {
                AuditoriaEntradas.AddRange(AuditoriaCapture.Materialize(pending));
                affected += base.SaveChanges(acceptAllChangesOnSuccess);
            }
            return affected;
        }

        public override async Task<int> SaveChangesAsync(
            bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default)
        {
            var pending = AuditoriaCapture.Snapshot(this, _executor?.UsuarioIdAtual);
            var affected = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            if (pending.Count > 0)
            {
                AuditoriaEntradas.AddRange(AuditoriaCapture.Materialize(pending));
                affected += await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            }
            return affected;
        }
    }
}
