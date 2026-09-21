using Mercadex.Domain.Common;

namespace Mercadex.Domain.Comercial;

public sealed class Caseta : Entity
{
    private Caseta()
    {
    }


    public Caseta(
        string numero,
        Guid socioId)
    {
        if (string.IsNullOrWhiteSpace(numero))
        {
            throw new ArgumentException(
                "El número de caseta es obligatorio.",
                nameof(numero));
        }


        if (socioId == Guid.Empty)
        {
            throw new ArgumentException(
                "El socio es obligatorio.",
                nameof(socioId));
        }


        Numero = numero.Trim();

        SocioId = socioId;

        Estado = EstadoCaseta.Activa;
    }


    public string Numero { get; private set; }
        = string.Empty;


    public Guid SocioId { get; private set; }


    public EstadoCaseta Estado { get; private set; }


    public void Desactivar()
    {
        Estado = EstadoCaseta.Inactiva;
    }
}