using Mercadex.Domain.Socios;

namespace Mercadex.Domain.Tests.Socios;

public sealed class SocioTests
{
    [Fact]
    public void CrearSocio_ConDatosValidos_DebeCrearloActivo()
    {
        var socio = new Socio(
            "Juan",
            "Pérez",
            "12345678",
            "70000000");

        Assert.Equal("Juan", socio.Nombres);
        Assert.Equal("Pérez", socio.Apellidos);
        Assert.Equal("Juan Pérez", socio.NombreCompleto);
        Assert.True(socio.Activo);
    }

    [Fact]
    public void CrearSocio_SinNombres_DebeFallar()
    {
        Assert.Throws<ArgumentException>(() =>
            new Socio(
                "",
                "Pérez"));
    }

    [Fact]
    public void DesactivarSocio_DebeCambiarEstado()
    {
        var socio = new Socio(
            "Juan",
            "Pérez");

        socio.Desactivar();

        Assert.False(socio.Activo);
    }

    [Fact]
    public void ActualizarDatos_CorrigeNombreCiYCelular()
    {
        var socio = new Socio("Juan", "Peres", "123", "700");

        socio.ActualizarDatos("  Juan Carlos ", "Pérez", " 1234567 LP ", "  ");

        Assert.Equal("Juan Carlos", socio.Nombres);
        Assert.Equal("Pérez", socio.Apellidos);
        Assert.Equal("1234567 LP", socio.DocumentoIdentidad);
        Assert.Null(socio.Telefono);
    }

    [Fact]
    public void ActualizarDatos_SinApellidos_DebeFallarYNoCambiarNada()
    {
        var socio = new Socio("Juan", "Pérez", "123", "700");

        Assert.Throws<ArgumentException>(() => socio.ActualizarDatos("Pedro", " ", "999", null));

        Assert.Equal("Juan", socio.Nombres);
        Assert.Equal("123", socio.DocumentoIdentidad);
    }
}
