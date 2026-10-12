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
        // MERCADEX_BLOQUEO_GENERADOR_UNIFORME_DEV
        // Nunca usar la generacion de precio uniforme
        // en el entorno de tarifas individuales.

        if (Environment.GetEnvironmentVariable(
                "MERCADEX_TARIFAS_DEV_MODE") == "SI")
        {
            return Conflict(new
            {
                mensaje =
                    "Generador uniforme deshabilitado. " +
                    "Utiliza generacion mensual por tarifa individual."
            });
        }
        var resultado =
            await _service.GenerarAsync(dto);


        return Ok(resultado);
    }
}