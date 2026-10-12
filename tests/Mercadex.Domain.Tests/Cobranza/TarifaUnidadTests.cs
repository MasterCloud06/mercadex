using Mercadex.Domain.Cobranza;

namespace Mercadex.Domain.Tests.Cobranza;

public sealed class TarifaUnidadTests
{
    [Fact]
    public void CrearTarifa_ConImporteValido()
    {
        var unidadId = Guid.CreateVersion7();
        var conceptoId = Guid.CreateVersion7();
        var monedaId = Guid.CreateVersion7();

        var tarifa = new TarifaUnidad(
            unidadId,
            conceptoId,
            monedaId,
            85.50m);

        Assert.Equal(unidadId, tarifa.UnidadComercialId);
        Assert.Equal(conceptoId, tarifa.ConceptoCobroId);
        Assert.Equal(monedaId, tarifa.MonedaId);
        Assert.Equal(85.50m, tarifa.Monto);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("12.345")]
    public void RechazarMontosInvalidos(string texto)
    {
        var monto = decimal.Parse(
            texto,
            System.Globalization.CultureInfo.InvariantCulture);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new TarifaUnidad(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                monto));
    }

    [Fact]
    public void ActualizarTarifa()
    {
        var tarifa = new TarifaUnidad(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            120m);

        var unidadOriginal = tarifa.UnidadComercialId;
        var conceptoOriginal = tarifa.ConceptoCobroId;
        var monedaNueva = Guid.CreateVersion7();

        tarifa.Actualizar(monedaNueva, 95m);

        Assert.Equal(95m, tarifa.Monto);
        Assert.Equal(monedaNueva, tarifa.MonedaId);
        Assert.Equal(unidadOriginal, tarifa.UnidadComercialId);
        Assert.Equal(conceptoOriginal, tarifa.ConceptoCobroId);
    }
}