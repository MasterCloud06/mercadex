using Mercadex.Domain.Caja;

namespace Mercadex.Domain.Tests.Caja;

public sealed class ReciboTests
{
    [Fact]
    public void CrearRecibo_DebeCalcularTotal()
    {
        var detalles = CrearDetalles();

        var recibo = new Recibo(
            1,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateTimeOffset.UtcNow,
            detalles);

        Assert.Equal(
            EstadoRecibo.Emitido,
            recibo.Estado);

        Assert.Equal(
            160m,
            recibo.Total);

        Assert.Equal(
            2,
            recibo.Detalles.Count);
    }

    [Fact]
    public void CrearRecibo_ConObligacionDuplicada_DebeFallar()
    {
        var obligacionId =
            Guid.CreateVersion7();

        var detalles = new[]
        {
            new DetalleRecibo(
                obligacionId,
                Guid.CreateVersion7(),
                "Mensualidad",
                100m),

            new DetalleRecibo(
                obligacionId,
                Guid.CreateVersion7(),
                "Alquiler",
                60m)
        };

        Assert.Throws<InvalidOperationException>(() =>
            new Recibo(
                1,
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                DateTimeOffset.UtcNow,
                detalles));
    }

    [Fact]
    public void AnularRecibo_DebeConservarDetalles()
    {
        var fecha =
            DateTimeOffset.UtcNow;

        var recibo = new Recibo(
            1,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            fecha,
            CrearDetalles());

        recibo.Anular(
            "Registro incorrecto",
            fecha.AddMinutes(5));

        Assert.True(
            recibo.EstaAnulado);

        Assert.Equal(
            EstadoRecibo.Anulado,
            recibo.Estado);

        Assert.Equal(
            2,
            recibo.Detalles.Count);

        Assert.Equal(
            160m,
            recibo.Total);

        Assert.Equal(
            "Registro incorrecto",
            recibo.MotivoAnulacion);
    }

    [Fact]
    public void AnularReciboDosVeces_DebeFallar()
    {
        var fecha =
            DateTimeOffset.UtcNow;

        var recibo = new Recibo(
            1,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            fecha,
            CrearDetalles());

        recibo.Anular(
            "Registro incorrecto",
            fecha.AddMinutes(1));

        Assert.Throws<InvalidOperationException>(() =>
            recibo.Anular(
                "Segundo intento",
                fecha.AddMinutes(2)));
    }

    private static List<DetalleRecibo> CrearDetalles()
    {
        return
        [
            new DetalleRecibo(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                "Mensualidad",
                100m,
                2026,
                9),

            new DetalleRecibo(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                "Alquiler de caseta",
                60m,
                2026,
                9)
        ];
    }
}
