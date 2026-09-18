namespace Mercadex.Application.DTOs;

public sealed record CrearPagoDto(
    Guid MonedaId,
    decimal Monto,
    int FormaPago,
    string NombrePagador,
    decimal TipoCambio,
    string? Referencia);