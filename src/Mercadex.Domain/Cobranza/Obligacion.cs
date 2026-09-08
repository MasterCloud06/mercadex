using Mercadex.Domain.Common;

namespace Mercadex.Domain.Cobranza;

public sealed class Obligacion : Entity
{
    private Obligacion()
    {
    }

    public Obligacion(
        Guid unidadComercialId,
        Guid conceptoCobroId,
        Guid monedaId,
        decimal monto,
        DateOnly fechaEmision,
        DateOnly? fechaVencimiento = null,
        int? periodoAnio = null,
        int? periodoMes = null)
    {
        if (unidadComercialId == Guid.Empty)
        {
            throw new ArgumentException(
                "La unidad comercial es obligatoria.",
                nameof(unidadComercialId));
        }

        if (conceptoCobroId == Guid.Empty)
        {
            throw new ArgumentException(
                "El concepto de cobro es obligatorio.",
                nameof(conceptoCobroId));
        }

        if (monedaId == Guid.Empty)
        {
            throw new ArgumentException(
                "La moneda es obligatoria.",
                nameof(monedaId));
        }

        if (monto <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(monto),
                "El monto debe ser mayor a cero.");
        }

        if (fechaVencimiento.HasValue &&
            fechaVencimiento.Value < fechaEmision)
        {
            throw new ArgumentException(
                "La fecha de vencimiento no puede ser anterior a la fecha de emisión.",
                nameof(fechaVencimiento));
        }

        if (periodoMes.HasValue &&
            (periodoMes.Value < 1 || periodoMes.Value > 12))
        {
            throw new ArgumentOutOfRangeException(
                nameof(periodoMes),
                "El mes debe estar entre 1 y 12.");
        }

        if (periodoMes.HasValue &&
            !periodoAnio.HasValue)
        {
            throw new ArgumentException(
                "Para especificar un mes también debe especificarse el año.",
                nameof(periodoAnio));
        }

        UnidadComercialId = unidadComercialId;
        ConceptoCobroId = conceptoCobroId;
        MonedaId = monedaId;

        Monto = monto;

        FechaEmision = fechaEmision;
        FechaVencimiento = fechaVencimiento;

        PeriodoAnio = periodoAnio;
        PeriodoMes = periodoMes;

        Estado = EstadoObligacion.Pendiente;
    }

    public Guid UnidadComercialId { get; private set; }

    public Guid ConceptoCobroId { get; private set; }

    public Guid MonedaId { get; private set; }

    public decimal Monto { get; private set; }

    public DateOnly FechaEmision { get; private set; }

    public DateOnly? FechaVencimiento { get; private set; }

    public int? PeriodoAnio { get; private set; }

    public int? PeriodoMes { get; private set; }

    public EstadoObligacion Estado { get; private set; }

    public bool EstaPendiente =>
        Estado == EstadoObligacion.Pendiente;

    public bool EstaPagada =>
        Estado == EstadoObligacion.Pagada;

    public void MarcarComoPagada()
    {
        if (Estado != EstadoObligacion.Pendiente)
        {
            throw new InvalidOperationException(
                "Solamente una obligación pendiente puede marcarse como pagada.");
        }

        Estado = EstadoObligacion.Pagada;
    }

    public void RestaurarComoPendiente()
    {
        if (Estado != EstadoObligacion.Pagada)
        {
            throw new InvalidOperationException(
                "Solamente una obligación pagada puede restaurarse como pendiente.");
        }

        Estado = EstadoObligacion.Pendiente;
    }
}
