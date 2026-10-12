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
    public IActionResult Generar(
        GenerarCobranzaDto dto)
    {
        // Desde Mercadex 1.5 las obligaciones mensuales se generan
        // por tarifa individual (api/tarifas-unidad/generar-mensual).
        // El generador de precio uniforme queda deshabilitado.

        return Conflict(new
        {
            mensaje =
                "Generador uniforme deshabilitado. " +
                "Utiliza generacion mensual por tarifa individual."
        });
    }
}