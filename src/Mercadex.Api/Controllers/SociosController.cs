using Mercadex.Application.DTOs.Socios;
using Mercadex.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Mercadex.Api.Controllers;

[ApiController]
[Route("api/socios")]
public sealed class SociosController : ControllerBase
{
    private readonly SocioService _service;


    public SociosController(
        SocioService service)
    {
        _service = service;
    }


    [HttpGet]
    public async Task<IActionResult> Obtener()
    {
        return Ok(
            await _service.ObtenerAsync());
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPorId(
        Guid id)
    {
        var socio =
            await _service.ObtenerPorIdAsync(id);


        if (socio is null)
        {
            return NotFound();
        }


        return Ok(socio);
    }


    [HttpPost]
    public async Task<IActionResult> Crear(
        CrearSocioDto dto)
    {
        var resultado =
            await _service.CrearAsync(dto);


        return Ok(resultado);
    }
}