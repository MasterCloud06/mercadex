using Microsoft.AspNetCore.Authorization;
using Mercadex.Application.DTOs;
using Mercadex.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Mercadex.Application.DTOs.Caja;

namespace Mercadex.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CajaController : ControllerBase
{
    private readonly ICajaService _service;
    private readonly IPagoRepository _pagoRepository;
    private readonly IReciboRepository _reciboRepository;

    public CajaController(
        ICajaService service,
        IPagoRepository pagoRepository,
        IReciboRepository reciboRepository)
    {
        _service = service;
        _pagoRepository = pagoRepository;
        _reciboRepository = reciboRepository;
    }


    [HttpPost("pagos")]
    public async Task<ActionResult<PagoDto>> CrearPago(
        CrearPagoDto dto)
    {
        var pago = await _service.RegistrarPagoAsync(dto);

        return Ok(pago);
    }


    [HttpGet("pagos/{id:guid}")]
    public async Task<ActionResult<PagoDto>> ObtenerPago(
        Guid id)
    {
        var pago = await _pagoRepository.GetByIdAsync(id);

        if (pago is null)
        {
            return NotFound();
        }

        return Ok(pago);
    }


    [HttpPost("recibos")]
    public async Task<ActionResult<ReciboDto>> CrearRecibo(
        CrearReciboDto dto)
    {
        var recibo = await _service.GenerarReciboAsync(dto);

        return Ok(recibo);
    }


    [HttpGet("recibos/{id:guid}")]
    public async Task<ActionResult<ReciboDto>> ObtenerRecibo(
        Guid id)
    {
        var recibo = await _reciboRepository.GetByIdAsync(id);

        if (recibo is null)
        {
            return NotFound();
        }

        return Ok(recibo);
    }

[HttpPut("recibos/{id:guid}/anular")]
[Authorize(Roles = "Administrador")]
public async Task<IActionResult> AnularRecibo(
    Guid id,
    AnularReciboDto dto)
{
    var usuario = User.Identity?.Name;

    if (string.IsNullOrWhiteSpace(usuario))
    {
        return Unauthorized(new
        {
            mensaje =
                "No se pudo identificar al usuario autenticado."
        });
    }

    var resultado =
        await _service.AnularReciboAsync(
            id,
            dto,
            usuario);

    if (!resultado)
    {
        return NotFound(new
        {
            mensaje = "No se encontró el recibo."
        });
    }

    return NoContent();
}

[HttpPost("cobrar")]
public async Task<ActionResult<ResultadoCobroDto>> ProcesarCobro(
    ProcesarCobroDto dto)
{
    var resultado = await _service
        .ProcesarCobroAsync(dto);

    return Ok(resultado);
}

[HttpGet("recibos")]
public async Task<ActionResult<IEnumerable<ReciboDto>>> ObtenerRecibos()
{
    var recibos = await _reciboRepository.GetAllAsync();

    return Ok(recibos);
}

}