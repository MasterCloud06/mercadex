using Mercadex.Domain.Caja;
using Mercadex.Domain.Cobranza;

namespace Mercadex.Domain.Tests.Caja;

public sealed class ServicioCobroTests
{
    [Fact]
    public void RegistrarCobro_DebeCrearPagoReciboYPagarObligaciones()
    {
        var monedaId =
            Guid.CreateVersion7();

        var unidadId =
            Guid.CreateVersion7();

        var obligaciones =
            CrearObligaciones(
                unidadId,
                monedaId);

        var items = new[]
        {
            new ItemCobro(
                obligaciones[0],
                "Mensualidad"),

            new ItemCobro(
                obligaciones[1],
                "Alquiler de caseta")
        };

        var resultado =
            ServicioCobro.Registrar(
                1,
                items,
                FormaPago.Efectivo,
                "Juan Pérez",
                DateTimeOffset.UtcNow);

        Assert.Equal(
            160m,
            resultado.Pago.Monto);

        Assert.Equal(
            160m,
            resultado.Recibo.Total);

        Assert.All(
            obligaciones,
            x => Assert.True(x.EstaPagada));
    }

    [Fact]
    public void RegistrarCobro_DeUnidadesDistintas_DebeFallar()
    {
        var monedaId =
            Guid.CreateVersion7();

        var obligacion1 =
            CrearObligacion(
                Guid.CreateVersion7(),
                monedaId,
                100m);

        var obligacion2 =
            CrearObligacion(
                Guid.CreateVersion7(),
                monedaId,
                60m);

        var items = new[]
        {
            new ItemCobro(
                obligacion1,
                "Mensualidad"),

            new ItemCobro(
                obligacion2,
                "Alquiler")
        };

        Assert.Throws<InvalidOperationException>(() =>
            ServicioCobro.Registrar(
                1,
                items,
                FormaPago.Efectivo,
                "Juan Pérez",
                DateTimeOffset.UtcNow));
    }

    [Fact]
    public void RegistrarCobro_ConObligacionPagada_DebeFallar()
    {
        var obligacion =
            CrearObligacion(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                100m);

        obligacion.MarcarComoPagada();

        var items = new[]
        {
            new ItemCobro(
                obligacion,
                "Mensualidad")
        };

        Assert.Throws<InvalidOperationException>(() =>
            ServicioCobro.Registrar(
                1,
                items,
                FormaPago.Efectivo,
                "Juan Pérez",
                DateTimeOffset.UtcNow));
    }

    [Fact]
    public void AnularCobro_DebeAnularPagoReciboYRestaurarObligaciones()
    {
        var monedaId =
            Guid.CreateVersion7();

        var unidadId =
            Guid.CreateVersion7();

        var obligaciones =
            CrearObligaciones(
                unidadId,
                monedaId);

        var items = new[]
        {
            new ItemCobro(
                obligaciones[0],
                "Mensualidad"),

            new ItemCobro(
                obligaciones[1],
                "Alquiler de caseta")
        };

        var fecha =
            DateTimeOffset.UtcNow;

        var resultado =
            ServicioCobro.Registrar(
                1,
                items,
                FormaPago.QR,
                "Juan Pérez",
                fecha,
                1m,
                "REF-123");

        ServicioCobro.Anular(
            resultado.Recibo,
            resultado.Pago,
            obligaciones,
            "Pago registrado por error",
            fecha.AddMinutes(10));

        Assert.True(
            resultado.Recibo.EstaAnulado);

        Assert.True(
            resultado.Pago.EstaAnulado);

        Assert.All(
            obligaciones,
            x => Assert.True(x.EstaPendiente));
    }

    [Fact]
    public void AnularCobro_ConObligacionesIncompletas_DebeFallar()
    {
        var monedaId =
            Guid.CreateVersion7();

        var unidadId =
            Guid.CreateVersion7();

        var obligaciones =
            CrearObligaciones(
                unidadId,
                monedaId);

        var items = new[]
        {
            new ItemCobro(
                obligaciones[0],
                "Mensualidad"),

            new ItemCobro(
                obligaciones[1],
                "Alquiler de caseta")
        };

        var fecha =
            DateTimeOffset.UtcNow;

        var resultado =
            ServicioCobro.Registrar(
                1,
                items,
                FormaPago.Efectivo,
                "Juan Pérez",
                fecha);

        Assert.Throws<InvalidOperationException>(() =>
            ServicioCobro.Anular(
                resultado.Recibo,
                resultado.Pago,
                new[] { obligaciones[0] },
                "Intento inválido",
                fecha.AddMinutes(5)));
    }

    private static List<Obligacion> CrearObligaciones(
        Guid unidadId,
        Guid monedaId)
    {
        return
        [
            CrearObligacion(
                unidadId,
                monedaId,
                100m),

            CrearObligacion(
                unidadId,
                monedaId,
                60m)
        ];
    }

    private static Obligacion CrearObligacion(
        Guid unidadId,
        Guid monedaId,
        decimal monto)
    {
        return new Obligacion(
            unidadId,
            Guid.CreateVersion7(),
            monedaId,
            monto,
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 9, 30),
            2026,
            9);
    }
}
