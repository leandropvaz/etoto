using EToto.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EToto.Infrastructure.Data.Configurations
{
    public class CampanhaRevalidacaoConfiguration : IEntityTypeConfiguration<CampanhaRevalidacao>
    {
        public void Configure(EntityTypeBuilder<CampanhaRevalidacao> builder)
        {
            builder.ToTable("CampanhasRevalidacao");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Nome).IsRequired().HasMaxLength(200);
            builder.Property(c => c.Periodicidade).HasConversion<int>().IsRequired();
            builder.Property(c => c.Status).HasConversion<int>().IsRequired();
            builder.Property(c => c.DataInicio).IsRequired();
            builder.Property(c => c.DataFimPrevista).IsRequired();
            builder.Property(c => c.Notas).HasMaxLength(2000);

            builder.HasOne(c => c.CriadoPor)
                .WithMany()
                .HasForeignKey(c => c.CriadoPorId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(c => c.Itens)
                .WithOne(i => i.Campanha)
                .HasForeignKey(i => i.CampanhaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(c => c.Status);
            builder.HasIndex(c => c.DataInicio).IsDescending();
        }
    }

    public class ItemCampanhaRevalidacaoConfiguration : IEntityTypeConfiguration<ItemCampanhaRevalidacao>
    {
        public void Configure(EntityTypeBuilder<ItemCampanhaRevalidacao> builder)
        {
            builder.ToTable("ItensCampanhaRevalidacao");
            builder.HasKey(i => i.Id);

            builder.Property(i => i.Decisao).HasConversion<int>();
            builder.Property(i => i.Observacao).HasMaxLength(1000);
            builder.Property(i => i.SnapshotUsuarioJson);

            builder.HasOne(i => i.Usuario)
                .WithMany()
                .HasForeignKey(i => i.UsuarioId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(i => i.DecididoPor)
                .WithMany()
                .HasForeignKey(i => i.DecididoPorId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);

            builder.HasIndex(i => i.CampanhaId);
            builder.HasIndex(i => new { i.CampanhaId, i.UsuarioId }).IsUnique();
        }
    }
}
