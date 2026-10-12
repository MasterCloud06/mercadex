using Mercadex.Domain.Caja;
using Mercadex.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mercadex.Api.Controllers;

[ApiController]
[Route("api/arqueo-diario")]
public sealed class ArqueoDiarioController : ControllerBase
{
    private static readonly TimeSpan ZonaBolivia = TimeSpan.FromHours(-4);

    private readonly MercadexDbContext _db;

    public ArqueoDiarioController(MercadexDbContext db)
    {
        _db = db;
    }

    // =============================================================
    // CONSULTA DEL DÍA (con estado de cierre)
    // =============================================================

    [HttpGet("preliminar")]
    public async Task<IActionResult> ObtenerPreliminar(
        [FromQuery] DateOnly fecha,
        CancellationToken cancellationToken)
    {
        if (fecha == default)
        {
            return BadRequest(new
            {
                mensaje = "Debe indicar una fecha válida."
            });
        }

        var calculo = await CalcularAsync(fecha, cancellationToken);

        var cierres = await _db.CierresCaja
            .AsNoTracking()
            .Where(c => c.Fecha == fecha)
            .ToListAsync(cancellationToken);

        var resumen = calculo.Resumen
            .Select(r =>
            {
                var cierre = cierres.FirstOrDefault(c => c.MonedaId == r.MonedaId);

                return new
                {
                    monedaId = r.MonedaId,
                    moneda = r.Moneda,
                    simbolo = r.Simbolo,
                    efectivo = r.Efectivo,
                    qr = r.Qr,
                    otros = r.Otros,
                    total = r.Total,
                    cantidad = r.Cantidad,
                    cantidadEfectivo = r.CantidadEfectivo,
                    cantidadQr = r.CantidadQr,
                    cantidadOtros = r.CantidadOtros,
                    cerrado = cierre != null,
                    cierre = cierre == null ? null : (object?)AResumenCierre(cierre, r.Moneda),
                    // Cobros o anulaciones posteriores al cierre.
                    cambiosPosteriores = cierre != null &&
                        (cierre.Total != r.Total || cierre.CantidadPagos != r.Cantidad)
                };
            })
            .ToList();

        // Monedas cerradas que hoy ya no tienen cobros válidos.
        foreach (var cierre in cierres.Where(c => resumen.All(r => r.monedaId != c.MonedaId)))
        {
            var codigo = await _db.Monedas
                .Where(m => m.Id == cierre.MonedaId)
                .Select(m => new { m.Codigo, m.Simbolo })
                .FirstAsync(cancellationToken);

            resumen.Add(new
            {
                monedaId = cierre.MonedaId,
                moneda = codigo.Codigo,
                simbolo = codigo.Simbolo,
                efectivo = 0m,
                qr = 0m,
                otros = 0m,
                total = 0m,
                cantidad = 0,
                cantidadEfectivo = 0,
                cantidadQr = 0,
                cantidadOtros = 0,
                cerrado = true,
                cierre = (object?)AResumenCierre(cierre, codigo.Codigo),
                cambiosPosteriores = true
            });
        }

        return Ok(new
        {
            fecha = fecha.ToString("yyyy-MM-dd"),
            zonaHoraria = "Bolivia UTC-04:00",
            estado = cierres.Count > 0 ? "Con cierre" : "Preliminar",
            cantidadPagosValidos = calculo.CantidadValidos,
            cantidadIncidencias = calculo.Incidencias.Sum(i => i.Cantidad),
            resumen,
            pagosPorRevisar = calculo.Incidencias.Select(i => new
            {
                moneda = i.Moneda,
                cantidad = i.Cantidad,
                monto = i.Monto
            }),
            mensaje = calculo.Incidencias.Count > 0
                ? "Existen pagos que requieren revisión antes del cierre."
                : "Consulta generada correctamente."
        });
    }

    // =============================================================
    // REGISTRAR CIERRE DEL DÍA
    // =============================================================

    public sealed record RegistrarCierreRequest(
        DateOnly Fecha,
        Guid MonedaId,
        decimal EfectivoContado,
        string Responsable,
        string? Observacion);

