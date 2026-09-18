namespace Mercadex.Application.DTOs;

public sealed record ObligacionDto(
    Guid Id,
    Guid UnidadComercialId,
    Guid ConceptoCobroId,
    Guid MonedaId,
    decimal Monto,
    DateOnly FechaEmision,
    DateOnly? FechaVencimiento,
    int? PeriodoAnio,
    int? PeriodoMes,
    string Estado);