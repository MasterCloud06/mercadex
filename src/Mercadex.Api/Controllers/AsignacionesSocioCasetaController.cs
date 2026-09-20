using Mercadex.Application.DTOs.Comercial;
using Mercadex.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Mercadex.Api.Controllers;

[ApiController]
[Route("api/asignaciones-socio-caseta")]
public sealed class AsignacionesSocioCasetaController
    : ControllerBase
{
    private readonly IAsignacionSocioCasetaService _service;


    public AsignacionesSocioCasetaController(
        IAsignacionSocioCasetaService service)
    {
        _service = service;
    }


    [HttpGet]
    public async Task<IActionResult> Obtener()
    {
        var resultado =
            await _service.ObtenerAsync();

        return Ok(resultado);
    }


    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObtenerPorId(
        Guid id)
    {
        var resultado =
            await _service.ObtenerPorIdAsync(id);


        if (resultado is null)
        {
            return NotFound();
        }


        return Ok(resultado);
    }


    [HttpPost]
    public async Task<IActionResult> Crear(
        CrearAsignacionSocioCasetaDto dto)
    {
        var resultado =
            await _service.CrearAsync(dto);


        return Ok(resultado);
    }
}