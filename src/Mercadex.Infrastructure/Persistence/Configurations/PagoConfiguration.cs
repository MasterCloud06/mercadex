using Mercadex.Domain.Caja;
using Mercadex.Domain.Cobranza;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mercadex.Infrastructure.Persistence.Configurations;

public sealed class PagoConfiguration :
    IEntityTypeConfiguration<Pago>
{
    public void Configure(
        EntityTypeBuilder<Pago> builder)
    {
        builder.ToTable("pagos");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Monto)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.FormaPago)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.NombrePagador)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Referencia)
            .HasMaxLength(150);

        builder.Property(x => x.TipoCambio)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(x => x.FechaRegistro)
            .IsRequired();

        builder.Property(x => x.Estado)
            .HasConversion<int>()
            .IsRequired();

        builder.Ignore(x => x.EstaAnulado);

        builder.HasOne<Moneda>()
            .WithMany()
            .HasForeignKey(x => x.MonedaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.FechaRegistro);
        builder.HasIndex(x => x.Referencia);
    }
}
