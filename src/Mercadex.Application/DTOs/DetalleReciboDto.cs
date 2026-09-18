namespace Mercadex.Application.DTOs;

public sealed record DetalleReciboDto(
    Guid ObligacionId,
    Guid ConceptoCobroId,
    string Concepto,
    decimal Monto,
    int? PeriodoAnio,
    int? PeriodoMes);
