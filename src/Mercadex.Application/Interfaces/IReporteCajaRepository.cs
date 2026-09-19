using Mercadex.Application.DTOs;

namespace Mercadex.Application.Interfaces;

public interface IReporteCajaRepository
{
    Task<ReporteIngresosDto> ObtenerIngresosAsync(
        DateTimeOffset inicio,
        DateTimeOffset fin);


    Task<IReadOnlyList<ReporteConceptoDto>>
        ObtenerPorConceptoAsync(
            DateTimeOffset inicio,
            DateTimeOffset fin);


    Task<IReadOnlyList<ReporteDetalleCobroDto>>
        ObtenerDetalleAsync(
            DateTimeOffset inicio,
            DateTimeOffset fin);
}