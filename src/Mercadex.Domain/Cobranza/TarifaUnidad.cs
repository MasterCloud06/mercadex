using Mercadex.Domain.Common;

namespace Mercadex.Domain.Cobranza;

public sealed class TarifaUnidad : Entity
{
    private TarifaUnidad()
    {
    }

    public TarifaUnidad(
        Guid unidadComercialId,
        Guid conceptoCobroId,
        Guid monedaId,
        decimal monto)
    {
        if (unidadComercialId == Guid.Empty)
            throw new ArgumentException(
                "La unidad es obligatoria.",
                nameof(unidadComercialId));

        if (conceptoCobroId == Guid.Empty)
            throw new ArgumentException(
                "El concepto es obligatorio.",
                nameof(conceptoCobroId));

        UnidadComercialId = unidadComercialId;
        ConceptoCobroId = conceptoCobroId;

        Actualizar(monedaId, monto);
    }

    public Guid UnidadComercialId { get; private set; }

    public Guid ConceptoCobroId { get; private set; }

    public Guid MonedaId { get; private set; }

    public decimal Monto { get; private set; }

    public DateTimeOffset FechaActualizacionUtc { get; private set; }

    public void Actualizar(Guid monedaId, decimal monto)
    {
        if (monedaId == Guid.Empty)
            throw new ArgumentException(
                "La moneda es obligatoria.",
                nameof(monedaId));

        if (monto <= 0 || decimal.Round(monto, 2) != monto)
            throw new ArgumentOutOfRangeException(
                nameof(monto),
                "El monto debe ser positivo y tener maximo dos decimales.");

        MonedaId = monedaId;
        Monto = monto;

        FechaActualizacionUtc = DateTimeOffset.UtcNow;
    }
}