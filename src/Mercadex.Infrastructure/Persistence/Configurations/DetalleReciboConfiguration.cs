using Mercadex.Domain.Caja;
using Mercadex.Domain.Cobranza;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mercadex.Infrastructure.Persistence.Configurations;

public sealed class DetalleReciboConfiguration :
    IEntityTypeConfiguration<DetalleRecibo>
{
    public void Configure(
        EntityTypeBuilder<DetalleRecibo> builder)
    {
        builder.ToTable("detalles_recibo");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Concepto)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Monto)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.PeriodoAnio);

        builder.Property(x => x.PeriodoMes);


        builder.HasOne<Recibo>()
            .WithMany(x => x.Detalles)
            .HasForeignKey("ReciboId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);


        builder.HasOne<Obligacion>()
            .WithMany()
            .HasForeignKey(x => x.ObligacionId)
            .OnDelete(DeleteBehavior.Restrict);


        builder.HasOne<ConceptoCobro>()
            .WithMany()
            .HasForeignKey(x => x.ConceptoCobroId)
            .OnDelete(DeleteBehavior.Restrict);


        builder.HasIndex(x => x.ObligacionId);

        builder.HasIndex(x => x.ConceptoCobroId);
    }
}