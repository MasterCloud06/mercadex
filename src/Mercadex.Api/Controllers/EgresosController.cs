using Mercadex.Domain.Finanzas;
using Mercadex.Infrastructure.Persistence;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mercadex.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class EgresosController : ControllerBase
{
    private readonly MercadexDbContext _context;

    public EgresosController(
        MercadexDbContext context)
    {
        _context = context;
    }

    // ============================================================
    // GET: /api/egresos
    // GET: /api/egresos?desde=2026-09-01&hasta=2026-09-30
    // GET: /api/egresos?categoria=Servicios
    // ============================================================

    [HttpGet]
    public async Task<ActionResult<
        IReadOnlyList<EgresoDto>>> Get(
        [FromQuery] DateOnly? desde = null,
        [FromQuery] DateOnly? hasta = null,
        [FromQuery] string? categoria = null,
        [FromQuery] bool incluirAnulados = false)
    {
        if (desde.HasValue &&
            hasta.HasValue &&
            hasta.Value < desde.Value)
        {
            return BadRequest(
                "La fecha final no puede ser anterior a la fecha inicial.");
        }

        var consulta =
            _context.Egresos
                .AsNoTracking()
                .AsQueryable();

        if (!incluirAnulados)
        {
            consulta =
                consulta.Where(x => !x.Anulado);
        }

        if (desde.HasValue)
        {
            consulta =
                consulta.Where(x =>
                    x.Fecha >= desde.Value);
        }

        if (hasta.HasValue)
        {
            consulta =
                consulta.Where(x =>
                    x.Fecha <= hasta.Value);
        }

        if (!string.IsNullOrWhiteSpace(categoria))
        {
            var categoriaFiltro =
                categoria.Trim();

            consulta =
                consulta.Where(x =>
                    x.Categoria == categoriaFiltro);
        }

        var egresos =
            await consulta
                .OrderByDescending(x => x.Fecha)
                .ThenByDescending(x => x.FechaRegistro)
                .Select(x => new EgresoDto
                {
                    Id = x.Id,
                    Fecha = x.Fecha,
                    Categoria = x.Categoria,
                    Concepto = x.Concepto,
                    Beneficiario = x.Beneficiario,
                    Monto = x.Monto,
                    MonedaId = x.MonedaId,
                    FormaPago = x.FormaPago,
                    NumeroComprobante =
                        x.NumeroComprobante,
                    Observacion =
                        x.Observacion,
                    FechaRegistro =
                        x.FechaRegistro,
                    Anulado =
                        x.Anulado,
                    FechaAnulacion =
                        x.FechaAnulacion,
                    MotivoAnulacion =
                        x.MotivoAnulacion
                })
                .ToListAsync();

        return Ok(egresos);
    }

    // ============================================================
    // GET: /api/egresos/{id}
    // ============================================================

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EgresoDto>> GetById(
        Guid id)
    {
        var egreso =
            await _context.Egresos
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new EgresoDto
                {
                    Id = x.Id,
                    Fecha = x.Fecha,
                    Categoria = x.Categoria,
                    Concepto = x.Concepto,
                    Beneficiario = x.Beneficiario,
                    Monto = x.Monto,
                    MonedaId = x.MonedaId,
                    FormaPago = x.FormaPago,
                    NumeroComprobante =
                        x.NumeroComprobante,
                    Observacion =
                        x.Observacion,
                    FechaRegistro =
                        x.FechaRegistro,
                    Anulado =
                        x.Anulado,
                    FechaAnulacion =
                        x.FechaAnulacion,
                    MotivoAnulacion =
                        x.MotivoAnulacion
                })
                .FirstOrDefaultAsync();

        if (egreso is null)
        {
            return NotFound(
                "No se encontró el egreso.");
        }

        return Ok(egreso);
    }

    // ============================================================
    // POST: /api/egresos
    // ============================================================

    [HttpPost]
    public async Task<ActionResult<EgresoDto>> Post(
        [FromBody] CrearEgresoRequest request)
    {
        if (request.MonedaId == Guid.Empty)
        {
            return BadRequest(
                "Debe seleccionar una moneda.");
        }

        var monedaExiste =
            await _context.Monedas
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == request.MonedaId);

        if (!monedaExiste)
        {
            return BadRequest(
                "La moneda seleccionada no existe.");
        }

        Egreso egreso;

        try
        {
            egreso =
                new Egreso(
                    request.Fecha,
                    request.Categoria,
                    request.Concepto,
                    request.Beneficiario,
                    request.Monto,
                    request.MonedaId,
                    request.FormaPago,
                    request.NumeroComprobante,
                    request.Observacion);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }

        _context.Egresos.Add(egreso);

        await _context.SaveChangesAsync();

        var respuesta =
            new EgresoDto
            {
                Id = egreso.Id,
                Fecha = egreso.Fecha,
                Categoria = egreso.Categoria,
                Concepto = egreso.Concepto,
                Beneficiario = egreso.Beneficiario,
                Monto = egreso.Monto,
                MonedaId = egreso.MonedaId,
                FormaPago = egreso.FormaPago,
                NumeroComprobante =
                    egreso.NumeroComprobante,
                Observacion =
                    egreso.Observacion,
                FechaRegistro =
                    egreso.FechaRegistro,
                Anulado =
                    egreso.Anulado,
                FechaAnulacion =
                    egreso.FechaAnulacion,
                MotivoAnulacion =
                    egreso.MotivoAnulacion
            };

        return CreatedAtAction(
            nameof(GetById),
            new { id = egreso.Id },
            respuesta);
    }

    // ============================================================
    // PUT: /api/egresos/{id}/anular
    // ============================================================

    [HttpPut("{id:guid}/anular")]
    public async Task<IActionResult> Anular(
        Guid id,
        [FromBody] AnularEgresoRequest request)
    {
        var egreso =
            await _context.Egresos
                .FirstOrDefaultAsync(x =>
                    x.Id == id);

        if (egreso is null)
        {
            return NotFound(
                "No se encontró el egreso.");
        }

        try
        {
            egreso.Anular(
                request.Motivo,
                DateTimeOffset.UtcNow);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // ============================================================
    // GET: /api/egresos/resumen
    // ============================================================

    [HttpGet("resumen")]
    public async Task<ActionResult<
        IReadOnlyList<ResumenEgresoDto>>> Resumen(
        [FromQuery] DateOnly? desde = null,
        [FromQuery] DateOnly? hasta = null)
    {
        if (desde.HasValue &&
            hasta.HasValue &&
            hasta.Value < desde.Value)
        {
            return BadRequest(
                "La fecha final no puede ser anterior a la fecha inicial.");
        }

        var consulta =
            _context.Egresos
                .AsNoTracking()
                .Where(x => !x.Anulado);

        if (desde.HasValue)
        {
            consulta =
                consulta.Where(x =>
                    x.Fecha >= desde.Value);
        }

        if (hasta.HasValue)
        {
            consulta =
                consulta.Where(x =>
                    x.Fecha <= hasta.Value);
        }

        var resumen =
            await consulta
                .GroupBy(x => new
                {
                    x.Categoria,
                    x.MonedaId
                })
                .Select(g => new ResumenEgresoDto
                {
                    Categoria =
                        g.Key.Categoria,

                    MonedaId =
                        g.Key.MonedaId,

                    Cantidad =
                        g.Count(),

                    Total =
                        g.Sum(x => x.Monto)
                })
                .OrderBy(x => x.Categoria)
                .ToListAsync();

        return Ok(resumen);
    }
}


// ============================================================
// DTO DE EGRESO
// ============================================================

public sealed class EgresoDto
{
    public Guid Id { get; set; }

    public DateOnly Fecha { get; set; }

    public string Categoria { get; set; } =
        string.Empty;

    public string Concepto { get; set; } =
        string.Empty;

    public string Beneficiario { get; set; } =
        string.Empty;

    public decimal Monto { get; set; }

    public Guid MonedaId { get; set; }

    public string FormaPago { get; set; } =
        string.Empty;

    public string? NumeroComprobante { get; set; }

    public string? Observacion { get; set; }

    public DateTimeOffset FechaRegistro { get; set; }

    public bool Anulado { get; set; }

    public DateTimeOffset? FechaAnulacion { get; set; }

    public string? MotivoAnulacion { get; set; }
}


// ============================================================
// REQUEST PARA REGISTRAR EGRESO
// ============================================================

public sealed class CrearEgresoRequest
{
    public DateOnly Fecha { get; set; }

    public string Categoria { get; set; } =
        string.Empty;

    public string Concepto { get; set; } =
        string.Empty;

    public string Beneficiario { get; set; } =
        string.Empty;

    public decimal Monto { get; set; }

    public Guid MonedaId { get; set; }

    public string FormaPago { get; set; } =
        string.Empty;

    public string? NumeroComprobante { get; set; }

    public string? Observacion { get; set; }
}


// ============================================================
// REQUEST PARA ANULAR
// ============================================================

public sealed class AnularEgresoRequest
{
    public string Motivo { get; set; } =
        string.Empty;
}


// ============================================================
// RESUMEN PARA REPORTES
// ============================================================

public sealed class ResumenEgresoDto
{
    public string Categoria { get; set; } =
        string.Empty;

    public Guid MonedaId { get; set; }

    public int Cantidad { get; set; }

    public decimal Total { get; set; }
}