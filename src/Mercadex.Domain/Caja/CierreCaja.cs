using Mercadex.Domain.Common;

namespace Mercadex.Domain.Caja;

/// <summary>
/// Cierre de caja (arqueo) de un día, una moneda y una caja.
/// Desde la 1.6.3 cada cajera cierra su propia caja (sus cobros y sus
/// egresos en efectivo). Los cierres anteriores son de la caja general.
/// Los subtotales los calcula el servidor a partir de los cobros
/// válidos; el cajero solo informa el efectivo contado.
/// Un cierre registrado no se modifica.
/// </summary>
public sealed class CierreCaja : Entity
{
    private CierreCaja()
    {
    }

    public CierreCaja(
        DateOnly fecha,
        Guid monedaId,
        decimal efectivo,
        decimal qr,
        decimal otros,
        int cantidadEfectivo,
        int cantidadQr,
        int cantidadOtros,
        decimal efectivoContado,
        string responsable,
        string? observacion,
        DateTimeOffset fechaRegistroUtc,
        decimal egresosEfectivo = 0m,
        int cantidadEgresos = 0,
        string? cajeroId = null,
        string? cajeroNombre = null)
    {
        if (fecha == default)
            throw new ArgumentException("La fecha del cierre es obligatoria.", nameof(fecha));

        if (monedaId == Guid.Empty)
            throw new ArgumentException("La moneda es obligatoria.", nameof(monedaId));

        if (efectivo < 0 || qr < 0 || otros < 0 ||
            cantidadEfectivo < 0 || cantidadQr < 0 || cantidadOtros < 0)
            throw new ArgumentException("Los totales del cierre no pueden ser negativos.");

        if (egresosEfectivo < 0 || cantidadEgresos < 0)
            throw new ArgumentException("Los egresos del cierre no pueden ser negativos.");

        if (efectivoContado < 0)
            throw new ArgumentException("El efectivo contado no puede ser negativo.", nameof(efectivoContado));

        responsable = responsable?.Trim() ?? "";

        if (responsable.Length is < 3 or > 120)
            throw new ArgumentException(
                "Indique el nombre del responsable del cierre (3 a 120 caracteres).",
                nameof(responsable));

        observacion = string.IsNullOrWhiteSpace(observacion) ? null : observacion.Trim();

        if (observacion?.Length > 500)
            throw new ArgumentException("La observación no puede superar 500 caracteres.", nameof(observacion));

        // En caja debe haber lo cobrado en efectivo menos lo pagado en efectivo.
        var diferencia = efectivoContado - (efectivo - egresosEfectivo);

        // Toda diferencia de efectivo debe quedar explicada.
        if (diferencia != 0 && observacion is null)
            throw new ArgumentException(
                "Existe una diferencia de efectivo. Escriba una observación que la explique.");

        Fecha = fecha;
        MonedaId = monedaId;
        Efectivo = efectivo;
        Qr = qr;
        Otros = otros;
        Total = efectivo + qr + otros;
        CantidadEfectivo = cantidadEfectivo;
        CantidadQr = cantidadQr;
        CantidadOtros = cantidadOtros;
        CantidadPagos = cantidadEfectivo + cantidadQr + cantidadOtros;
        EgresosEfectivo = egresosEfectivo;
        CantidadEgresos = cantidadEgresos;
        EfectivoContado = efectivoContado;
        Diferencia = diferencia;
        Responsable = responsable;
        Observacion = observacion;
        FechaRegistroUtc = fechaRegistroUtc;

        cajeroId = cajeroId?.Trim();
        CajeroId = string.IsNullOrEmpty(cajeroId) ? CajaGeneral : (cajeroId.Length > 64 ? cajeroId[..64] : cajeroId);

        cajeroNombre = cajeroNombre?.Trim();
        CajeroNombre = string.IsNullOrEmpty(cajeroNombre)
            ? null
            : (cajeroNombre.Length > 120 ? cajeroNombre[..120] : cajeroNombre);
    }

    /// <summary>Caja de todo el día (cierres anteriores a la 1.6.3 o sin sesión).</summary>
    public const string CajaGeneral = "general";

    public DateOnly Fecha { get; private set; }

    public Guid MonedaId { get; private set; }

    public decimal Efectivo { get; private set; }

    public decimal Qr { get; private set; }

    public decimal Otros { get; private set; }

    public decimal Total { get; private set; }

    public int CantidadEfectivo { get; private set; }

    public int CantidadQr { get; private set; }

    public int CantidadOtros { get; private set; }

    public int CantidadPagos { get; private set; }

    /// <summary>Egresos (gastos) pagados en efectivo ese día y en esa moneda.</summary>
    public decimal EgresosEfectivo { get; private set; }

    public int CantidadEgresos { get; private set; }

    /// <summary>Efectivo que debe haber en caja: cobros en efectivo menos egresos en efectivo.</summary>
    public decimal EfectivoEsperado => Efectivo - EgresosEfectivo;

    public decimal EfectivoContado { get; private set; }

    public decimal Diferencia { get; private set; }

    public string Responsable { get; private set; } = "";

    public string? Observacion { get; private set; }

    public DateTimeOffset FechaRegistroUtc { get; private set; }

    /// <summary>De quién es la caja cerrada: identificador de la cajera o "general".</summary>
    public string CajeroId { get; private set; } = CajaGeneral;

    /// <summary>Nombre de la cajera dueña de la caja (vacío en la caja general).</summary>
    public string? CajeroNombre { get; private set; }

    public bool EsGeneral => CajeroId == CajaGeneral;
}
