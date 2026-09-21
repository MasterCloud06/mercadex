using Mercadex.Application.DTOs.Ingresos;
using Mercadex.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;


namespace Mercadex.Api.Controllers;


[ApiController]
[Route("api/[controller]")]
public sealed class IngresosController : ControllerBase
{

    private readonly IIngresoService _service;


    public IngresosController(
        IIngresoService service)
    {
        _service = service;
    }



    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<IngresoMovimientoDto>>> Get()
    {
        var ingresos = await _service.ObtenerAsync();

        return Ok(ingresos);
    }

}