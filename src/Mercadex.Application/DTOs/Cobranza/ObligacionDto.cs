namespace Mercadex.Application.DTOs.Cobranza;

public sealed record ObligacionDto(
    Guid Id,
    Guid UnidadComercialId,
    Guid ConceptoCobroId,
    decimal Monto,
    DateTimeOffset FechaVencimiento,
    string Estado);