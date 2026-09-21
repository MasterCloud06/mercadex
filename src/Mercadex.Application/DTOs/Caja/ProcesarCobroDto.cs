namespace Mercadex.Application.DTOs.Caja;

public sealed record ProcesarCobroDto(
    long NumeroRecibo,
    Guid UnidadComercialId,
    Guid MonedaId,
    decimal Monto,
    int FormaPago,
    string NombrePagador,
    decimal TipoCambio,
    string? Referencia,
    IEnumerable<ItemCobroDto> Items
);