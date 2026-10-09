using Mercadex.Domain.Caja;
using Mercadex.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mercadex.Api.Controllers;

[ApiController]
[Route("api/arqueo-diario")]
public sealed class ArqueoDiarioController : ControllerBase
{
    private readonly MercadexDbContext _db;

    public ArqueoDiarioController(MercadexDbContext db)
    {
        _db = db;
    }

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

        // Horario de Bolivia: UTC-04:00.
        // Inicio inclusivo y día siguiente exclusivo.
        var inicio = new DateTimeOffset(
            fecha.ToDateTime(TimeOnly.MinValue),
            TimeSpan.FromHours(-4));

        var fin = inicio.AddDays(1);

        var inicioUtc = inicio.ToUniversalTime();
        var finUtc = fin.ToUniversalTime();

        // Una fila por pago y recibo.
        // No se utilizan asignaciones de socios,
        // evitando duplicaciones por titularidad.
        var registros = await (
            from pago in _db.Pagos.AsNoTracking()

            join moneda in _db.Monedas.AsNoTracking()
                on pago.MonedaId equals moneda.Id

            join recibo in _db.Recibos.AsNoTracking()
                on pago.Id equals recibo.PagoId
                into grupoRecibos

            from recibo in grupoRecibos.DefaultIfEmpty()

            where pago.FechaRegistro >= inicioUtc
                && pago.FechaRegistro < finUtc

            select new
            {
                pago.Id,
                pago.FechaRegistro,
                pago.Monto,
                pago.FormaPago,
                pago.Estado,

                Moneda = moneda.Codigo,

                Simbolo = moneda.Simbolo,

                NumeroRecibo = recibo == null
                    ? (long?)null
                    : recibo.Numero,

                EstadoRecibo = recibo == null
                    ? (EstadoRecibo?)null
                    : recibo.Estado
            }
        ).ToListAsync(cancellationToken);

        // Solo cobros registrados con recibos emitidos.
        var validos = registros
            .Where(x =>
                x.Estado == EstadoPago.Registrado &&
                x.EstadoRecibo == EstadoRecibo.Emitido)
            .ToList();

        // Pagos registrados que requieren revisión.
        var incidencias = registros
            .Where(x =>
                x.Estado == EstadoPago.Registrado &&
                x.EstadoRecibo != EstadoRecibo.Emitido)
            .ToList();

        // Agrupar sin mezclar monedas distintas.
        var resumen = validos
            .GroupBy(x => new
            {
                x.Moneda,
                x.Simbolo
            })
            .Select(grupo => new
            {
                moneda = grupo.Key.Moneda,
                simbolo = grupo.Key.Simbolo,

                efectivo = grupo
                    .Where(x =>
                        x.FormaPago == FormaPago.Efectivo)
                    .Sum(x => x.Monto),

                qr = grupo
                    .Where(x =>
                        x.FormaPago == FormaPago.QR)
                    .Sum(x => x.Monto),

                otros = grupo
                    .Where(x =>
                        x.FormaPago != FormaPago.Efectivo &&
                        x.FormaPago != FormaPago.QR)
                    .Sum(x => x.Monto),

                total = grupo.Sum(x => x.Monto),

                cantidad = grupo.Count(),

                cantidadEfectivo = grupo.Count(x =>
                    x.FormaPago == FormaPago.Efectivo),

                cantidadQr = grupo.Count(x =>
                    x.FormaPago == FormaPago.QR),

                cantidadOtros = grupo.Count(x =>
                    x.FormaPago != FormaPago.Efectivo &&
                    x.FormaPago != FormaPago.QR)
            })
            .OrderBy(x => x.moneda)
            .ToList();

        var pendientes = incidencias
            .GroupBy(x => x.Moneda)
            .Select(grupo => new
            {
                moneda = grupo.Key,
                cantidad = grupo.Count(),
                monto = grupo.Sum(x => x.Monto)
            })
            .OrderBy(x => x.moneda)
            .ToList();

        return Ok(new
        {
            fecha = fecha.ToString("yyyy-MM-dd"),

            zonaHoraria = "Bolivia UTC-04:00",

            estado = "Preliminar",

            cantidadPagosValidos = validos.Count,

            cantidadIncidencias = incidencias.Count,

            resumen,

            pagosPorRevisar = pendientes,

            mensaje = incidencias.Count > 0
                ? "Existen pagos que requieren revisión antes del cierre."
                : "Consulta preliminar generada correctamente."
        });
    }
}