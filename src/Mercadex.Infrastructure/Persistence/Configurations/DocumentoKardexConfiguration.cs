using Mercadex.Domain.Organizacion;
using Mercadex.Domain.Socios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mercadex.Infrastructure.Persistence.Configurations;

public sealed class DocumentoKardexConfiguration :
    IEntityTypeConfiguration<DocumentoKardex>
{
    public void Configure(EntityTypeBuilder<DocumentoKardex> builder)
    {
        builder.ToTable("documentos_kardex");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Tipo)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.NombreArchivo)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.TipoContenido)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.TamanoBytes)
            .IsRequired();

        builder.Property(x => x.Sha256)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.ClaveArchivo)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.RegistradoPor)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.FechaRegistroUtc)
            .IsRequired();

        builder.HasOne<Socio>()
            .WithMany()
            .HasForeignKey(x => x.SocioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<UnidadComercial>()
            .WithMany()
            .HasForeignKey(x => x.UnidadComercialId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.SocioId, x.Tipo });

        builder.HasIndex(x => x.ClaveArchivo)
            .IsUnique();
    }
}