using Mercadex.Api.Seguridad;
using System.Data;
using System.Security.Claims;
using Mercadex.Domain.Cobranza;
using Mercadex.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mercadex.Api.Controllers;

[ApiController]
[Route("api/obligaciones-correcciones")]
public sealed class CorreccionesObligacionesController : ControllerBase
{
    private readonly MercadexDbContext _db;

    public CorreccionesObligacionesController(
        MercadexDbContext db)
    {
        _db = db;
    }

    // MERCADEX_AUDITORIA_DEUDAS_V1

    // Modulo de Mercadex 1.5 (sin autorizacion de administrador).
    // Ya no depende de variables de entorno ni del nombre de la base.
    private static bool EntornoPermitido() => true;

    // MERCADEX_LISTADO_DEUDAS_V1
    // Una sola consulta para obligaciones, casetas,
    // monedas, historial y recibos asociados.

    [HttpGet("listado")]
    public async Task<IActionResult> Listado(
        [FromQuery] Guid conceptoCobroId,
        [FromQuery] int anio,
        [FromQuery] int mes,
        CancellationToken cancellationToken)
    {
        if (!EntornoPermitido())
        {
            return StatusCode(403, new
            {
                mensaje = "Modulo no disponible."
            });
        }

        if (conceptoCobroId == Guid.Empty ||
            anio < 2000 || anio > 2100 ||
            mes < 1 || mes > 12)
        {
            return BadRequest(new
            {
                mensaje = "Selecciona un concepto y periodo validos."
            });
        }

        var datos = await (
            from obligacion in _db.Obligaciones.AsNoTracking()

            join unidad in _db.UnidadesComerciales.AsNoTracking()
                on obligacion.UnidadComercialId equals unidad.Id

            join moneda in _db.Monedas.AsNoTracking()
                on obligacion.MonedaId equals moneda.Id

            where obligacion.ConceptoCobroId == conceptoCobroId
               && obligacion.PeriodoAnio == anio
               && obligacion.PeriodoMes == mes
               && unidad.Tipo ==
                    Mercadex.Domain.Organizacion.TipoUnidadComercial.Caseta

            orderby unidad.Codigo

            select new
            {
                obligacion.Id,
                obligacion.UnidadComercialId,
                obligacion.ConceptoCobroId,
                obligacion.MonedaId,
                obligacion.Monto,
                obligacion.Estado,
                obligacion.FechaEmision,
                obligacion.FechaVencimiento,
                obligacion.PeriodoAnio,
                obligacion.PeriodoMes,

                Caseta = unidad.Codigo,
                Bloque = unidad.Bloque,
                Moneda = moneda.Codigo,

                Modificado = _db.CambiosMontoObligacion
                    .Any(c =>
                        c.ObligacionId == obligacion.Id),

                TieneRecibo = _db.DetallesRecibo
                    .Any(d =>
                        d.ObligacionId == obligacion.Id)
            }
        ).ToListAsync(cancellationToken);

        var resultado = datos.Select(x => new
        {
            x.Id,
            x.UnidadComercialId,
            x.ConceptoCobroId,
            x.MonedaId,
            x.Caseta,
            x.Bloque,
            x.Moneda,
            x.Monto,

            Estado = x.Estado.ToString(),

            x.FechaEmision,
            x.FechaVencimiento,
            x.PeriodoAnio,
            x.PeriodoMes,
            x.Modificado,
            x.TieneRecibo,

            PuedeModificar =
                x.Estado ==
                    EstadoObligacion.Pendiente &&
                !x.TieneRecibo
        }).ToList();

        return Ok(new
        {
            conceptoCobroId,
            anio,
            mes,

            cantidad = resultado.Count,

            modificadas = resultado.Count(x =>
                x.Modificado),

            pendientes = resultado.Count(x =>
                x.Estado == "Pendiente"),

            totalPorMoneda = resultado
                .GroupBy(x => x.Moneda)
                .Select(g => new
                {
                    moneda = g.Key,
                    cantidad = g.Count(),
                    total = g.Sum(x => x.Monto)
                })
                .ToList(),

            obligaciones = resultado
        });
    }
    [HttpGet("{id:guid}/historial")]
    public async Task<IActionResult> Historial(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!EntornoPermitido())
        {
            return StatusCode(403, new
            {
                mensaje = "Modulo no disponible."
            });
        }

        var obligacion = await _db.Obligaciones
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.UnidadComercialId,
                x.ConceptoCobroId,
                x.MonedaId,
                x.Monto,
                x.Estado,
                x.PeriodoAnio,
                x.PeriodoMes
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (obligacion is null)
        {
            return NotFound(new
            {
                mensaje = "Obligacion no encontrada."
            });
        }

