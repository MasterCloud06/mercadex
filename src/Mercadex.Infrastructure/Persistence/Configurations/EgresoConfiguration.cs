using Mercadex.Domain.Cobranza;
using Mercadex.Domain.Finanzas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mercadex.Infrastructure.Persistence.Configurations;

public sealed class EgresoConfiguration
    : IEntityTypeConfiguration<Egreso>
{
    public void Configure(
        EntityTypeBuilder<Egreso> builder)
    {
        builder.ToTable("egresos");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Fecha)
            .IsRequired();

        builder.Property(x => x.Categoria)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Concepto)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Beneficiario)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Monto)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.MonedaId)
            .IsRequired();

        builder.Property(x => x.FormaPago)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.NumeroComprobante)
            .HasMaxLength(100);

        builder.Property(x => x.Observacion)
            .HasMaxLength(1000);

        builder.Property(x => x.FechaRegistro)
            .IsRequired();

        builder.Property(x => x.Anulado)
            .IsRequired();

        builder.Property(x => x.MotivoAnulacion)
            .HasMaxLength(500);

        builder.HasIndex(x => x.Fecha);

        builder.HasIndex(x => x.Categoria);

        builder.HasIndex(x => x.MonedaId);

        builder.HasOne<Moneda>()
            .WithMany()
            .HasForeignKey(x => x.MonedaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}