using Mercadex.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Mercadex.Api.Controllers;

[ApiController]
[Route("api/caja/recibos")]
public sealed class RecibosController : ControllerBase
{
    private readonly IConsultaRecibosService _service;


    public RecibosController(
        IConsultaRecibosService service)
    {
        _service = service;
    }


    [HttpGet]
    public async Task<IActionResult> ObtenerTodos()
    {
        var recibos =
            await _service.ObtenerTodosAsync();

        return Ok(recibos);
    }


    [HttpGet("fecha/{fecha}")]
    public async Task<IActionResult> ObtenerPorFecha(
        DateTimeOffset fecha)
    {
        var recibos =
            await _service.ObtenerPorFechaAsync(
                fecha);

        return Ok(recibos);
    }
}