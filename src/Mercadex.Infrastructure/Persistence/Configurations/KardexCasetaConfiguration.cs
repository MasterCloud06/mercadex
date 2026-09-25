using Mercadex.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mercadex.Infrastructure.Persistence.Configurations;

public sealed class KardexCasetaConfiguration
    : IEntityTypeConfiguration<KardexCaseta>
{
    public void Configure(
        EntityTypeBuilder<KardexCaseta> builder)
    {
        builder.ToTable(
            "kardex_casetas",
            "public");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("Id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.UnidadComercialId)
            .HasColumnName("UnidadComercialId")
            .IsRequired();

        builder.HasIndex(x => x.UnidadComercialId)
            .IsUnique();

        builder.HasOne<
            Mercadex.Domain.Organizacion.UnidadComercial>()
            .WithMany()
            .HasForeignKey(x => x.UnidadComercialId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.SocioId)
            .HasColumnName("SocioId");

        builder.HasOne<
            Mercadex.Domain.Socios.Socio>()
            .WithMany()
            .HasForeignKey(x => x.SocioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.FechaIngreso)
            .HasColumnName("FechaIngreso")
            .HasColumnType("date");

        builder.Property(x => x.FechaRegistro)
            .HasColumnName("FechaRegistro")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()");

        builder.Property(x => x.EstadoVida)
            .HasColumnName("EstadoVida")
            .HasMaxLength(20);

        builder.Property(x => x.TipoDocumentoPropiedad)
            .HasColumnName("TipoDocumentoPropiedad")
            .HasMaxLength(40);

        builder.Property(x => x.PropietarioAnterior)
            .HasColumnName("PropietarioAnterior")
            .HasMaxLength(300);

        builder.Property(x => x.AporteSocioPagado)
            .HasColumnName("AporteSocioPagado");

        builder.Property(x => x.Observaciones)
            .HasColumnName("Observaciones")
            .HasColumnType("text");
    }
}
