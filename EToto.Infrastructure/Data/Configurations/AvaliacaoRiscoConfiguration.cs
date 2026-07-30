using EToto.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EToto.Infrastructure.Data.Configurations
{
    public class AvaliacaoRiscoConfiguration : IEntityTypeConfiguration<AvaliacaoRisco>
    {
        public void Configure(EntityTypeBuilder<AvaliacaoRisco> builder)
        {
            builder.ToTable("AvaliacaoRisco");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Numero).HasMaxLength(20).IsRequired();
            builder.HasIndex(e => e.Numero).IsUnique();

            builder.Property(e => e.Departamento).HasMaxLength(200).IsRequired();
            builder.Property(e => e.Operacao).HasMaxLength(200).IsRequired();
            builder.Property(e => e.Tarefa).HasMaxLength(500).IsRequired();
            builder.Property(e => e.Data).IsRequired();

            builder.Property(e => e.Status).IsRequired();
            builder.Property(e => e.Observacoes).HasMaxLength(2000).IsRequired(false);

            builder.Property(e => e.IsDeleted).HasDefaultValue(false);
            builder.Property(e => e.DataCriacao).HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(e => e.CriadoPor).WithMany().HasForeignKey(e => e.CriadoPorId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.ModificadoPor).WithMany().HasForeignKey(e => e.ModificadoPorId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.Equipamentos).WithOne(eq => eq.AvaliacaoRisco).HasForeignKey(eq => eq.AvaliacaoRiscoId).OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(e => e.Itens).WithOne(i => i.AvaliacaoRisco).HasForeignKey(i => i.AvaliacaoRiscoId).OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(e => e.Historico).WithOne(h => h.AvaliacaoRisco).HasForeignKey(h => h.AvaliacaoRiscoId).OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class AvaliacaoRiscoEquipamentoConfiguration : IEntityTypeConfiguration<AvaliacaoRiscoEquipamento>
    {
        public void Configure(EntityTypeBuilder<AvaliacaoRiscoEquipamento> builder)
        {
            builder.ToTable("AvaliacaoRiscoEquipamento");
            builder.HasKey(e => new { e.AvaliacaoRiscoId, e.EquipamentoId });
            builder.HasOne(e => e.Equipamento).WithMany().HasForeignKey(e => e.EquipamentoId).OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class AvaliacaoRiscoItemConfiguration : IEntityTypeConfiguration<AvaliacaoRiscoItem>
    {
        public void Configure(EntityTypeBuilder<AvaliacaoRiscoItem> builder)
        {
            builder.ToTable("AvaliacaoRiscoItem");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Tarefa).HasMaxLength(500).IsRequired();
            builder.Property(e => e.Perigo).HasMaxLength(500).IsRequired();
            builder.Property(e => e.NumeroExpostos).IsRequired();
            builder.Property(e => e.GravidadeAntes).HasMaxLength(50).IsRequired();
            builder.Property(e => e.ProbabilidadeAntes).HasMaxLength(50).IsRequired();
            builder.Property(e => e.NivelRiscoAntes).HasMaxLength(50).IsRequired();
            builder.Property(e => e.MedidasProtecao).HasMaxLength(2000).IsRequired();
            builder.Property(e => e.GravidadeDepois).HasMaxLength(50).IsRequired();
            builder.Property(e => e.ProbabilidadeDepois).HasMaxLength(50).IsRequired();
            builder.Property(e => e.NivelRiscoDepois).HasMaxLength(50).IsRequired();
        }
    }

    public class AvaliacaoRiscoHistoricoConfiguration : IEntityTypeConfiguration<AvaliacaoRiscoHistorico>
    {
        public void Configure(EntityTypeBuilder<AvaliacaoRiscoHistorico> builder)
        {
            builder.ToTable("AvaliacaoRiscoHistorico");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Acao).HasMaxLength(50).IsRequired();
            builder.Property(e => e.Detalhes).HasMaxLength(1000).IsRequired(false);
            builder.Property(e => e.DataAcao).HasDefaultValueSql("GETUTCDATE()");
            builder.HasOne(e => e.Usuario).WithMany().HasForeignKey(e => e.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
