using Mercadex.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Mercadex.Api.Controllers;

[ApiController]
[Route("api/caja")]
public sealed class CajaConsultasController : ControllerBase
{
    private readonly ConsultaCajaService _service;


    public CajaConsultasController(
        ConsultaCajaService service)
    {
        _service = service;
    }


    [HttpGet("deudas/{unidadComercialId:guid}")]
    public async Task<IActionResult> ObtenerDeudas(
        Guid unidadComercialId)
    {
        var resultado =
            await _service.ObtenerDeudasAsync(
                unidadComercialId);

        return Ok(resultado);
    }
}