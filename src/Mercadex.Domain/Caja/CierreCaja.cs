using Mercadex.Domain.Common;

namespace Mercadex.Domain.Caja;

/// <summary>
/// Cierre de caja (arqueo) de un día y una moneda.
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
        int cantidadPagos,
        decimal efectivoContado,
        string responsable,
        string? observacion,
        DateTimeOffset fechaRegistroUtc)
    {
        if (fecha == default)
            throw new ArgumentException("La fecha del cierre es obligatoria.", nameof(fecha));

        if (monedaId == Guid.Empty)
            throw new ArgumentException("La moneda es obligatoria.", nameof(monedaId));

        if (efectivo < 0 || qr < 0 || otros < 0 || cantidadPagos < 0)
            throw new ArgumentException("Los totales del cierre no pueden ser negativos.");

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

        var diferencia = efectivoContado - efectivo;

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
        CantidadPagos = cantidadPagos;
        EfectivoContado = efectivoContado;
        Diferencia = diferencia;
        Responsable = responsable;
        Observacion = observacion;
        FechaRegistroUtc = fechaRegistroUtc;
    }

    public DateOnly Fecha { get; private set; }

    public Guid MonedaId { get; private set; }

    public decimal Efectivo { get; private set; }

    public decimal Qr { get; private set; }

    public decimal Otros { get; private set; }

    public decimal Total { get; private set; }

    public int CantidadPagos { get; private set; }

    public decimal EfectivoContado { get; private set; }

    public decimal Diferencia { get; private set; }

    public string Responsable { get; private set; } = "";

    public string? Observacion { get; private set; }

    public DateTimeOffset FechaRegistroUtc { get; private set; }
}
