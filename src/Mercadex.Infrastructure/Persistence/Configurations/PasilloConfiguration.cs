using Mercadex.Domain.Organizacion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mercadex.Infrastructure.Persistence.Configurations;

public sealed class PasilloConfiguration :
    IEntityTypeConfiguration<Pasillo>
{
    public void Configure(
        EntityTypeBuilder<Pasillo> builder)
    {
        builder.ToTable("pasillos");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Codigo)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.Nombre)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Activo)
            .IsRequired();

        builder.HasIndex(x => x.Codigo)
            .IsUnique();
    }
}
