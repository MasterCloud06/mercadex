namespace Mercadex.Application.DTOs;

public sealed record ReciboConsultaDto(
    Guid Id,
    long Numero,
    Guid UnidadComercialId,
    Guid PagoId,
    DateTimeOffset FechaEmision,
    string Estado,
    decimal Total);