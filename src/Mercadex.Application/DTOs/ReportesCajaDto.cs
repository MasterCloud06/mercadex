namespace Mercadex.Application.DTOs;

public sealed record ReporteIngresosDto(
    DateTimeOffset FechaInicio,
    DateTimeOffset FechaFin,
    int CantidadRecibos,
    decimal Total);


public sealed record ReporteConceptoDto(
    string Concepto,
    int Cantidad,
    decimal Total);


public sealed record ReporteDetalleCobroDto(
    long NumeroRecibo,
    Guid UnidadComercialId,
    DateTimeOffset Fecha,
    string Concepto,
    decimal Monto);