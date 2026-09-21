using Mercadex.Application.DTOs;
using Mercadex.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Mercadex.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ObligacionesController : ControllerBase
{
    private readonly IObligacionService _service;

    public ObligacionesController(
        IObligacionService service)
    {
        _service = service;
    }


    [HttpGet]
    public async Task<ActionResult<IEnumerable<ObligacionDto>>> GetAll()
    {
        var obligaciones = await _service.GetAllAsync();

        return Ok(obligaciones);
    }


    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ObligacionDto>> GetById(
        Guid id)
    {
        var obligacion = await _service.GetByIdAsync(id);

        if (obligacion is null)
        {
            return NotFound();
        }

        return Ok(obligacion);
    }


    [HttpPost]
    public async Task<ActionResult<ObligacionDto>> Crear(
        CrearObligacionDto dto)
    {
        var obligacion = await _service.CrearAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = obligacion.Id },
            obligacion);
    }


    [HttpPut("{id:guid}/pagar")]
    public async Task<IActionResult> MarcarComoPagada(
        Guid id)
    {
        var resultado = await _service.MarcarComoPagadaAsync(id);

        if (!resultado)
        {
            return NotFound();
        }

        return NoContent();
    }


    [HttpPut("{id:guid}/restaurar")]
    public async Task<IActionResult> RestaurarComoPendiente(
        Guid id)
    {
        var resultado = await _service.RestaurarComoPendienteAsync(id);

        if (!resultado)
        {
            return NotFound();
        }

        return NoContent();
    }

[HttpGet("unidad/{unidadComercialId:guid}/pendientes")]
public async Task<ActionResult<IEnumerable<ObligacionDto>>> ObtenerPendientesPorUnidad(
    Guid unidadComercialId)
{
    var obligaciones = await _service
        .ObtenerPendientesPorUnidadAsync(
            unidadComercialId);

    return Ok(obligaciones);
}

}