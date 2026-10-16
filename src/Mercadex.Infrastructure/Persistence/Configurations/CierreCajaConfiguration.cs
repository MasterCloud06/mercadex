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
        builder.Property(x => x.CantidadEfectivo).IsRequired();
        builder.Property(x => x.CantidadQr).IsRequired();
        builder.Property(x => x.CantidadOtros).IsRequired();

        builder.Property(x => x.Responsable).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Observacion).HasMaxLength(500);

        builder.Property(x => x.FechaRegistroUtc).IsRequired();

        builder.HasOne<Moneda>()
            .WithMany()
            .HasForeignKey(x => x.MonedaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.CajeroId)
            .HasMaxLength(64)
            .IsRequired()
            .HasDefaultValue(CierreCaja.CajaGeneral);

        builder.Property(x => x.CajeroNombre).HasMaxLength(120);

        // Un solo cierre por día, moneda y caja (cada cajera la suya).
        builder.HasIndex(x => new { x.Fecha, x.MonedaId, x.CajeroId }).IsUnique();
        builder.Ignore(x => x.EsGeneral);
        builder.Property(x => x.EgresosEfectivo).HasPrecision(18, 2);
        builder.Ignore(x => x.EfectivoEsperado);
    }
}
