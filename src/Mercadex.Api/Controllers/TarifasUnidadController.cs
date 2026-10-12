using Mercadex.Domain.Cobranza;
using Mercadex.Domain.Organizacion;
using Mercadex.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mercadex.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrador")]
[Route("api/tarifas-unidad")]
public sealed class TarifasUnidadController : ControllerBase
{
    private readonly MercadexDbContext _db;

    public TarifasUnidadController(MercadexDbContext db)
    {
        _db = db;
    }

    // ==================================================
    // CONSULTAR TARIFAS DE LAS CASETAS
    // ==================================================

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] Guid conceptoCobroId,
        CancellationToken cancellationToken)
    {
        if (conceptoCobroId == Guid.Empty)
        {
            return BadRequest(new
            {
                mensaje = "Selecciona un concepto."
            });
        }

        var conceptoExiste = await _db.ConceptosCobro
            .AsNoTracking()
            .AnyAsync(x =>
                x.Id == conceptoCobroId &&
                x.Activo,
                cancellationToken);

        if (!conceptoExiste)
        {
            return NotFound(new
            {
                mensaje = "El concepto no existe o esta inactivo."
            });
        }

        var casetas = await _db.UnidadesComerciales
            .AsNoTracking()
            .Where(x =>
                x.Activa &&
                x.Tipo == TipoUnidadComercial.Caseta)
            .Select(x => new
            {
                x.Id,
                x.Codigo,
                x.Bloque
            })
            .ToListAsync(cancellationToken);

        var tarifas = await _db.TarifasUnidad
            .AsNoTracking()
            .Where(x =>
                x.ConceptoCobroId == conceptoCobroId)
            .ToListAsync(cancellationToken);

        var porCaseta = tarifas.ToDictionary(
            x => x.UnidadComercialId);

        var resultado = casetas.Select(c =>
        {
            porCaseta.TryGetValue(c.Id, out var tarifa);

            return new
            {
                UnidadComercialId = c.Id,
                Caseta = c.Codigo,
                c.Bloque,

                TieneTarifa = tarifa is not null,

                MonedaId = tarifa?.MonedaId,
                Monto = tarifa?.Monto,

                FechaActualizacionUtc =
                    tarifa?.FechaActualizacionUtc
            };
        })
        .OrderBy(x => x.Caseta)
        .ToList();

        return Ok(new
        {
            conceptoCobroId,
            cantidadCasetas = resultado.Count,
            cantidadConTarifa =
                resultado.Count(x => x.TieneTarifa),
            casetas = resultado
        });
    }

    // ==================================================
    // GUARDAR O ACTUALIZAR TARIFA INDIVIDUAL
    // ==================================================

    [HttpPut("{unidadId:guid}/{conceptoCobroId:guid}")]
    public async Task<IActionResult> Guardar(
        Guid unidadId,
        Guid conceptoCobroId,
        [FromBody] GuardarTarifaUnidadRequest? solicitud,
        CancellationToken cancellationToken)
    {
        if (unidadId == Guid.Empty ||
            conceptoCobroId == Guid.Empty ||
            solicitud is null ||
            solicitud.MonedaId == Guid.Empty ||
            solicitud.Monto <= 0 ||
            decimal.Round(solicitud.Monto, 2) != solicitud.Monto)
        {
            return BadRequest(new
            {
                mensaje =
                    "Datos invalidos. El monto debe ser positivo " +
                    "y tener como maximo dos decimales."
            });
        }

        var casetaExiste = await _db.UnidadesComerciales
            .AsNoTracking()
            .AnyAsync(x =>
                x.Id == unidadId &&
                x.Activa &&
                x.Tipo == TipoUnidadComercial.Caseta,
                cancellationToken);

        if (!casetaExiste)
        {
            return NotFound(new
            {
                mensaje = "Caseta no encontrada o inactiva."
            });
        }

        var conceptoExiste = await _db.ConceptosCobro
            .AsNoTracking()
            .AnyAsync(x =>
                x.Id == conceptoCobroId &&
                x.Activo,
                cancellationToken);

        if (!conceptoExiste)
        {
            return NotFound(new
            {
                mensaje = "Concepto no encontrado o inactivo."
            });
        }

        var monedaExiste = await _db.Monedas
            .AsNoTracking()
            .AnyAsync(x =>
                x.Id == solicitud.MonedaId &&
                x.Activa,
                cancellationToken);

        if (!monedaExiste)
        {
            return NotFound(new
            {
                mensaje = "Moneda no encontrada o inactiva."
            });
        }

        var tarifa = await _db.TarifasUnidad
            .SingleOrDefaultAsync(x =>
                x.UnidadComercialId == unidadId &&
                x.ConceptoCobroId == conceptoCobroId,
                cancellationToken);

        bool nueva = tarifa is null;

        if (tarifa is null)
        {
            tarifa = new TarifaUnidad(
                unidadId,
                conceptoCobroId,
                solicitud.MonedaId,
                solicitud.Monto);

            _db.TarifasUnidad.Add(tarifa);
        }
        else
        {
            tarifa.Actualizar(
                solicitud.MonedaId,
                solicitud.Monto);
        }

        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            tarifa.Id,
            tarifa.UnidadComercialId,
            tarifa.ConceptoCobroId,
            tarifa.MonedaId,
            tarifa.Monto,
            tarifa.FechaActualizacionUtc,
            creada = nueva
        });
    }

    // ==================================================
    // VISTA PREVIA DE GENERACION MENSUAL
    // NO CREA NI MODIFICA OBLIGACIONES
    // ==================================================

    [HttpGet("vista-previa")]
    public async Task<IActionResult> VistaPrevia(
        [FromQuery] Guid conceptoCobroId,
        [FromQuery] int anio,
        [FromQuery] int mes,
        CancellationToken cancellationToken)
    {
        if (conceptoCobroId == Guid.Empty ||
            anio < 2000 ||
            anio > 2100 ||
            mes < 1 ||
            mes > 12)
        {
            return BadRequest(new
            {
                mensaje =
                    "Indica un concepto y un periodo valido."
            });
        }

        var conceptoExiste = await _db.ConceptosCobro
            .AsNoTracking()
            .AnyAsync(x =>
                x.Id == conceptoCobroId &&
                x.Activo,
                cancellationToken);

        if (!conceptoExiste)
        {
            return NotFound(new
            {
                mensaje = "Concepto no encontrado o inactivo."
            });
        }

        var casetas = await _db.UnidadesComerciales
            .AsNoTracking()
            .Where(x =>
                x.Activa &&
                x.Tipo == TipoUnidadComercial.Caseta)
            .Select(x => new
            {
                x.Id,
                x.Codigo,
                x.Bloque
            })
            .ToListAsync(cancellationToken);

        var tarifas = await _db.TarifasUnidad
            .AsNoTracking()
            .Where(x =>
                x.ConceptoCobroId == conceptoCobroId)
            .ToListAsync(cancellationToken);

        var obligacionesExistentes = await _db.Obligaciones
            .AsNoTracking()
            .Where(x =>
                x.ConceptoCobroId == conceptoCobroId &&
                x.PeriodoAnio == anio &&
                x.PeriodoMes == mes)
            .Select(x => x.UnidadComercialId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var tarifasPorCaseta = tarifas.ToDictionary(
            x => x.UnidadComercialId);

        var conDeuda = obligacionesExistentes.ToHashSet();

        var detalle = casetas.Select(c =>
        {
            tarifasPorCaseta.TryGetValue(
                c.Id,
                out var tarifa);

            var deudaExistente = conDeuda.Contains(c.Id);

            var estado = deudaExistente
                ? "DeudaExistente"
                : tarifa is null
                    ? "SinTarifa"
                    : "Lista";

            return new
            {
                UnidadComercialId = c.Id,
                Caseta = c.Codigo,
                c.Bloque,

                Estado = estado,

                Monto = tarifa?.Monto,
                MonedaId = tarifa?.MonedaId,

                DeudaExistente = deudaExistente
            };
        })
        .OrderBy(x => x.Caseta)
        .ToList();

        // Los totales se separan por moneda.
        // Nunca sumar bolivianos y dolares.

        var totales = detalle
            .Where(x => x.Estado == "Lista")
            .GroupBy(x => x.MonedaId)
            .Select(grupo => new
            {
                MonedaId = grupo.Key,
                Cantidad = grupo.Count(),

                Total = grupo.Sum(
                    x => x.Monto.GetValueOrDefault())
            })
            .ToList();

        return Ok(new
        {
            conceptoCobroId,
            anio,
            mes,

            totalCasetas = detalle.Count,

            listas = detalle.Count(
                x => x.Estado == "Lista"),

            sinTarifa = detalle.Count(
                x => x.Estado == "SinTarifa"),

            conDeudaExistente = detalle.Count(
                x => x.Estado == "DeudaExistente"),

            totalesPorMoneda = totales,

            detalle,

            aviso =
                "VISTA PREVIA. No se creo ninguna deuda."
        });
    }

    // ==================================================
    // MERCADEX_GENERACION_INDIVIDUAL_V1
    // GENERACION MENSUAL POR TARIFA DE CADA CASETA
    // ==================================================

    [HttpPost("generar-mensual")]
    public async Task<IActionResult> GenerarMensual(
        [FromBody] GenerarMensualTarifasRequest? solicitud,
        CancellationToken cancellationToken)
    {
        if (Environment.GetEnvironmentVariable(
                "MERCADEX_ENABLE_V15") != "SI" ||
            !string.Equals(
                _db.Database.GetDbConnection().Database,
                "mercadex_db_4qha",
                StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(403, new
            {
                mensaje = "Modulo no habilitado."
            });
        }
        if (solicitud is null ||
            solicitud.ConceptoCobroId == Guid.Empty ||
            solicitud.Anio < 2000 ||
            solicitud.Anio > 2100 ||
            solicitud.Mes < 1 ||
            solicitud.Mes > 12 ||
            solicitud.FechaEmision == default ||
            solicitud.ListasEsperadas is < 0)
        {
            return BadRequest(new
            {
                mensaje = "Los datos de generacion son invalidos."
            });
        }

        if (solicitud.FechaVencimiento.HasValue &&
            solicitud.FechaVencimiento.Value <
                solicitud.FechaEmision)
        {
            return BadRequest(new
            {
                mensaje = "La fecha de vencimiento es anterior a la emision."
            });
        }

        if (!solicitud.Confirmar)
        {
            return BadRequest(new
            {
                mensaje =
                    "Debes confirmar expresamente la generacion mensual."
            });
        }

        // Transaccion serializable para evitar lecturas
        // inconsistentes y generaciones parciales.

        await using var transaccion =
            await _db.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable,
                cancellationToken);

        // Bloqueo exclusivo por concepto y periodo.
        // Dos ejecuciones simultaneas esperan su turno.

        var claveBloqueo =
            $"mercadex-tarifas:{solicitud.ConceptoCobroId:N}";

        var periodoBloqueo =
            solicitud.Anio * 100 + solicitud.Mes;

        await _db.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock(hashtext({claveBloqueo}), {periodoBloqueo})",
            cancellationToken);

        var conceptoValido =
            await _db.ConceptosCobro
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == solicitud.ConceptoCobroId &&
                    x.Activo,
                    cancellationToken);

        if (!conceptoValido)
        {
            return NotFound(new
            {
                mensaje = "El concepto no existe o esta inactivo."
            });
        }

        // Casetas activas, exclusivamente.
        // No incluir comedores ni ambulantes.

        var casetas = await _db.UnidadesComerciales
            .AsNoTracking()
            .Where(x =>
                x.Activa &&
                x.Tipo == TipoUnidadComercial.Caseta)
            .Select(x => new
            {
                x.Id,
                x.Codigo
            })
            .OrderBy(x => x.Codigo)
            .ToListAsync(cancellationToken);

        if (casetas.Count == 0)
        {
            return Conflict(new
            {
                mensaje = "No existen casetas activas."
            });
        }

        var tarifas = await _db.TarifasUnidad
            .AsNoTracking()
            .Where(x =>
                x.ConceptoCobroId == solicitud.ConceptoCobroId)
            .ToListAsync(cancellationToken);

        var tarifasPorUnidad = tarifas
            .ToDictionary(x => x.UnidadComercialId);

        // Bloquear cualquier generacion incompleta.

        var sinTarifa = casetas
            .Where(x => !tarifasPorUnidad.ContainsKey(x.Id))
            .Select(x => x.Codigo)
            .ToList();

        if (sinTarifa.Count > 0)
        {
            return Conflict(new
            {
                mensaje =
                    "Generacion cancelada. Primero configura " +
                    "las tarifas de todas las casetas activas.",
                cantidadSinTarifa = sinTarifa.Count,
                casetasSinTarifa = sinTarifa,
                obligacionesGeneradas = 0
            });
        }

        var monedasActivas = await _db.Monedas
            .AsNoTracking()
            .Where(x => x.Activa)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var monedasValidas = monedasActivas.ToHashSet();

        var tarifasInvalidas = casetas
            .Where(x =>
            {
                var tarifa = tarifasPorUnidad[x.Id];

                return tarifa.Monto <= 0 ||
                    decimal.Round(tarifa.Monto, 2) != tarifa.Monto ||
                    !monedasValidas.Contains(tarifa.MonedaId);
            })
            .Select(x => x.Codigo)
            .ToList();

        if (tarifasInvalidas.Count > 0)
        {
            return Conflict(new
            {
                mensaje =
                    "Generacion cancelada. Existen tarifas " +
                    "o monedas que requieren correccion.",
                casetas = tarifasInvalidas,
                obligacionesGeneradas = 0
            });
        }

        // Revisar todas las obligaciones del mismo
        // concepto y periodo, incluyendo pagadas.

        var existentes = await _db.Obligaciones
            .AsNoTracking()
            .Where(x =>
                x.ConceptoCobroId == solicitud.ConceptoCobroId &&
                x.PeriodoAnio == solicitud.Anio &&
                x.PeriodoMes == solicitud.Mes)
            .Select(x => x.UnidadComercialId)
            .ToListAsync(cancellationToken);

        // Si ya hubiera duplicaciones historicas,
        // no agregar nuevas obligaciones.

        var duplicadosPrevios = existentes
            .GroupBy(x => x)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToList();

        if (duplicadosPrevios.Count > 0)
        {
            return Conflict(new
            {
                mensaje =
                    "El periodo contiene obligaciones duplicadas. " +
                    "Debe revisarse antes de generar otras.",
                cantidadUnidadesDuplicadas =
                    duplicadosPrevios.Count,
                obligacionesGeneradas = 0
            });
        }

        var unidadesConDeuda = existentes.ToHashSet();

        var pendientes = casetas
            .Where(x => !unidadesConDeuda.Contains(x.Id))
            .ToList();

        // Protección ante cambios entre la vista previa
        // y la confirmacion del operador.

        if (solicitud.ListasEsperadas.HasValue &&
            solicitud.ListasEsperadas.Value != pendientes.Count)
        {
            return Conflict(new
            {
                mensaje =
                    "La vista previa cambio. Actualizala antes " +
                    "de confirmar la generacion.",
                listasActuales = pendientes.Count,
                listasEsperadas = solicitud.ListasEsperadas,
                obligacionesGeneradas = 0
            });
        }

        if (pendientes.Count == 0)
        {
            return Conflict(new
            {
                mensaje =
                    "No hay obligaciones nuevas por generar " +
                    "para este periodo.",
                obligacionesExistentes = existentes.Count,
                obligacionesGeneradas = 0
            });
        }

        // Cada caseta obtiene su monto y moneda propios.

        var nuevas = pendientes
            .Select(c =>
            {
                var tarifa = tarifasPorUnidad[c.Id];

                return new Obligacion(
                    c.Id,
                    solicitud.ConceptoCobroId,
                    tarifa.MonedaId,
                    tarifa.Monto,
                    solicitud.FechaEmision,
                    solicitud.FechaVencimiento,
                    solicitud.Anio,
                    solicitud.Mes);
            })
            .ToList();

        _db.Obligaciones.AddRange(nuevas);

        // Un solo guardado y una sola transaccion.
        // Un error revierte toda la operacion.

        await _db.SaveChangesAsync(cancellationToken);
        await transaccion.CommitAsync(cancellationToken);

        var totales = nuevas
            .GroupBy(x => x.MonedaId)
            .Select(grupo => new
            {
                monedaId = grupo.Key,
                cantidad = grupo.Count(),
                total = grupo.Sum(x => x.Monto)
            })
            .ToList();

        return Ok(new
        {
            mensaje = "Generacion mensual completada.",
            conceptoCobroId = solicitud.ConceptoCobroId,
            anio = solicitud.Anio,
            mes = solicitud.Mes,
            casetasActivas = casetas.Count,
            obligacionesGeneradas = nuevas.Count,
            obligacionesExistentes = existentes.Count,
            sinTarifa = 0,
            totalesPorMoneda = totales
        });
    }
}

public sealed record GuardarTarifaUnidadRequest(
    Guid MonedaId,
    decimal Monto);
// Solicitud para generacion mensual con tarifas por caseta.
// ListasEsperadas se obtiene de la vista previa.

public sealed record GenerarMensualTarifasRequest(
    Guid ConceptoCobroId,
    int Anio,
    int Mes,
    DateOnly FechaEmision,
    DateOnly? FechaVencimiento,
    int? ListasEsperadas,
    bool Confirmar);