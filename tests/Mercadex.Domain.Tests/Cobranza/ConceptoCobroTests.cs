using Mercadex.Domain.Cobranza;

namespace Mercadex.Domain.Tests.Cobranza;

public sealed class ConceptoCobroTests
{
    [Fact]
    public void CrearConcepto_ConDatosValidos_DebeCrearloActivo()
    {
        var concepto = new ConceptoCobro(
            "mens",
            "Mensualidad");

        Assert.Equal("MENS", concepto.Codigo);
        Assert.Equal("Mensualidad", concepto.Nombre);
        Assert.True(concepto.Activo);
    }

    [Fact]
    public void CrearConcepto_SinNombre_DebeFallar()
    {
        Assert.Throws<ArgumentException>(() =>
            new ConceptoCobro(
                "MENS",
                ""));
    }

    [Fact]
    public void Desactivar_y_corregir_conserva_el_codigo()
    {
        var concepto = new ConceptoCobro("luz", "Excedente de lus");

        concepto.CambiarNombre(" Excedente de luz ");
        concepto.CambiarDescripcion("  ");
        concepto.Desactivar();

        Assert.Equal("LUZ", concepto.Codigo);
        Assert.Equal("Excedente de luz", concepto.Nombre);
        Assert.Null(concepto.Descripcion);
        Assert.False(concepto.Activo);

        concepto.Activar();
        Assert.True(concepto.Activo);
    }
}
