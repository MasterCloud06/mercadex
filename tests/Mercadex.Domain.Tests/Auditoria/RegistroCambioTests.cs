using Mercadex.Domain.Auditoria;
using Xunit;

namespace Mercadex.Domain.Tests.Auditoria;

public sealed class RegistroCambioTests
{
    [Fact]
    public void Recorta_textos_largos_y_quita_saltos_de_linea()
    {
        var linea = new RegistroCambio(DateTimeOffset.UtcNow, null, null, null, "Escritorio",
            "Caja", "Recibo anulado", "Motivo:\r\nse cobró dos veces" + new string('x', 2000), null, null);

        Assert.Equal("Sin sesión", linea.Usuario);
        Assert.DoesNotContain('\n', linea.Detalle!);
        Assert.Equal(1000, linea.Detalle!.Length);
    }

    [Fact]
    public void Guarda_quien_que_y_desde_donde()
    {
        var linea = new RegistroCambio(DateTimeOffset.UtcNow, "u1", "María López", "Cajero", "Panel web",
            "Pagos QR", "Pago QR aprobado", "Caseta A-12", "abc", "10.0.0.1");

        Assert.Equal("María López", linea.Usuario);
        Assert.Equal("Panel web", linea.Origen);
        Assert.Equal("Pago QR aprobado", linea.Accion);
        Assert.Equal("10.0.0.1", linea.Ip);
    }
}
