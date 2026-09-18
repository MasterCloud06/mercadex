namespace Mercadex.Application.DTOs;

public sealed record CrearObligacionDto(
    Guid UnidadComercialId,
    Guid ConceptoCobroId,
    Guid MonedaId,
    decimal Monto,
    DateOnly FechaEmision,
    DateOnly? FechaVencimiento,
    int? PeriodoAnio,
    int? PeriodoMes);