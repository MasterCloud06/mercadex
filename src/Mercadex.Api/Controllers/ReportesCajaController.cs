using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mercadex.Domain.Caja;
using Mercadex.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mercadex.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrador")]
[Route("api/reportes-caja")]
public sealed class ReportesCajaController : ControllerBase
{
    // MERCADEX_REPORTES_CAJA_LOCAL_V1

    private readonly MercadexDbContext _db;

    private static readonly TimeSpan ZonaBolivia =
        TimeSpan.FromHours(-4);

    public ReportesCajaController(MercadexDbContext db)
    {
        _db = db;
    }

    private bool EntornoPermitido()
    {
        return Environment.GetEnvironmentVariable(
                "MERCADEX_ENABLE_V15") == "SI" &&
               string.Equals(
                   _db.Database.GetDbConnection().Database,
                   "mercadex_db_4qha",
                   StringComparison.OrdinalIgnoreCase);
    }

    [HttpGet]
    public async Task<IActionResult> Consultar(
        [FromQuery] DateOnly? desde,
        [FromQuery] DateOnly? hasta,
        CancellationToken ct)
    {
        if (!EntornoPermitido())
            return StatusCode(403);

        if (!desde.HasValue || !hasta.HasValue)
        {
            return BadRequest(new
            {
                mensaje =
                    "Debe indicar desde y hasta con formato AAAA-MM-DD."
            });
        }

        if (desde.Value > hasta.Value)
        {
            return BadRequest(new
            {
                mensaje =
                    "La fecha inicial no puede superar la final."
            });
        }

        if (
            hasta.Value.DayNumber -
            desde.Value.DayNumber > 366 ||
            hasta.Value == DateOnly.MaxValue
        )
        {
            return BadRequest(new
            {
                mensaje =
                    "El rango debe ser de 367 dias o menos."
            });
        }

        // Las fechas introducidas corresponden a Bolivia.
        // Los limites son inclusivos para el usuario;
        // internamente se usa [inicio, fin exclusivo).

        var inicio = new DateTimeOffset(
            desde.Value.ToDateTime(TimeOnly.MinValue),
            ZonaBolivia).ToUniversalTime();

        var fin = new DateTimeOffset(
            hasta.Value.AddDays(1)
                .ToDateTime(TimeOnly.MinValue),
            ZonaBolivia).ToUniversalTime();

        // Incluir recibos emitidos dentro del periodo
        // y anulaciones realizadas dentro del periodo.
        // Consultar no modifica ningun dato.

        var recibos = await _db.Recibos
            .AsNoTracking()
            .Include(x => x.Detalles)
            .Where(x =>
                (
                    x.FechaEmision >= inicio &&
                    x.FechaEmision < fin
                ) ||
                (
                    x.Estado == EstadoRecibo.Anulado &&
                    x.FechaAnulacion.HasValue &&
                    x.FechaAnulacion.Value >= inicio &&
                    x.FechaAnulacion.Value < fin
                ))
            .OrderBy(x => x.FechaEmision)
            .Take(5001)
            .ToListAsync(ct);

        if (recibos.Count > 5000)
        {
            return BadRequest(new
            {
                mensaje =
                    "Demasiados recibos para una sola consulta. " +
                    "Seleccione un rango menor."
            });
        }

        var pagoIds = recibos
            .Select(x => x.PagoId)
            .Distinct()
            .ToList();

        var pagos = await _db.Pagos
            .AsNoTracking()
            .Where(x => pagoIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);

        var advertencias = new List<string>();

        var movimientos = new List<MovimientoReporte>();

        // Solo ingresos que actualmente permanecen validos.
        // Un recibo anulado nunca se suma como ingreso.

        foreach (var recibo in recibos)
        {
            if (
                recibo.FechaEmision < inicio ||
                recibo.FechaEmision >= fin ||
                recibo.Estado != EstadoRecibo.Emitido
            )
            {
                continue;
            }

            if (!pagos.TryGetValue(recibo.PagoId, out var pago))
            {
                advertencias.Add(
                    $"Recibo {recibo.Numero}: no tiene pago asociado.");

                continue;
            }

            if (pago.Estado != EstadoPago.Registrado)
            {
                advertencias.Add(
                    $"Recibo {recibo.Numero}: pago no registrado.");

                continue;
            }

            if (
                recibo.Detalles.Count == 0 ||
                recibo.Total != pago.Monto
            )
            {
                advertencias.Add(
                    $"Recibo {recibo.Numero}: " +
                    "el importe no coincide con el pago.");

                continue;
            }

            movimientos.Add(new MovimientoReporte(
                recibo.Id,
                recibo.Numero,
                recibo.UnidadComercialId,
                pago.MonedaId,
                recibo.Total,
                pago.FormaPago.ToString(),
                pago.NombrePagador,
                pago.Referencia,
                recibo.FechaEmision,
                recibo.Detalles
                    .Select(d => new ConceptoDetalle(
                        d.ConceptoCobroId,
                        d.Concepto,
                        d.Monto))
                    .ToList()
            ));
        }

        // Los totales se calculan POR MONEDA.
        // No se suman BOB, USD u otras monedas entre si.

        var resumen = movimientos
            .GroupBy(x => x.MonedaId)
            .Select(g => new
            {
                monedaId = g.Key,
                cantidadRecibos = g.Count(),
                total = g.Sum(x => x.Monto)
            })
            .OrderBy(x => x.monedaId)
            .ToList();

        var formasPago = movimientos
            .GroupBy(x => new
            {
                x.MonedaId,
                x.FormaPago
            })
            .Select(g => new
            {
                monedaId = g.Key.MonedaId,
                formaPago = g.Key.FormaPago,
                cantidad = g.Count(),
                total = g.Sum(x => x.Monto)
            })
            .OrderBy(x => x.formaPago)
            .ToList();

        var conceptos = movimientos
            .SelectMany(m =>
                m.Conceptos.Select(d => new
                {
                    m.MonedaId,
                    d.ConceptoCobroId,
                    d.Concepto,
                    d.Monto
                }))
            .GroupBy(x => new
            {
                x.MonedaId,
                x.ConceptoCobroId,
                x.Concepto
            })
            .Select(g => new
            {
                monedaId = g.Key.MonedaId,
                conceptoCobroId = g.Key.ConceptoCobroId,
                concepto = g.Key.Concepto,
                cantidad = g.Count(),
                total = g.Sum(x => x.Monto)
            })
            .OrderBy(x => x.concepto)
            .ToList();

        var diasBase = movimientos
            .GroupBy(x => new
            {
                x.MonedaId,
                Dia = x.FechaEmisionUtc
                    .ToOffset(ZonaBolivia)
                    .ToString("yyyy-MM-dd")
            })
            .Select(g => new DiaReporte(
                g.Key.Dia,
                g.Key.MonedaId,
                g.Count(),
                g.Sum(x => x.Monto),
                0m
            ))
            .ToList();

        var dias = new List<DiaReporte>();

        foreach (
            var moneda in diasBase.GroupBy(x => x.MonedaId)
        )
        {
            decimal acumulado = 0m;

            foreach (
                var dia in moneda.OrderBy(x => x.Fecha)
            )
            {
                acumulado += dia.Total;

                dias.Add(dia with
                {
                    Acumulado = acumulado
                });
            }
        }

        dias = dias
            .OrderBy(x => x.Fecha)
            .ThenBy(x => x.MonedaId)
            .ToList();

        // Anulaciones: informacion de auditoria.
        // No se restan otra vez de los ingresos validos.

        var anulaciones = recibos
            .Where(x =>
                x.Estado == EstadoRecibo.Anulado &&
                x.FechaAnulacion.HasValue &&
                x.FechaAnulacion.Value >= inicio &&
                x.FechaAnulacion.Value < fin)
            .Select(x => new
            {
                reciboId = x.Id,
                numeroRecibo = x.Numero,
                fechaAnulacionUtc = x.FechaAnulacion,
                montoExcluido = x.Total,
                monedaId = pagos.TryGetValue(
                    x.PagoId, out var pago)
                    ? (Guid?)pago.MonedaId
                    : null,
                motivo = x.MotivoAnulacion,
                responsable = x.AnuladoPor
            })
            .OrderByDescending(x => x.fechaAnulacionUtc)
            .ToList();

        var cobros = movimientos
            .OrderBy(x => x.FechaEmisionUtc)
            .ThenBy(x => x.NumeroRecibo)
            .Select(x => new
            {
                reciboId = x.ReciboId,
                numeroRecibo = x.NumeroRecibo,
                unidadComercialId = x.UnidadComercialId,
                monedaId = x.MonedaId,
                monto = x.Monto,
                formaPago = x.FormaPago,
                pagador = x.Pagador,
                referencia = x.Referencia,
                fechaEmisionUtc = x.FechaEmisionUtc,
                fechaBolivia = x.FechaEmisionUtc
                    .ToOffset(ZonaBolivia)
                    .ToString("dd/MM/yyyy HH:mm"),
                conceptos = x.Conceptos
            })
            .ToList();

        Response.Headers["Cache-Control"] = "no-store";

        return Ok(new
        {
            desde = desde.Value.ToString("yyyy-MM-dd"),
            hasta = hasta.Value.ToString("yyyy-MM-dd"),
            zonaHoraria = "Bolivia UTC-04:00",
            generadoUtc = DateTimeOffset.UtcNow,
            tipo = "Consulta de caja - no es un cierre definitivo",

            cantidadRecibosValidos = movimientos.Count,
            cantidadAnulaciones = anulaciones.Count,

            resumen,
            dias,
            formasPago,
            conceptos,
            cobros,
            anulaciones,
            advertencias
        });
    }

    private sealed record ConceptoDetalle(
        Guid ConceptoCobroId,
        string Concepto,
        decimal Monto);

    private sealed record MovimientoReporte(
        Guid ReciboId,
        long NumeroRecibo,
        Guid UnidadComercialId,
        Guid MonedaId,
        decimal Monto,
        string FormaPago,
        string Pagador,
        string? Referencia,
        DateTimeOffset FechaEmisionUtc,
        IReadOnlyList<ConceptoDetalle> Conceptos);

    private sealed record DiaReporte(
        string Fecha,
        Guid MonedaId,
        int CantidadRecibos,
        decimal Total,
        decimal Acumulado);
}