using Mercadex.Domain.Organizacion;
using Mercadex.Domain.Socios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mercadex.Infrastructure.Persistence.Configurations;

public sealed class AsignacionUnidadComercialConfiguration :
    IEntityTypeConfiguration<AsignacionUnidadComercial>
{
    public void Configure(
        EntityTypeBuilder<AsignacionUnidadComercial> builder)
    {
        builder.ToTable("asignaciones_unidades_comerciales");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.TipoVinculo)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.FechaInicio)
            .IsRequired();

        builder.Property(x => x.FechaFin);

        builder.Ignore(x => x.EstaVigente);

        builder.HasOne<Socio>()
            .WithMany()
            .HasForeignKey(x => x.SocioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<UnidadComercial>()
            .WithMany()
            .HasForeignKey(x => x.UnidadComercialId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x =>
            new
            {
                x.UnidadComercialId,
                x.SocioId,
                x.FechaInicio
            });
    }
}
