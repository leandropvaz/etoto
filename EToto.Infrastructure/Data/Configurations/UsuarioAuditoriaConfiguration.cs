using EToto.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EToto.Infrastructure.Data.Configurations
{
    // Configuração focada apenas nos campos de auditoria de cadastro (#1a).
    // Não toca em colunas/índices existentes para evitar drift de schema.
    public class UsuarioAuditoriaConfiguration : IEntityTypeConfiguration<Usuarios>
    {
        public void Configure(EntityTypeBuilder<Usuarios> builder)
        {
            // Auto-referência: quem criou / quem alterou.
            // NoAction evita ciclo de cascata no SQL Server (a tabela referencia a si mesma).
            builder.HasOne(u => u.CriadoPor)
                .WithMany()
                .HasForeignKey(u => u.CriadoPorId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);

            builder.HasOne(u => u.AlteradoPor)
                .WithMany()
                .HasForeignKey(u => u.AlteradoPorId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);
        }
    }
}