    [HttpPost("cierres")]
    public async Task<IActionResult> RegistrarCierre(
        [FromBody] RegistrarCierreRequest solicitud,
        CancellationToken cancellationToken)
    {
        if (solicitud is null || solicitud.Fecha == default || solicitud.MonedaId == Guid.Empty)
        {
            return BadRequest(new { mensaje = "Indique la fecha y la moneda del cierre." });
        }

        var hoy = DateOnly.FromDateTime(DateTimeOffset.UtcNow.ToOffset(ZonaBolivia).DateTime);

        if (solicitud.Fecha > hoy)
        {
            return BadRequest(new { mensaje = "No se puede cerrar una fecha futura." });
        }

        var yaCerrado = await _db.CierresCaja.AnyAsync(c =>
            c.Fecha == solicitud.Fecha &&
            c.MonedaId == solicitud.MonedaId, cancellationToken);

        if (yaCerrado)
        {
            return Conflict(new { mensaje = "La caja de esa fecha y moneda ya fue cerrada." });
        }

        // El servidor recalcula los totales: no se confía en el cliente.
        var calculo = await CalcularAsync(solicitud.Fecha, cancellationToken);

        var resumen = calculo.Resumen.FirstOrDefault(r => r.MonedaId == solicitud.MonedaId);

        var moneda = await _db.Monedas
            .AsNoTracking()
            .Where(m => m.Id == solicitud.MonedaId)
            .Select(m => new { m.Codigo })
            .FirstOrDefaultAsync(cancellationToken);

        if (moneda is null)
        {
            return BadRequest(new { mensaje = "La moneda indicada no existe." });
        }

        if (calculo.Incidencias.Any(i => i.Moneda == moneda.Codigo))
        {
            return Conflict(new
            {
                mensaje = "Hay pagos de esta moneda que requieren revisión. " +
                          "Resuélvalos antes de cerrar la caja."
            });
        }

        var cierre = new CierreCaja(
            solicitud.Fecha,
            solicitud.MonedaId,
            resumen?.Efectivo ?? 0,
            resumen?.Qr ?? 0,
            resumen?.Otros ?? 0,
            resumen?.CantidadEfectivo ?? 0,
            resumen?.CantidadQr ?? 0,
            resumen?.CantidadOtros ?? 0,
            solicitud.EfectivoContado,
            solicitud.Responsable,
            solicitud.Observacion,
            DateTimeOffset.UtcNow);

        _db.CierresCaja.Add(cierre);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Dos cajas cerrando al mismo tiempo: gana la primera.
            return Conflict(new { mensaje = "La caja de esa fecha y moneda ya fue cerrada." });
        }

