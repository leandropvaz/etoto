using EToto.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EToto.Infrastructure.Data.Configurations
{
    public class UsuarioPlantaConfiguration : IEntityTypeConfiguration<UsuarioPlanta>
    {
        public void Configure(EntityTypeBuilder<UsuarioPlanta> builder)
        {
            builder.ToTable("UsuarioPlantas");

            builder.HasKey(up => new { up.UsuarioId, up.PlantaId });

            builder.HasOne(up => up.Usuario)
                .WithMany(u => u.PlantasAssociadas)
                .HasForeignKey(up => up.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(up => up.Planta)
                .WithMany(p => p.UsuariosAssociados)
                .HasForeignKey(up => up.PlantaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(up => up.DataAssociacao)
                .IsRequired();
        }
    }
}