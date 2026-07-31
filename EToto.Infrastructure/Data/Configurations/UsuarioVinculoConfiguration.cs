using EToto.Domain.Entities;
using EToto.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EToto.Infrastructure.Data.Configurations
{
    // Configuração focada nos campos de vínculo Funcionário/Terceiro (#2).
    public class UsuarioVinculoConfiguration : IEntityTypeConfiguration<Usuarios>
    {
        public void Configure(EntityTypeBuilder<Usuarios> builder)
        {
            builder.Property(u => u.TipoVinculo)
                .HasConversion<int>()
                .HasDefaultValue(TipoVinculo.Funcionario)
                .IsRequired();

            builder.Property(u => u.NomeEmpresa)
                .HasMaxLength(200);
        }
    }
}
