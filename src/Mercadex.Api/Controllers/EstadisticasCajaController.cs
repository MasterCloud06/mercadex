using Mercadex.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Mercadex.Api.Controllers;

[ApiController]
[Route("api/estadisticas-caja")]
public sealed class EstadisticasCajaController 
    : ControllerBase
{
    private readonly IEstadisticaCajaService _service;


    public EstadisticasCajaController(
        IEstadisticaCajaService service)
    {
        _service = service;
    }


    [HttpGet]
    public async Task<IActionResult> Obtener(
        [FromQuery] DateTimeOffset? inicio,
        [FromQuery] DateTimeOffset? fin)
    {
        if (!inicio.HasValue || !fin.HasValue)
        {
            return BadRequest(new
            {
                mensaje = "Debe enviar fecha inicio y fecha fin."
            });
        }


        if (inicio.Value > fin.Value)
        {
            return BadRequest(new
            {
                mensaje = "La fecha inicio no puede ser mayor que la fecha fin."
            });
        }


        var resultado =
            await _service.ObtenerAsync(
                inicio.Value,
                fin.Value);


        return Ok(resultado);
    }
}