namespace Mercadex.Application.DTOs;

public sealed record ReciboDto(
    Guid Id,
    long Numero,
    Guid UnidadComercialId,
    Guid PagoId,
    DateTimeOffset FechaEmision,
    string Estado,
    decimal Total,
    IEnumerable<DetalleReciboDto> Detalles,
    string? MotivoAnulacion = null,
    DateTimeOffset? FechaAnulacion = null,
    string? AnuladoPor = null);