        var cambios = await _db.CambiosMontoObligacion
            .AsNoTracking()
            .Where(x => x.ObligacionId == id)
            .OrderBy(x => x.FechaUtc)
            .ThenBy(x => x.Id)
            .Select(x => new
            {
                x.Id,
                x.MontoAnterior,
                x.MontoNuevo,
                x.Motivo,
                x.Responsable,
                x.FechaUtc
            })
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            obligacion,
            modificado = cambios.Count > 0,
            cantidadCambios = cambios.Count,
            historial = cambios
        });
    }

    [HttpPut("{id:guid}/monto")]
    public async Task<IActionResult> CorregirMonto(
        Guid id,
        [FromBody] CorregirDeudaRequest? solicitud,
        CancellationToken cancellationToken)
    {
        if (!EntornoPermitido())
        {
            return StatusCode(403, new
            {
                mensaje = "Modulo no disponible."
            });
        }

        if (id == Guid.Empty ||
            solicitud is null ||
            solicitud.MontoAnteriorEsperado <= 0 ||
            solicitud.MontoNuevo <= 0 ||
            decimal.Round(solicitud.MontoNuevo, 2) !=
                solicitud.MontoNuevo ||
            decimal.Round(solicitud.MontoAnteriorEsperado, 2) !=
                solicitud.MontoAnteriorEsperado ||
            solicitud.MontoNuevo == solicitud.MontoAnteriorEsperado ||
            string.IsNullOrWhiteSpace(solicitud.Motivo) ||
            solicitud.Motivo.Trim().Length < 5 ||
            solicitud.Motivo.Trim().Length > 500)
        {
            return BadRequest(new
            {
                mensaje =
                    "Indica un monto nuevo valido, " +
                    "el monto anterior esperado y " +
                    "un motivo de 5 a 500 caracteres."
            });
        }

        // El responsable procede de la identidad autenticada,
        // nunca del cuerpo enviado por el cliente.

        var responsable =
            User.Identity?.Name ??
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
            User.FindFirst("sub")?.Value ??
            "Sistema";

        if (string.IsNullOrWhiteSpace(responsable) ||
            responsable.Length > 120)
        {
            return StatusCode(403, new
            {
                mensaje =
                    "No se pudo identificar al responsable " +
                    "autenticado de la correccion."
            });
        }

        await using var transaccion =
            await _db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

        // Bloqueo de fila: evita que dos correcciones
        // se apliquen simultaneamente a la misma obligacion.

        var encontrados = await _db.Obligaciones
            .FromSqlInterpolated(
                $"SELECT * FROM public.obligaciones WHERE \"Id\" = {id} FOR UPDATE")
            .ToListAsync(cancellationToken);

        var obligacion = encontrados.SingleOrDefault();

        if (obligacion is null)
        {
            return NotFound(new
            {
                mensaje = "Obligacion no encontrada."
            });
        }

        if (obligacion.Estado != EstadoObligacion.Pendiente)
        {
            return Conflict(new
            {
                mensaje =
                    "No se permite corregir una obligacion " +
                    "pagada o anulada."
            });
        }

        // Incluso si un recibo fue anulado, se evita modificar
        // una obligacion que tenga historial de cobro.
        // Este control es intencionadamente conservador.

        var tieneRecibo = await _db.DetallesRecibo
            .AsNoTracking()
            .AnyAsync(x => x.ObligacionId == id,
                cancellationToken);

        if (tieneRecibo)
        {
            return Conflict(new
            {
                mensaje =
                    "Esta obligacion tiene un recibo asociado. " +
                    "No se puede modificar su importe."
            });
        }

        if (obligacion.Monto != solicitud.MontoAnteriorEsperado)
        {
            return Conflict(new
            {
                mensaje =
                    "El importe fue modificado desde " +
                    "la ultima consulta. Actualiza la deuda.",
                montoActual = obligacion.Monto
            });
        }

        var anterior = obligacion.Monto;

        obligacion.CorregirMonto(solicitud.MontoNuevo);

        var cambio = new CambioMontoObligacion(
            obligacion.Id,
            anterior,
            solicitud.MontoNuevo,
            solicitud.Motivo,
            responsable,
            DateTime.UtcNow);

        _db.CambiosMontoObligacion.Add(cambio);

        // La correccion y su historial se guardan juntos.

        await _db.SaveChangesAsync(cancellationToken);

        await transaccion.CommitAsync(cancellationToken);

        var datos = await (
                from u in _db.UnidadesComerciales.AsNoTracking()
                where u.Id == obligacion.UnidadComercialId
                select u.Codigo)
            .FirstOrDefaultAsync(cancellationToken);
        var concepto = await _db.ConceptosCobro.AsNoTracking()
            .Where(c => c.Id == obligacion.ConceptoCobroId).Select(c => c.Nombre)
            .FirstOrDefaultAsync(cancellationToken);
        var moneda = await _db.Monedas.AsNoTracking()
            .Where(m => m.Id == obligacion.MonedaId).Select(m => m.Codigo)
            .FirstOrDefaultAsync(cancellationToken);

        HttpContext.Detalle(
            $"Caseta {datos} · {concepto}" +
            (obligacion.PeriodoMes is { } mes && obligacion.PeriodoAnio is { } anio ? $" {mes:00}/{anio}" : "") +
            $" · de {Mercadex.Api.Seguridad.Cambios.Monto(anterior, moneda ?? "")} a " +
            $"{Mercadex.Api.Seguridad.Cambios.Monto(obligacion.Monto, moneda ?? "")} · Motivo: {cambio.Motivo}");

        return Ok(new
        {
            obligacionId = obligacion.Id,
            monedaId = obligacion.MonedaId,
            montoAnterior = anterior,
            montoNuevo = obligacion.Monto,
            modificado = true,
            historialId = cambio.Id,
            motivo = cambio.Motivo,
            responsable = cambio.Responsable,
            fechaUtc = cambio.FechaUtc
        });
    }
}

public sealed record CorregirDeudaRequest(
    decimal MontoAnteriorEsperado,
    decimal MontoNuevo,
    string Motivo);