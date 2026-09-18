namespace Mercadex.Application.DTOs;

public sealed record PagoDto(
    Guid Id,
    Guid MonedaId,
    decimal Monto,
    string FormaPago,
    string NombrePagador,
    string? Referencia,
    decimal TipoCambio,
    DateTimeOffset FechaRegistro,
    string Estado);