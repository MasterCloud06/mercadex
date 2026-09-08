using Mercadex.Domain.Cobranza;

namespace Mercadex.Domain.Tests.Cobranza;

public sealed class ObligacionTests
{
    [Fact]
    public void CrearObligacion_DebeQuedarPendiente()
    {
        var obligacion = CrearObligacion();

        Assert.Equal(
            EstadoObligacion.Pendiente,
            obligacion.Estado);

        Assert.True(obligacion.EstaPendiente);

        Assert.Equal(
            100m,
            obligacion.Monto);
    }

    [Fact]
    public void CrearObligacion_ConMontoCero_DebeFallar()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Obligacion(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                0m,
                new DateOnly(2026, 9, 1)));
    }

    [Fact]
    public void CrearObligacion_ConVencimientoAnterior_DebeFallar()
    {
        Assert.Throws<ArgumentException>(() =>
            new Obligacion(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                100m,
                new DateOnly(2026, 9, 10),
                new DateOnly(2026, 9, 1)));
    }

    [Fact]
    public void MarcarComoPagada_DebeCambiarEstado()
    {
        var obligacion = CrearObligacion();

        obligacion.MarcarComoPagada();

        Assert.True(obligacion.EstaPagada);

        Assert.Equal(
            EstadoObligacion.Pagada,
            obligacion.Estado);
    }

    [Fact]
    public void RestaurarPagada_DebeVolverAPendiente()
    {
        var obligacion = CrearObligacion();

        obligacion.MarcarComoPagada();
        obligacion.RestaurarComoPendiente();

        Assert.True(obligacion.EstaPendiente);

        Assert.Equal(
            EstadoObligacion.Pendiente,
            obligacion.Estado);
    }

    private static Obligacion CrearObligacion()
    {
        return new Obligacion(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            100m,
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 9, 30),
            2026,
            9);
    }
}
