using Mercadex.Domain.Auditoria;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mercadex.Infrastructure.Persistence.Configurations;

public sealed class RegistroCambioConfiguration : IEntityTypeConfiguration<RegistroCambio>
{
    public void Configure(EntityTypeBuilder<RegistroCambio> builder)
    {
        builder.ToTable("registro_cambios");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.FechaUtc).IsRequired();
        builder.Property(x => x.UsuarioId).HasMaxLength(64);
        builder.Property(x => x.Usuario).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Rol).HasMaxLength(30);
        builder.Property(x => x.Origen).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Modulo).HasMaxLength(60).IsRequired();
        builder.Property(x => x.Accion).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Detalle).HasMaxLength(1000);
        builder.Property(x => x.Referencia).HasMaxLength(64);
        builder.Property(x => x.Ip).HasMaxLength(45);

        builder.HasIndex(x => x.FechaUtc);
        builder.HasIndex(x => x.UsuarioId);
    }
}
