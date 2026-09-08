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
}
