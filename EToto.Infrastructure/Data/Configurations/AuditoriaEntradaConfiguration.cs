using EToto.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EToto.Infrastructure.Data.Configurations
{
    public class AuditoriaEntradaConfiguration : IEntityTypeConfiguration<AuditoriaEntrada>
    {
        public void Configure(EntityTypeBuilder<AuditoriaEntrada> builder)
        {
            builder.ToTable("AuditoriaEntradas");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.NomeTabela)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(a => a.ChaveRegistro)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(a => a.Acao)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(a => a.ExecutadoEm)
                .IsRequired();

            // JSON pode ser grande — sem MaxLength. SQL Server → nvarchar(max).
            builder.Property(a => a.ValoresAntes);
            builder.Property(a => a.ValoresDepois);

            builder.HasOne(a => a.Usuario)
                .WithMany()
                .HasForeignKey(a => a.UsuarioId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);

            // Índices para consulta (#5b virá em seguida).
            builder.HasIndex(a => a.ExecutadoEm)
                .IsDescending();

            builder.HasIndex(a => a.UsuarioId);
            builder.HasIndex(a => a.NomeTabela);
        }
    }
}
