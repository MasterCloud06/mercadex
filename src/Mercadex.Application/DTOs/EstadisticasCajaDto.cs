namespace Mercadex.Application.DTOs;

public sealed record EstadisticasCajaDto(
    DateTimeOffset Inicio,
    DateTimeOffset Fin,
    int CantidadPagos,
    int CantidadRecibos,
    decimal TotalRecaudado,
    decimal PromedioPago,
    IReadOnlyList<EstadisticaFormaPagoDto> FormasPago);


public sealed record EstadisticaFormaPagoDto(
    string FormaPago,
    int Cantidad,
    decimal Total);