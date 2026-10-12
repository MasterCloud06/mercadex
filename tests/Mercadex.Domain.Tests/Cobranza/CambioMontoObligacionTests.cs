using Mercadex.Domain.Cobranza;

namespace Mercadex.Domain.Tests.Cobranza;

public sealed class CambioMontoObligacionTests
{
    private static Obligacion CrearObligacion()
    {
        return new Obligacion(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            100m,
            new DateOnly(2026, 11, 1),
            new DateOnly(2026, 11, 30),
            2026,
            11);
    }

    [Fact]
    public void CorregirMonto_Pendiente_DebeActualizar()
    {
        var obligacion = CrearObligacion();

        obligacion.CorregirMonto(125.50m);

        Assert.Equal(125.50m, obligacion.Monto);
        Assert.True(obligacion.EstaPendiente);
    }

    [Fact]
    public void CorregirMonto_Pagada_DebeRechazar()
    {
        var obligacion = CrearObligacion();

        obligacion.MarcarComoPagada();

        Assert.Throws<InvalidOperationException>(
            () => obligacion.CorregirMonto(150m));

        Assert.Equal(100m, obligacion.Monto);
    }

    [Fact]
    public void CorregirMonto_Invalido_DebeRechazar()
    {
        var obligacion = CrearObligacion();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => obligacion.CorregirMonto(0m));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => obligacion.CorregirMonto(100.123m));

        Assert.Throws<InvalidOperationException>(
            () => obligacion.CorregirMonto(100m));
    }

    [Fact]
    public void Historial_DebeConservarDatosOriginales()
    {
        var id = Guid.CreateVersion7();
        var fecha = DateTime.UtcNow;

        var cambio = new CambioMontoObligacion(
            id,
            100m,
            150m,
            "Correccion autorizada de tarifa aplicada",
            "admin_tarifas_dev",
            fecha);

        Assert.Equal(id, cambio.ObligacionId);
        Assert.Equal(100m, cambio.MontoAnterior);
        Assert.Equal(150m, cambio.MontoNuevo);
        Assert.Equal(
            "Correccion autorizada de tarifa aplicada",
            cambio.Motivo);
        Assert.Equal("admin_tarifas_dev", cambio.Responsable);
        Assert.Equal(fecha, cambio.FechaUtc);
    }

    [Fact]
    public void Historial_SinMotivo_DebeRechazar()
    {
        Assert.Throws<ArgumentException>(() =>
            new CambioMontoObligacion(
                Guid.CreateVersion7(),
                100m,
                150m,
                " ",
                "admin_tarifas_dev",
                DateTime.UtcNow));
    }
}