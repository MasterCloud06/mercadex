namespace Mercadex.Application.DTOs.Ingresos;


public sealed record IngresoMovimientoDto
(
    DateTimeOffset Fecha,
    long NumeroRecibo,
    string CodigoUnidad,
    string Bloque,
    string NombreSocio,
    string NombrePagador,
    string FormaPago,
    decimal Monto,
    string Estado
);