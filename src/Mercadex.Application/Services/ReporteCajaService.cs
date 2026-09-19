using Mercadex.Application.DTOs;
using Mercadex.Application.Interfaces;

namespace Mercadex.Application.Services;

public sealed class ReporteCajaService 
    : IReporteCajaService
{
    private readonly IReporteCajaRepository _repository;


    public ReporteCajaService(
        IReporteCajaRepository repository)
    {
        _repository = repository;
    }


    public Task<ReporteIngresosDto> ObtenerIngresosAsync(
        DateTimeOffset inicio,
        DateTimeOffset fin)
    {
        return _repository.ObtenerIngresosAsync(
            inicio,
            fin);
    }


    public Task<IReadOnlyList<ReporteConceptoDto>>
        ObtenerPorConceptoAsync(
            DateTimeOffset inicio,
            DateTimeOffset fin)
    {
        return _repository.ObtenerPorConceptoAsync(
            inicio,
            fin);
    }


    public Task<IReadOnlyList<ReporteDetalleCobroDto>>
        ObtenerDetalleAsync(
            DateTimeOffset inicio,
            DateTimeOffset fin)
    {
        return _repository.ObtenerDetalleAsync(
            inicio,
            fin);
    }
}