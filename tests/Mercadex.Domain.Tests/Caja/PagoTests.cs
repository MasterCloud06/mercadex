using Mercadex.Domain.Caja;

namespace Mercadex.Domain.Tests.Caja;

public sealed class PagoTests
{
    [Fact]
    public void CrearPago_DebeQuedarRegistrado()
    {
        var pago = new Pago(
            Guid.CreateVersion7(),
            160m,
            FormaPago.Efectivo,
            "Juan Pérez",
            DateTimeOffset.UtcNow);

        Assert.Equal(
            EstadoPago.Registrado,
            pago.Estado);

        Assert.Equal(
            160m,
            pago.Monto);

        Assert.False(
            pago.EstaAnulado);
    }

    [Fact]
    public void CrearPago_ConMontoCero_DebeFallar()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Pago(
                Guid.CreateVersion7(),
                0m,
                FormaPago.Efectivo,
                "Juan Pérez",
                DateTimeOffset.UtcNow));
    }

    [Fact]
    public void AnularPagoDosVeces_DebeFallar()
    {
        var pago = new Pago(
            Guid.CreateVersion7(),
            160m,
            FormaPago.Efectivo,
            "Juan Pérez",
            DateTimeOffset.UtcNow);

        pago.Anular();

        Assert.Throws<InvalidOperationException>(() =>
            pago.Anular());
    }
}
