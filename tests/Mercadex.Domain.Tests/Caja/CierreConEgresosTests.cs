using Mercadex.Domain.Caja;
using Mercadex.Domain.Finanzas;
using Xunit;

namespace Mercadex.Domain.Tests.Caja;

public sealed class CierreConEgresosTests
{
    private static readonly DateOnly Hoy = new(2026, 10, 13);

    [Fact]
    public void El_efectivo_esperado_descuenta_los_egresos_pagados_en_efectivo()
    {
        // Cobrado en efectivo 500, pagado un gasto de 120: en caja deben quedar 380.
        var cierre = new CierreCaja(Hoy, Guid.NewGuid(), 500m, 0m, 0m, 5, 0, 0,
            380m, "María López", null, DateTimeOffset.UtcNow, egresosEfectivo: 120m, cantidadEgresos: 1);

        Assert.Equal(380m, cierre.EfectivoEsperado);
        Assert.Equal(0m, cierre.Diferencia);
    }

    [Fact]
    public void Sin_egresos_se_calcula_como_antes()
    {
        var cierre = new CierreCaja(Hoy, Guid.NewGuid(), 500m, 0m, 0m, 5, 0, 0,
            450m, "María López", "Faltan 50", DateTimeOffset.UtcNow);

        Assert.Equal(-50m, cierre.Diferencia);
    }

    [Fact]
    public void Diferencia_con_egresos_exige_observacion()
    {
        Assert.Throws<ArgumentException>(() => new CierreCaja(Hoy, Guid.NewGuid(), 500m, 0m, 0m, 5, 0, 0,
            500m, "María López", null, DateTimeOffset.UtcNow, egresosEfectivo: 120m, cantidadEgresos: 1));
    }

    [Fact]
    public void Egreso_guarda_quien_lo_registro_y_quien_lo_anulo()
    {
        var egreso = new Egreso(Hoy, "Servicios", "Pago de luz", "DELAPAZ", 120m, Guid.NewGuid(), "Efectivo", null, null);

        egreso.AsignarRegistradoPor(" María López ");
        egreso.Anular("Se registró dos veces", DateTimeOffset.UtcNow, "Administrador");

        Assert.Equal("María López", egreso.RegistradoPor);
        Assert.Equal("Administrador", egreso.AnuladoPor);
        Assert.True(egreso.Anulado);
    }
}
