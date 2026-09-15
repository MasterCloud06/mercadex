using Mercadex.Domain.Cobranza;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mercadex.Infrastructure.Persistence.Configurations;

public sealed class MonedaConfiguration :
    IEntityTypeConfiguration<Moneda>
{
    public void Configure(
        EntityTypeBuilder<Moneda> builder)
    {
        builder.ToTable("monedas");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Codigo)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.Nombre)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Simbolo)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.Activa)
            .IsRequired();

        builder.HasIndex(x => x.Codigo)
            .IsUnique();
    }
}
