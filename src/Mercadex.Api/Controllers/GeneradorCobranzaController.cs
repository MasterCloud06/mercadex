using Mercadex.Application.DTOs.Cobranza;
using Mercadex.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Mercadex.Api.Controllers;

[ApiController]
[Route("api/generador-cobranza")]
public sealed class GeneradorCobranzaController
    : ControllerBase
{
    private readonly GeneradorCobranzaService _service;


    public GeneradorCobranzaController(
        GeneradorCobranzaService service)
    {
        _service = service;
    }


    [HttpPost]
    public async Task<IActionResult> Generar(
        GenerarCobranzaDto dto)
    {
        var resultado =
            await _service.GenerarAsync(dto);


        return Ok(resultado);
    }
}