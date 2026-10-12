using Mercadex.Domain.Cobranza;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mercadex.Infrastructure.Persistence.Configurations;

public sealed class CambioMontoObligacionConfiguration :
    IEntityTypeConfiguration<CambioMontoObligacion>
{
    public void Configure(
        EntityTypeBuilder<CambioMontoObligacion> builder)
    {
        builder.ToTable("cambios_monto_obligacion");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.MontoAnterior)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.MontoNuevo)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Motivo)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Responsable)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.FechaUtc)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasOne<Obligacion>()
            .WithMany()
            .HasForeignKey(x => x.ObligacionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.ObligacionId,
            x.FechaUtc
        });
    }
}