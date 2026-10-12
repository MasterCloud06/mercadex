using Mercadex.Domain.Caja;
using Mercadex.Domain.Cobranza;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mercadex.Infrastructure.Persistence.Configurations;

public sealed class CierreCajaConfiguration :
    IEntityTypeConfiguration<CierreCaja>
{
    public void Configure(EntityTypeBuilder<CierreCaja> builder)
    {
        builder.ToTable("cierres_caja");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Fecha).IsRequired();

        builder.Property(x => x.Efectivo).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.Qr).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.Otros).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.Total).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.EfectivoContado).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.Diferencia).HasPrecision(18, 2).IsRequired();

        builder.Property(x => x.CantidadPagos).IsRequired();

        builder.Property(x => x.Responsable).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Observacion).HasMaxLength(500);

        builder.Property(x => x.FechaRegistroUtc).IsRequired();

        builder.HasOne<Moneda>()
            .WithMany()
            .HasForeignKey(x => x.MonedaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Un solo cierre por día y moneda.
        builder.HasIndex(x => new { x.Fecha, x.MonedaId }).IsUnique();
    }
}
