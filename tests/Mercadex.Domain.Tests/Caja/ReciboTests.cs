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
    public void AnularRecibo_DebeRegistrarAuditoria()
    {
        var fecha = DateTimeOffset.UtcNow;

        var recibo = new Recibo(
            1,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            fecha,
            CrearDetalles());

        var fechaAnulacion =
            fecha.AddMinutes(5);

        recibo.Anular(
            "Registro incorrecto",
            fechaAnulacion,
            "administrador");

        Assert.True(
            recibo.EstaAnulado);

        Assert.Equal(
            EstadoRecibo.Anulado,
            recibo.Estado);

        Assert.Equal(
            "Registro incorrecto",
            recibo.MotivoAnulacion);

        Assert.Equal(
            fechaAnulacion,
            recibo.FechaAnulacion);

        Assert.Equal(
            "administrador",
            recibo.AnuladoPor);

        Assert.Equal(
            2,
            recibo.Detalles.Count);

        Assert.Equal(
            160m,
            recibo.Total);
    }

    [Fact]
    public void AnularRecibo_SinUsuario_DebeFallar()
    {
        var fecha = DateTimeOffset.UtcNow;

        var recibo = new Recibo(
            1,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            fecha,
            CrearDetalles());

        Assert.Throws<ArgumentException>(() =>
            recibo.Anular(
                "Registro incorrecto",
                fecha.AddMinutes(1),
                ""));
    }

    [Fact]
    public void AnularReciboDosVeces_DebeFallar()
    {
        var fecha = DateTimeOffset.UtcNow;

        var recibo = new Recibo(
            1,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            fecha,
            CrearDetalles());

        recibo.Anular(
            "Registro incorrecto",
            fecha.AddMinutes(1),
            "administrador");

        Assert.Throws<InvalidOperationException>(() =>
            recibo.Anular(
                "Segundo intento",
                fecha.AddMinutes(2),
                "administrador"));
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