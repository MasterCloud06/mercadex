using Mercadex.Domain.Caja;
using Mercadex.Domain.Finanzas;
using Xunit;

namespace Mercadex.Domain.Tests.Caja;

public sealed class CierrePorCajeraTests
{
    private static readonly DateOnly Hoy = new(2026, 10, 15);

    [Fact]
    public void Sin_cajera_el_cierre_es_de_la_caja_general()
    {
        var cierre = new CierreCaja(Hoy, Guid.NewGuid(), 100m, 0m, 0m, 1, 0, 0,
            100m, "Administrador", null, DateTimeOffset.UtcNow);

        Assert.Equal(CierreCaja.CajaGeneral, cierre.CajeroId);
        Assert.True(cierre.EsGeneral);
        Assert.Null(cierre.CajeroNombre);
    }

    [Fact]
    public void El_cierre_guarda_de_que_cajera_es_la_caja()
    {
        var id = Guid.NewGuid().ToString();
        var cierre = new CierreCaja(Hoy, Guid.NewGuid(), 100m, 0m, 0m, 1, 0, 0,
            100m, "María López", null, DateTimeOffset.UtcNow, cajeroId: $" {id} ", cajeroNombre: " María López ");

        Assert.Equal(id, cierre.CajeroId);
        Assert.Equal("María López", cierre.CajeroNombre);
        Assert.False(cierre.EsGeneral);
    }

    [Fact]
    public void Recibo_y_egreso_guardan_el_id_de_quien_los_registro()
    {
        var egreso = new Egreso(Hoy, "Servicios", "Pago de luz", "DELAPAZ", 120m, Guid.NewGuid(), "Efectivo", null, null);
        egreso.AsignarRegistradoPor("María López", " cajera-1 ");

        Assert.Equal("cajera-1", egreso.RegistradoPorId);
    }
}
