using EToto.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EToto.Infrastructure.Data.Configurations
{
    public class UsuarioPerfilConfiguration : IEntityTypeConfiguration<UsuarioPerfil>
    {
        public void Configure(EntityTypeBuilder<UsuarioPerfil> builder)
        {
            builder.ToTable("UsuarioPerfis");

            builder.HasKey(up => new { up.UsuarioId, up.Perfil });

            builder.Property(up => up.Perfil)
                .HasConversion<int>()
                .IsRequired();

            builder.HasOne(up => up.Usuario)
                .WithMany(u => u.Perfis)
                .HasForeignKey(up => up.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(up => up.DataAssociacao)
                .IsRequired();
        }
    }
}
