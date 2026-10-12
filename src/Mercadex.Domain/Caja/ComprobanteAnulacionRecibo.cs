using Mercadex.Domain.Common;

namespace Mercadex.Domain.Caja;

public sealed class ComprobanteAnulacionRecibo : Entity
{
    private ComprobanteAnulacionRecibo()
    {
    }

    public ComprobanteAnulacionRecibo(
        Guid reciboId,
        long numeroRecibo,
        Guid unidadComercialId,
        Guid pagoId,
        Guid monedaId,
        decimal montoTotal,
        DateTimeOffset fechaEmisionRecibo,
        DateTimeOffset fechaAnulacionUtc,
        string motivo,
        string responsable)
    {
        if (reciboId == Guid.Empty)
            throw new ArgumentException(
                "El recibo es obligatorio.",
                nameof(reciboId));

        if (numeroRecibo <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(numeroRecibo));

        if (unidadComercialId == Guid.Empty)
            throw new ArgumentException(
                "La unidad comercial es obligatoria.",
                nameof(unidadComercialId));

        if (pagoId == Guid.Empty)
            throw new ArgumentException(
                "El pago es obligatorio.",
                nameof(pagoId));

        if (monedaId == Guid.Empty)
            throw new ArgumentException(
                "La moneda es obligatoria.",
                nameof(monedaId));

        if (montoTotal <= 0 ||
            decimal.Round(montoTotal, 2) != montoTotal)
        {
            throw new ArgumentOutOfRangeException(
                nameof(montoTotal),
                "El importe debe ser positivo y tener maximo dos decimales.");
        }

        if (fechaEmisionRecibo == default ||
            fechaAnulacionUtc == default ||
            fechaAnulacionUtc < fechaEmisionRecibo)
        {
            throw new ArgumentException(
                "La fecha de anulacion no puede preceder a la emision.",
                nameof(fechaAnulacionUtc));
        }

        if (string.IsNullOrWhiteSpace(motivo) ||
            motivo.Trim().Length < 5 ||
            motivo.Trim().Length > 500)
        {
            throw new ArgumentException(
                "El motivo debe tener entre 5 y 500 caracteres.",
                nameof(motivo));
        }

        if (string.IsNullOrWhiteSpace(responsable) ||
            responsable.Trim().Length > 120)
        {
            throw new ArgumentException(
                "El responsable es obligatorio y no puede superar 120 caracteres.",
                nameof(responsable));
        }

        ReciboId = reciboId;
        NumeroRecibo = numeroRecibo;
        UnidadComercialId = unidadComercialId;
        PagoId = pagoId;
        MonedaId = monedaId;
        MontoTotal = montoTotal;

        FechaEmisionRecibo = fechaEmisionRecibo.ToUniversalTime();
        FechaAnulacionUtc = fechaAnulacionUtc.ToUniversalTime();

        Motivo = motivo.Trim();
        Responsable = responsable.Trim();

        Referencia = "ANU-" + Id.ToString("N").ToUpperInvariant();
    }

    public Guid ReciboId { get; private set; }

    public long NumeroRecibo { get; private set; }

    public Guid UnidadComercialId { get; private set; }

    public Guid PagoId { get; private set; }

    public Guid MonedaId { get; private set; }

    public decimal MontoTotal { get; private set; }

    public DateTimeOffset FechaEmisionRecibo { get; private set; }

    public DateTimeOffset FechaAnulacionUtc { get; private set; }

    public string Motivo { get; private set; } = "";

    public string Responsable { get; private set; } = "";

    public string Referencia { get; private set; } = "";
}