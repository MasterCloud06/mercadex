using Mercadex.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Mercadex.Api.Controllers;

[ApiController]
[Route("api/reportes-caja")]
public sealed class ReportesCajaController : ControllerBase
{
    private readonly IReporteCajaService _service;


    public ReportesCajaController(
        IReporteCajaService service)
    {
        _service = service;
    }


    [HttpGet("ingresos")]
    public async Task<IActionResult> Ingresos(
        DateTimeOffset inicio,
        DateTimeOffset fin)
    {
        return Ok(
            await _service.ObtenerIngresosAsync(
                inicio,
                fin));
    }


    [HttpGet("conceptos")]
    public async Task<IActionResult> Conceptos(
        DateTimeOffset inicio,
        DateTimeOffset fin)
    {
        return Ok(
            await _service.ObtenerPorConceptoAsync(
                inicio,
                fin));
    }


    [HttpGet("detalle")]
    public async Task<IActionResult> Detalle(
        DateTimeOffset inicio,
        DateTimeOffset fin)
    {
        return Ok(
            await _service.ObtenerDetalleAsync(
                inicio,
                fin));
    }
}