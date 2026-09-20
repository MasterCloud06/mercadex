namespace Mercadex.Application.DTOs.Cobranza;

public sealed record GenerarCobranzaDto(
    Guid ConceptoCobroId,
    Guid MonedaId,
    decimal Monto,
    DateOnly FechaEmision,
    DateOnly? FechaVencimiento,
    int PeriodoAnio,
    int PeriodoMes);