using Mercadex.Domain.Cobranza;
using Mercadex.Domain.Organizacion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mercadex.Infrastructure.Persistence.Configurations;

public sealed class TarifaUnidadConfiguration :
    IEntityTypeConfiguration<TarifaUnidad>
{
    public void Configure(EntityTypeBuilder<TarifaUnidad> builder)
    {
        builder.ToTable("tarifas_unidad");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Monto)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.FechaActualizacionUtc)
            .IsRequired();

        builder.HasOne<UnidadComercial>()
            .WithMany()
            .HasForeignKey(x => x.UnidadComercialId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ConceptoCobro>()
            .WithMany()
            .HasForeignKey(x => x.ConceptoCobroId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Moneda>()
            .WithMany()
            .HasForeignKey(x => x.MonedaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.UnidadComercialId,
            x.ConceptoCobroId
        }).IsUnique();
    }
}