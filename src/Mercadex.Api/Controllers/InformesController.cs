using Mercadex.Domain.Caja;
using Mercadex.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mercadex.Api.Controllers;

/// <summary>
/// Informes financieros consolidados (solo lectura).
/// </summary>
[ApiController]
[Route("api/informes")]
public sealed class InformesController : ControllerBase
{
    private static readonly TimeSpan ZonaBolivia = TimeSpan.FromHours(-4);

    private readonly MercadexDbContext _db;

    public InformesController(MercadexDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Informe de Ingresos y Gastos de un período, por moneda:
    /// ingresos por concepto (cobros válidos), egresos por categoría
    /// (no anulados) y saldo. No mezcla monedas.
    /// </summary>
    [HttpGet("ingresos-gastos")]
    public async Task<IActionResult> IngresosGastos(
        [FromQuery] DateOnly? desde,
        [FromQuery] DateOnly? hasta,
        CancellationToken ct)
    {
        if (!desde.HasValue || !hasta.HasValue)
            return BadRequest(new { mensaje = "Indique desde y hasta (AAAA-MM-DD)." });

        if (desde.Value > hasta.Value)
            return BadRequest(new { mensaje = "La fecha inicial no puede superar la final." });

        if (hasta.Value.DayNumber - desde.Value.DayNumber > 731)
            return BadRequest(new { mensaje = "El período no puede superar 2 años." });

        var inicioUtc = new DateTimeOffset(desde.Value.ToDateTime(TimeOnly.MinValue), ZonaBolivia).ToUniversalTime();
        var finUtc = new DateTimeOffset(hasta.Value.AddDays(1).ToDateTime(TimeOnly.MinValue), ZonaBolivia).ToUniversalTime();

        var monedas = await _db.Monedas.AsNoTracking()
            .ToDictionaryAsync(m => m.Id, m => m.Codigo, ct);

        // Ingresos: detalle de recibos emitidos con pago registrado.
        var ingresos = await (
                from d in _db.DetallesRecibo.AsNoTracking()
                join r in _db.Recibos.AsNoTracking() on EF.Property<Guid>(d, "ReciboId") equals r.Id
                join p in _db.Pagos.AsNoTracking() on r.PagoId equals p.Id
                where r.Estado == EstadoRecibo.Emitido
                      && p.Estado == EstadoPago.Registrado
                      && r.FechaEmision >= inicioUtc
                      && r.FechaEmision < finUtc
                select new { p.MonedaId, d.Concepto, d.Monto, ReciboId = r.Id })
            .ToListAsync(ct);

        var recibosPorMoneda = ingresos
            .GroupBy(x => x.MonedaId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.ReciboId).Distinct().Count());

        // Egresos no anulados del período.
        var egresos = await _db.Egresos.AsNoTracking()
            .Where(e => !e.Anulado && e.Fecha >= desde.Value && e.Fecha <= hasta.Value)
            .Select(e => new { e.MonedaId, e.Categoria, e.Monto })
            .ToListAsync(ct);

        var monedasUsadas = ingresos.Select(x => x.MonedaId)
            .Concat(egresos.Select(x => x.MonedaId))
            .Distinct()
            .ToList();

        var resultado = monedasUsadas
            .Select(mid =>
            {
                var ing = ingresos.Where(x => x.MonedaId == mid)
                    .GroupBy(x => x.Concepto)
                    .Select(g => new { concepto = g.Key, cantidad = g.Count(), total = g.Sum(x => x.Monto) })
                    .OrderByDescending(x => x.total)
                    .ToList();

                var egr = egresos.Where(x => x.MonedaId == mid)
                    .GroupBy(x => x.Categoria)
                    .Select(g => new { categoria = g.Key, cantidad = g.Count(), total = g.Sum(x => x.Monto) })
                    .OrderByDescending(x => x.total)
                    .ToList();

                var totalIngresos = ing.Sum(x => x.total);
                var totalEgresos = egr.Sum(x => x.total);

                return new
                {
                    monedaId = mid,
                    moneda = monedas.GetValueOrDefault(mid, "?"),
                    recibos = recibosPorMoneda.GetValueOrDefault(mid),
                    ingresos = ing,
                    totalIngresos,
                    egresos = egr,
                    totalEgresos,
                    saldo = totalIngresos - totalEgresos
                };
            })
            .OrderBy(x => x.moneda == "BOB" ? 0 : 1)
            .ThenBy(x => x.moneda)
            .ToList();

        Response.Headers["Cache-Control"] = "no-store";

        return Ok(new
        {
            desde = desde.Value,
            hasta = hasta.Value,
            generado = DateTimeOffset.UtcNow.ToOffset(ZonaBolivia),
            monedas = resultado
        });
    }
}
