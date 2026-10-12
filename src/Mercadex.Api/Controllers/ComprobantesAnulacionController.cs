using Mercadex.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mercadex.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrador")]
[Route("api/comprobantes-anulacion")]
public sealed class ComprobantesAnulacionController : ControllerBase
{
    private readonly MercadexDbContext _db;

    public ComprobantesAnulacionController(
        MercadexDbContext db)
    {
        _db = db;
    }

    // MERCADEX_CONSULTA_COMPROBANTES_ANULACION_V1

    // Modulo de Mercadex 1.5: protegido por [Authorize(Roles = "Administrador")].
    // Ya no depende de variables de entorno ni del nombre de la base.
    private static bool EntornoPermitido() => true;

    [HttpGet]
    public async Task<IActionResult> Listar(
        CancellationToken ct)
    {
        if (!EntornoPermitido())
            return StatusCode(403);

        Response.Headers["Cache-Control"] = "no-store";

        var total = await _db.ComprobantesAnulacion
            .AsNoTracking()
            .CountAsync(ct);

        var comprobantes = await _db.ComprobantesAnulacion
            .AsNoTracking()
            .OrderByDescending(x => x.FechaAnulacionUtc)
            .ThenByDescending(x => x.Id)
            .Take(100)
            .Select(x => new
            {
                x.Id,
                x.ReciboId,
                x.NumeroRecibo,
                x.Referencia,
                x.MontoTotal,
                x.Responsable,
                x.FechaAnulacionUtc
            })
            .ToListAsync(ct);

        return Ok(new
        {
            cantidad = total,
            mostrados = comprobantes.Count,
            comprobantes
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Obtener(
        Guid id,
        CancellationToken ct)
    {
        if (!EntornoPermitido())
            return StatusCode(403);

        Response.Headers["Cache-Control"] = "no-store";

        var comprobante = await _db.ComprobantesAnulacion
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.ReciboId,
                x.NumeroRecibo,
                x.UnidadComercialId,
                x.PagoId,
                x.MonedaId,
                x.MontoTotal,
                x.FechaEmisionRecibo,
                x.FechaAnulacionUtc,
                x.Motivo,
                x.Responsable,
                x.Referencia
            })
            .SingleOrDefaultAsync(ct);

        if (comprobante is null)
            return NotFound(new
            {
                mensaje = "Comprobante no encontrado."
            });

        return Ok(comprobante);
    }

    [HttpGet("recibo/{reciboId:guid}")]
    public async Task<IActionResult> PorRecibo(
        Guid reciboId,
        CancellationToken ct)
    {
        if (!EntornoPermitido())
            return StatusCode(403);

        Response.Headers["Cache-Control"] = "no-store";

        var comprobante = await _db.ComprobantesAnulacion
            .AsNoTracking()
            .Where(x => x.ReciboId == reciboId)
            .Select(x => new
            {
                x.Id,
                x.ReciboId,
                x.NumeroRecibo,
                x.UnidadComercialId,
                x.PagoId,
                x.MonedaId,
                x.MontoTotal,
                x.FechaEmisionRecibo,
                x.FechaAnulacionUtc,
                x.Motivo,
                x.Responsable,
                x.Referencia
            })
            .SingleOrDefaultAsync(ct);

        if (comprobante is null)
            return NotFound(new
            {
                mensaje = "Este recibo no tiene comprobante de anulacion."
            });

        return Ok(comprobante);
    }
}