using Mercadex.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mercadex.Infrastructure.Persistence.Configurations;

public sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.NombreUsuario).HasMaxLength(40).IsRequired();
        builder.Property(x => x.NombreCompleto).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Rol).HasConversion<int>().IsRequired();
        builder.Property(x => x.Cargo).HasMaxLength(80);
        builder.Ignore(x => x.CargoParaRecibo);
        builder.Property(x => x.ContrasenaHash).HasMaxLength(200).IsRequired();

        builder.HasIndex(x => x.NombreUsuario).IsUnique();
    }
}
