using Mercadex.Domain.Caja;

namespace Mercadex.Domain.Tests.Caja;

public sealed class ComprobanteAnulacionReciboTests
{
    private static ComprobanteAnulacionRecibo Crear(
        string motivo = "Correccion administrativa de prueba",
        string responsable = "admin_tarifas_dev",
        decimal monto = 150m)
    {
        var fecha = DateTimeOffset.UtcNow;

        return new ComprobanteAnulacionRecibo(
            Guid.CreateVersion7(),
            101,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            monto,
            fecha.AddDays(-1),
            fecha,
            motivo,
            responsable);
    }

    [Fact]
    public void ComprobanteValido_ConservaDatos()
    {
        var c = Crear();

        Assert.Equal(101, c.NumeroRecibo);
        Assert.Equal(150m, c.MontoTotal);
        Assert.Equal("admin_tarifas_dev", c.Responsable);
        Assert.Equal(
            "Correccion administrativa de prueba",
            c.Motivo);

        Assert.StartsWith("ANU-", c.Referencia);
        Assert.Equal(36, c.Referencia.Length);
        Assert.Equal(TimeSpan.Zero, c.FechaAnulacionUtc.Offset);
    }

    [Fact]
    public void CadaComprobante_ReferenciaDiferente()
    {
        var primero = Crear();
        var segundo = Crear();

        Assert.NotEqual(
            primero.Referencia,
            segundo.Referencia);
    }

    [Fact]
    public void MotivoVacio_DebeRechazar()
    {
        Assert.Throws<ArgumentException>(() =>
            Crear(motivo: " "));
    }

    [Fact]
    public void MotivoMuyCorto_DebeRechazar()
    {
        Assert.Throws<ArgumentException>(() =>
            Crear(motivo: "abc"));
    }

    [Fact]
    public void ResponsableVacio_DebeRechazar()
    {
        Assert.Throws<ArgumentException>(() =>
            Crear(responsable: " "));
    }

    [Fact]
    public void ImporteInvalido_DebeRechazar()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Crear(monto: 0m));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Crear(monto: 150.123m));
    }

    [Fact]
    public void FechaAnteriorAEmision_DebeRechazar()
    {
        var fecha = DateTimeOffset.UtcNow;

        Assert.Throws<ArgumentException>(() =>
            new ComprobanteAnulacionRecibo(
                Guid.CreateVersion7(),
                101,
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                150m,
                fecha,
                fecha.AddDays(-1),
                "Anulacion de prueba",
                "admin_tarifas_dev"));
    }

    [Fact]
    public void ReciboInvalido_DebeRechazar()
    {
        var fecha = DateTimeOffset.UtcNow;

        Assert.Throws<ArgumentException>(() =>
            new ComprobanteAnulacionRecibo(
                Guid.Empty,
                101,
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                150m,
                fecha.AddDays(-1),
                fecha,
                "Anulacion de prueba",
                "admin_tarifas_dev"));
    }
}