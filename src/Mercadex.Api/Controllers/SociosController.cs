using Mercadex.Application.DTOs;
using Mercadex.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Mercadex.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class SociosController : ControllerBase
{
    private readonly ISocioService _service;

    public SociosController(ISocioService service)
    {
        _service = service;
    }


    [HttpGet]
    public async Task<ActionResult<IEnumerable<SocioDto>>> GetAll()
    {
        var socios = await _service.GetAllAsync();

        return Ok(socios);
    }


    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SocioDto>> GetById(Guid id)
    {
        var socio = await _service.GetByIdAsync(id);

        if (socio is null)
        {
            return NotFound();
        }

        return Ok(socio);
    }


    [HttpPost]
    public async Task<ActionResult<SocioDto>> Crear(
        CrearSocioDto dto)
    {
        var socio = await _service.CrearAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = socio.Id },
            socio);
    }


    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SocioDto>> Actualizar(
        Guid id,
        ActualizarSocioDto dto)
    {
        var socio = await _service.ActualizarAsync(id, dto);

        if (socio is null)
        {
            return NotFound();
        }

        return Ok(socio);
    }


    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Desactivar(
        Guid id)
    {
        var eliminado = await _service.DesactivarAsync(id);

        if (!eliminado)
        {
            return NotFound();
        }

        return NoContent();
    }
}