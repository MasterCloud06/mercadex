using Mercadex.Domain.Common;

namespace Mercadex.Domain.Cobranza;

public sealed class CambioMontoObligacion : Entity
{
    private CambioMontoObligacion()
    {
    }

    public CambioMontoObligacion(
        Guid obligacionId,
        decimal montoAnterior,
        decimal montoNuevo,
        string motivo,
        string responsable,
        DateTime fechaUtc)
    {
        if (obligacionId == Guid.Empty)
        {
            throw new ArgumentException(
                "La obligacion es obligatoria.",
                nameof(obligacionId));
        }

        if (montoAnterior <= 0 ||
            montoNuevo <= 0 ||
            decimal.Round(montoAnterior, 2) != montoAnterior ||
            decimal.Round(montoNuevo, 2) != montoNuevo)
        {
            throw new ArgumentOutOfRangeException(
                nameof(montoNuevo),
                "Los montos deben ser positivos y tener maximo dos decimales.");
        }

        if (montoAnterior == montoNuevo)
        {
            throw new ArgumentException(
                "Los montos anterior y nuevo deben ser diferentes.",
                nameof(montoNuevo));
        }

        if (string.IsNullOrWhiteSpace(motivo) ||
            motivo.Trim().Length < 5 ||
            motivo.Trim().Length > 500)
        {
            throw new ArgumentException(
                "El motivo debe contener entre 5 y 500 caracteres.",
                nameof(motivo));
        }

        if (string.IsNullOrWhiteSpace(responsable) ||
            responsable.Trim().Length > 120)
        {
            throw new ArgumentException(
                "El responsable es obligatorio.",
                nameof(responsable));
        }

        if (fechaUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException(
                "La fecha debe estar expresada en UTC.",
                nameof(fechaUtc));
        }

        ObligacionId = obligacionId;
        MontoAnterior = montoAnterior;
        MontoNuevo = montoNuevo;
        Motivo = motivo.Trim();
        Responsable = responsable.Trim();
        FechaUtc = fechaUtc;
    }

    public Guid ObligacionId { get; private set; }

    public decimal MontoAnterior { get; private set; }

    public decimal MontoNuevo { get; private set; }

    public string Motivo { get; private set; } = string.Empty;

    public string Responsable { get; private set; } = string.Empty;

    public DateTime FechaUtc { get; private set; }
}