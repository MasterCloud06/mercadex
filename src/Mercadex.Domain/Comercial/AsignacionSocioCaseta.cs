using Mercadex.Domain.Common;
using Mercadex.Domain.Socios;

namespace Mercadex.Domain.Comercial;

public sealed class AsignacionSocioCaseta : Entity
{
    private AsignacionSocioCaseta()
    {
    }


    public AsignacionSocioCaseta(
        Guid socioId,
        Guid unidadComercialId,
        DateTimeOffset fechaAsignacion)
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

        FechaAsignacion = fechaAsignacion;

        Activa = true;
    }


    public Guid SocioId { get; private set; }


    public Guid UnidadComercialId { get; private set; }


    public DateTimeOffset FechaAsignacion { get; private set; }


    public bool Activa { get; private set; }


    public void Desactivar()
    {
        Activa = false;
    }


    public void Activar()
    {
        Activa = true;
    }
}