        return Ok(AResumenCierre(cierre, moneda.Codigo));
    }

    // =============================================================
    // HISTORIAL DE CIERRES
    // =============================================================

    [HttpGet("cierres")]
    public async Task<IActionResult> ListarCierres(
        [FromQuery] DateOnly? desde,
        [FromQuery] DateOnly? hasta,
        CancellationToken cancellationToken)
    {
        var hoy = DateOnly.FromDateTime(DateTimeOffset.UtcNow.ToOffset(ZonaBolivia).DateTime);
        var fin = hasta ?? hoy;
        var inicio = desde ?? fin.AddDays(-30);

        if (inicio > fin)
        {
            return BadRequest(new { mensaje = "La fecha inicial no puede superar la final." });
        }

        var monedas = await _db.Monedas
            .AsNoTracking()
            .ToDictionaryAsync(m => m.Id, m => m.Codigo, cancellationToken);

        var cierres = await _db.CierresCaja
            .AsNoTracking()
            .Where(c => c.Fecha >= inicio && c.Fecha <= fin)
            .OrderByDescending(c => c.Fecha)
            .ThenBy(c => c.MonedaId)
            .ToListAsync(cancellationToken);

        return Ok(cierres.Select(c => AResumenCierre(c, monedas.GetValueOrDefault(c.MonedaId, "?"))));
    }

    // =============================================================
    // CÁLCULO COMÚN
    // =============================================================

    private static object AResumenCierre(CierreCaja c, string moneda) => new
    {
        id = c.Id,
        fecha = c.Fecha.ToString("yyyy-MM-dd"),
        monedaId = c.MonedaId,
        moneda,
        efectivo = c.Efectivo,
        qr = c.Qr,
        otros = c.Otros,
        total = c.Total,
        cantidadPagos = c.CantidadPagos,
        cantidadEfectivo = c.CantidadEfectivo,
        cantidadQr = c.CantidadQr,
        cantidadOtros = c.CantidadOtros,
        efectivoContado = c.EfectivoContado,
        diferencia = c.Diferencia,
        responsable = c.Responsable,
        observacion = c.Observacion,
        fechaRegistro = c.FechaRegistroUtc
    };

    private sealed record ResumenMoneda(
        Guid MonedaId,
        string Moneda,
        string Simbolo,
        decimal Efectivo,
        decimal Qr,
        decimal Otros,
        decimal Total,
        int Cantidad,
        int CantidadEfectivo,
        int CantidadQr,
        int CantidadOtros);

    private sealed record Incidencia(string Moneda, int Cantidad, decimal Monto);

    private sealed record Calculo(
        List<ResumenMoneda> Resumen,
        List<Incidencia> Incidencias,
        int CantidadValidos);

    private async Task<Calculo> CalcularAsync(DateOnly fecha, CancellationToken ct)
    {
        // Horario de Bolivia: inicio inclusivo, día siguiente exclusivo.
        var inicio = new DateTimeOffset(fecha.ToDateTime(TimeOnly.MinValue), ZonaBolivia);
        var inicioUtc = inicio.ToUniversalTime();
        var finUtc = inicio.AddDays(1).ToUniversalTime();

        // Una fila por pago y recibo (sin asignaciones de socios,
        // para no duplicar por titularidad).
        var registros = await (
            from pago in _db.Pagos.AsNoTracking()
            join moneda in _db.Monedas.AsNoTracking() on pago.MonedaId equals moneda.Id
            join recibo in _db.Recibos.AsNoTracking() on pago.Id equals recibo.PagoId into grupoRecibos
            from recibo in grupoRecibos.DefaultIfEmpty()
            where pago.FechaRegistro >= inicioUtc && pago.FechaRegistro < finUtc
            select new
            {
                pago.MonedaId,
                pago.Monto,
                pago.FormaPago,
                pago.Estado,
                Moneda = moneda.Codigo,
                moneda.Simbolo,
                EstadoRecibo = recibo == null ? (EstadoRecibo?)null : recibo.Estado
            }).ToListAsync(ct);

        var validos = registros
            .Where(x => x.Estado == EstadoPago.Registrado && x.EstadoRecibo == EstadoRecibo.Emitido)
            .ToList();

        var incidencias = registros
            .Where(x => x.Estado == EstadoPago.Registrado && x.EstadoRecibo != EstadoRecibo.Emitido)
            .GroupBy(x => x.Moneda)
            .Select(g => new Incidencia(g.Key, g.Count(), g.Sum(x => x.Monto)))
            .OrderBy(x => x.Moneda)
            .ToList();

        // "Otros" incluye Transferencia (histórico) y Otro.
        var resumen = validos
            .GroupBy(x => new { x.MonedaId, x.Moneda, x.Simbolo })
            .Select(g => new ResumenMoneda(
                g.Key.MonedaId,
                g.Key.Moneda,
                g.Key.Simbolo,
                g.Where(x => x.FormaPago == FormaPago.Efectivo).Sum(x => x.Monto),
                g.Where(x => x.FormaPago == FormaPago.QR).Sum(x => x.Monto),
                g.Where(x => x.FormaPago != FormaPago.Efectivo && x.FormaPago != FormaPago.QR).Sum(x => x.Monto),
                g.Sum(x => x.Monto),
                g.Count(),
                g.Count(x => x.FormaPago == FormaPago.Efectivo),
                g.Count(x => x.FormaPago == FormaPago.QR),
                g.Count(x => x.FormaPago != FormaPago.Efectivo && x.FormaPago != FormaPago.QR)))
            .OrderBy(x => x.Moneda == "BOB" ? 0 : 1)
            .ThenBy(x => x.Moneda)
            .ToList();

        return new Calculo(resumen, incidencias, validos.Count);
    }
}
