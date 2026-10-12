using Mercadex.Domain.Caja;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mercadex.Infrastructure.Persistence.Configurations;

public sealed class ComprobanteAnulacionReciboConfiguration :
    IEntityTypeConfiguration<ComprobanteAnulacionRecibo>
{
    public void Configure(
        EntityTypeBuilder<ComprobanteAnulacionRecibo> builder)
    {
        builder.ToTable("comprobantes_anulacion_recibo");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.NumeroRecibo)
            .IsRequired();

        builder.Property(x => x.MontoTotal)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.FechaEmisionRecibo)
            .IsRequired();

        builder.Property(x => x.FechaAnulacionUtc)
            .IsRequired();

        builder.Property(x => x.Motivo)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Responsable)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.Referencia)
            .HasMaxLength(36)
            .IsRequired();

        builder.HasOne<Recibo>()
            .WithMany()
            .HasForeignKey(x => x.ReciboId)
            .OnDelete(DeleteBehavior.Restrict);

        // Un recibo solo puede tener un comprobante.
        builder.HasIndex(x => x.ReciboId)
            .IsUnique();

        builder.HasIndex(x => x.Referencia)
            .IsUnique();

        builder.HasIndex(x => x.FechaAnulacionUtc);
    }
}