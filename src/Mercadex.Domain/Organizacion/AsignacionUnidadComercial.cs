using Mercadex.Domain.Common;

namespace Mercadex.Domain.Organizacion;

public sealed class AsignacionUnidadComercial : Entity
{
    private AsignacionUnidadComercial()
    {
    }

    public AsignacionUnidadComercial(
        Guid socioId,
        Guid unidadComercialId,
        TipoVinculoUnidad tipoVinculo,
        DateOnly fechaInicio)
    {
        if (socioId == Guid.Empty)
        {
            throw new ArgumentException(
                "El socio es obligatorio.",
                nameof(socioId));
        }

        if (unidadComercialId == Guid.Empty)
        {
            throw new ArgumentException(
                "La unidad comercial es obligatoria.",
                nameof(unidadComercialId));
        }

        SocioId = socioId;
        UnidadComercialId = unidadComercialId;
        TipoVinculo = tipoVinculo;
        FechaInicio = fechaInicio;
    }

    public Guid SocioId { get; private set; }

    public Guid UnidadComercialId { get; private set; }

    public TipoVinculoUnidad TipoVinculo { get; private set; }

    public DateOnly FechaInicio { get; private set; }

    public DateOnly? FechaFin { get; private set; }

    public bool EstaVigente =>
        FechaFin is null;

    public void Finalizar(DateOnly fechaFin)
    {
        if (FechaFin is not null)
        {
            throw new InvalidOperationException(
                "La asignación ya fue finalizada.");
        }

        if (fechaFin < FechaInicio)
        {
            throw new ArgumentException(
                "La fecha final no puede ser anterior a la fecha inicial.",
                nameof(fechaFin));
        }

        FechaFin = fechaFin;
    }
}
