using Mercadex.Domain.AppSocios;
using Xunit;

namespace Mercadex.Domain.Tests.AppSocios;

public sealed class CuentaSocioAppTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 10, 12, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Nuevo_codigo_anula_contrasena_y_sesiones()
    {
        var cuenta = new CuentaSocioApp(Guid.NewGuid(), Ahora);
        cuenta.AsignarCodigo("h1", Ahora.AddDays(7));
        cuenta.Activar("clave", Ahora);
        var version = cuenta.VersionSesion;

        cuenta.AsignarCodigo("h2", Ahora.AddDays(7));

        Assert.False(cuenta.TieneContrasena);
        Assert.True(cuenta.CodigoVigente(Ahora));
        Assert.NotEqual(version, cuenta.VersionSesion);
    }

    [Fact]
    public void Se_bloquea_tras_cinco_fallos_y_se_desbloquea_con_el_tiempo()
    {
        var cuenta = new CuentaSocioApp(Guid.NewGuid(), Ahora);

        for (var i = 0; i < CuentaSocioApp.IntentosPermitidos; i++)
            cuenta.RegistrarFallo(Ahora);

        Assert.True(cuenta.EstaBloqueada(Ahora));
        Assert.False(cuenta.EstaBloqueada(Ahora + CuentaSocioApp.TiempoBloqueo + TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public void El_codigo_vence()
    {
        var cuenta = new CuentaSocioApp(Guid.NewGuid(), Ahora);
        cuenta.AsignarCodigo("h", Ahora.AddDays(7));
        Assert.False(cuenta.CodigoVigente(Ahora.AddDays(8)));
    }
}

public sealed class PagoReportadoTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 10, 12, 12, 0, 0, TimeSpan.Zero);
    private static readonly byte[] Foto = { 0xFF, 0xD8, 0xFF, 0x00 };

    private static PagoReportado Nuevo() => new(
        Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 100m,
        new[] { Guid.NewGuid() }, "TRX-12345", Foto, "image/jpeg", Ahora);

    [Fact]
    public void Exige_numero_de_transaccion()
    {
        Assert.Throws<ArgumentException>(() => new PagoReportado(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 100m,
            new[] { Guid.NewGuid() }, " ", Foto, "image/jpeg", Ahora));
    }

    [Fact]
    public void No_se_revisa_dos_veces()
    {
        var pago = Nuevo();
        pago.Rechazar("Cajera Ana", "El monto no coincide", Ahora);

        Assert.Equal(EstadoPagoReportado.Rechazado, pago.Estado);
        Assert.Throws<InvalidOperationException>(() => pago.Aprobar("Cajera Ana", Guid.NewGuid(), 10, Ahora));
    }

    [Fact]
    public void Rechazar_exige_motivo()
    {
        Assert.Throws<ArgumentException>(() => Nuevo().Rechazar("Cajera Ana", "no", Ahora));
    }
}
