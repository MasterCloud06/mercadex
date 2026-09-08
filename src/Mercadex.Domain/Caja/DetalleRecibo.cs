using Mercadex.Domain.Common;

namespace Mercadex.Domain.Caja;

public sealed class DetalleRecibo : Entity
{
    private DetalleRecibo()
    {
    }

    public DetalleRecibo(
        Guid obligacionId,
        Guid conceptoCobroId,
        string concepto,
        decimal monto,
        int? periodoAnio = null,
        int? periodoMes = null)
    {
        if (obligacionId == Guid.Empty)
        {
            throw new ArgumentException(
                "La obligación es obligatoria.",
                nameof(obligacionId));
        }

        if (conceptoCobroId == Guid.Empty)
        {
            throw new ArgumentException(
                "El concepto de cobro es obligatorio.",
                nameof(conceptoCobroId));
        }

        if (string.IsNullOrWhiteSpace(concepto))
        {
            throw new ArgumentException(
                "La descripción del concepto es obligatoria.",
                nameof(concepto));
        }

        if (monto <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(monto),
                "El monto debe ser mayor a cero.");
        }

        ObligacionId = obligacionId;
        ConceptoCobroId = conceptoCobroId;

        Concepto = concepto.Trim();
        Monto = monto;

        PeriodoAnio = periodoAnio;
        PeriodoMes = periodoMes;
    }

    public Guid ObligacionId { get; private set; }

    public Guid ConceptoCobroId { get; private set; }

    public string Concepto { get; private set; } = string.Empty;

    public decimal Monto { get; private set; }

    public int? PeriodoAnio { get; private set; }

    public int? PeriodoMes { get; private set; }
}
