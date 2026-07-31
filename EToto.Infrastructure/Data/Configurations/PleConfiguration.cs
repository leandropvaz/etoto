using EToto.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EToto.Infrastructure.Data.Configurations
{
    public class PleConfiguration : IEntityTypeConfiguration<Ple>
    {
        public void Configure(EntityTypeBuilder<Ple> builder)
        {
            builder.ToTable("Ple");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Numero).HasMaxLength(20).IsRequired();
            builder.HasIndex(e => e.Numero).IsUnique();

            builder.Property(e => e.DataInicio).IsRequired();
            builder.Property(e => e.DataFim).IsRequired(false);

            builder.Property(e => e.Status).IsRequired();
            builder.Property(e => e.MotivoCancelamento).HasColumnName("MotivoCancelamento").HasMaxLength(500).IsRequired(false);

            builder.Property(e => e.IsDeleted).HasDefaultValue(false);
            builder.Property(e => e.DataCriacao).HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(e => e.Planta).WithMany().HasForeignKey(e => e.PlantaId).OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.Equipamentos).WithOne(pe => pe.Ple).HasForeignKey(pe => pe.PleId).OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.CriadoPor).WithMany().HasForeignKey(e => e.CriadoPorId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.ModificadoPor).WithMany().HasForeignKey(e => e.ModificadoPorId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.FinalizadoPor).WithMany().HasForeignKey(e => e.FinalizadoPorId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.UsuariosPermitidos).WithOne(up => up.Ple).HasForeignKey(up => up.PleId).OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(e => e.Historico).WithOne(h => h.Ple).HasForeignKey(h => h.PleId).OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class PleEquipamentoConfiguration : IEntityTypeConfiguration<PleEquipamento>
    {
        public void Configure(EntityTypeBuilder<PleEquipamento> builder)
        {
            builder.ToTable("PleEquipamento");
            builder.HasKey(e => new { e.PleId, e.EquipamentoId });
            builder.HasOne(e => e.Equipamento).WithMany().HasForeignKey(e => e.EquipamentoId).OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class PleUsuarioPermitidoConfiguration : IEntityTypeConfiguration<PleUsuarioPermitido>
    {
        public void Configure(EntityTypeBuilder<PleUsuarioPermitido> builder)
        {
            builder.ToTable("PleUsuarioPermitido");
            builder.HasKey(e => new { e.PleId, e.UsuarioId });
            builder.HasOne(e => e.Usuario).WithMany().HasForeignKey(e => e.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class PleHistoricoConfiguration : IEntityTypeConfiguration<PleHistorico>
    {
        public void Configure(EntityTypeBuilder<PleHistorico> builder)
        {
            builder.ToTable("PleHistorico");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Acao).HasMaxLength(50).IsRequired();
            builder.Property(e => e.StatusAnterior).HasMaxLength(50).IsRequired(false);
            builder.Property(e => e.StatusNovo).HasMaxLength(50).IsRequired(false);
            builder.Property(e => e.Detalhes).HasMaxLength(1000).IsRequired(false);
            builder.Property(e => e.DataAcao).HasDefaultValueSql("GETUTCDATE()");
            builder.HasOne(e => e.Usuario).WithMany().HasForeignKey(e => e.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
