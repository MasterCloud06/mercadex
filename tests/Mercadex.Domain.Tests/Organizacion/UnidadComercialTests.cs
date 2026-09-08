using Mercadex.Domain.Organizacion;

namespace Mercadex.Domain.Tests.Organizacion;

public sealed class UnidadComercialTests
{
    [Fact]
    public void CrearUnidad_ConCodigoValido_DebeCrearlaActiva()
    {
        var unidad = new UnidadComercial(
            "c-025",
            TipoUnidadComercial.Caseta);

        Assert.Equal("C-025", unidad.Codigo);

        Assert.Equal(
            TipoUnidadComercial.Caseta,
            unidad.Tipo);

        Assert.True(unidad.Activa);
    }

    [Fact]
    public void CrearUnidad_SinCodigo_DebeFallar()
    {
        Assert.Throws<ArgumentException>(() =>
            new UnidadComercial(
                "",
                TipoUnidadComercial.Caseta));
    }

    [Fact]
    public void DesactivarUnidad_DebeCambiarEstado()
    {
        var unidad = new UnidadComercial(
            "C-025",
            TipoUnidadComercial.Caseta);

        unidad.Desactivar();

        Assert.False(unidad.Activa);
    }
}
