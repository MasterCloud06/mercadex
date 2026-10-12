using Mercadex.Domain.AppSocios;
using Mercadex.Domain.Cobranza;
using Mercadex.Domain.Organizacion;
using Mercadex.Domain.Socios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mercadex.Infrastructure.Persistence.Configurations;

public sealed class CuentaSocioAppConfiguration : IEntityTypeConfiguration<CuentaSocioApp>
{
    public void Configure(EntityTypeBuilder<CuentaSocioApp> builder)
    {
        builder.ToTable("cuentas_socio_app");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.ContrasenaHash).HasMaxLength(200);
        builder.Property(x => x.CodigoActivacionHash).HasMaxLength(200);
        builder.Ignore(x => x.TieneContrasena);

        builder.HasOne<Socio>()
            .WithMany()
            .HasForeignKey(x => x.SocioId)
            .OnDelete(DeleteBehavior.Restrict);

        // Una cuenta por socio.
        builder.HasIndex(x => x.SocioId).IsUnique();
    }
}

public sealed class PagoReportadoConfiguration : IEntityTypeConfiguration<PagoReportado>
{
    public void Configure(EntityTypeBuilder<PagoReportado> builder)
    {
        builder.ToTable("pagos_reportados");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Monto).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.ObligacionIds).IsRequired();
        builder.Property(x => x.Referencia).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Comprobante).IsRequired();
        builder.Property(x => x.ComprobanteTipo).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Estado).HasConversion<int>().IsRequired();
        builder.Property(x => x.MotivoRechazo).HasMaxLength(300);
        builder.Property(x => x.RevisadoPor).HasMaxLength(120);

        builder.HasOne<Socio>().WithMany().HasForeignKey(x => x.SocioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<UnidadComercial>().WithMany().HasForeignKey(x => x.UnidadComercialId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Moneda>().WithMany().HasForeignKey(x => x.MonedaId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.Estado, x.FechaReporteUtc });
        builder.HasIndex(x => x.SocioId);
    }
}

public sealed class ConfiguracionPagoAppConfiguration : IEntityTypeConfiguration<ConfiguracionPagoApp>
{
    public void Configure(EntityTypeBuilder<ConfiguracionPagoApp> builder)
    {
        builder.ToTable("configuracion_pago_app");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.QrTipo).HasMaxLength(30);
        builder.Property(x => x.Banco).HasMaxLength(80);
        builder.Property(x => x.NumeroCuenta).HasMaxLength(60);
        builder.Property(x => x.TitularCuenta).HasMaxLength(120);
        builder.Property(x => x.Instrucciones).HasMaxLength(600);
    }
}
