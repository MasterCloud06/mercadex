using Mercadex.Domain.Socios;
using Mercadex.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mercadex.Infrastructure.Persistence.Configurations;

public sealed class ArchivoKardexConfiguration : IEntityTypeConfiguration<ArchivoKardex>
{
    public void Configure(EntityTypeBuilder<ArchivoKardex> builder)
    {
        builder.ToTable("archivos_kardex");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Contenido).IsRequired();

        // Un archivo por documento; si se borra el documento, también su archivo.
        builder.HasOne<DocumentoKardex>()
            .WithOne()
            .HasForeignKey<ArchivoKardex>(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
