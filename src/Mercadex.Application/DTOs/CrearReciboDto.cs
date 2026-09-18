namespace Mercadex.Application.DTOs;

public sealed record CrearReciboDto(
    long Numero,
    Guid UnidadComercialId,
    Guid PagoId,
    IEnumerable<DetalleReciboDto> Detalles);