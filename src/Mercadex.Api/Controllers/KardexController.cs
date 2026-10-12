
using Mercadex.Domain.Organizacion;
using Mercadex.Infrastructure.Persistence;
using Mercadex.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mercadex.Api.Controllers;

[ApiController]
[Route("api/kardex")]
public sealed class KardexController : ControllerBase
{
    private readonly MercadexDbContext _db;

    public KardexController(MercadexDbContext db)
    {
        _db = db;
    }

    // Consultar el Kardex de una caseta.
    // Si todavía no tiene ficha, devuelve sus datos básicos.

    [HttpGet("caseta/{unidadId:guid}")]
    public async Task<IActionResult> Obtener(
        Guid unidadId,
        CancellationToken cancellationToken)
    {
        var unidad = await _db.UnidadesComerciales
            .AsNoTracking()
            .Where(u =>
                u.Id == unidadId &&
                u.Tipo == TipoUnidadComercial.Caseta)
            .Select(u => new
            {
                u.Id,
                u.Codigo,
                u.Bloque,
                u.PasilloId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (unidad is null)
        {
            return NotFound(new
            {
                mensaje = "No se encontró la caseta."
            });
        }

        var kardex = await _db.KardexCasetas
            .AsNoTracking()
            .FirstOrDefaultAsync(
                k => k.UnidadComercialId == unidadId,
                cancellationToken);

        return Ok(new
        {
            unidad,
            kardex
        });
    }

    // Crear o actualizar la ficha de una caseta.

    [HttpPut("caseta/{unidadId:guid}")]
    public async Task<IActionResult> Guardar(
        Guid unidadId,
        [FromBody] GuardarKardexRequest datos,
        CancellationToken cancellationToken)
    {
        var existeCaseta = await _db.UnidadesComerciales
            .AnyAsync(
                u => u.Id == unidadId &&
                     u.Tipo == TipoUnidadComercial.Caseta,
                cancellationToken);

        if (!existeCaseta)
        {
            return NotFound(new
            {
                mensaje = "La caseta no existe."
            });
        }

        if (datos.SocioId.HasValue)
        {
            var existeSocio = await _db.Socios
                .AnyAsync(
                    s => s.Id == datos.SocioId.Value,
                    cancellationToken);

            if (!existeSocio)
            {
                return BadRequest(new
                {
                    mensaje = "El socio seleccionado no existe."
                });
            }
        }

        var estadosPermitidos = new[]
        {
            "VIVO",
            "FALLECIDO"
        };

        var documentosPermitidos = new[]
        {
            "DERECHOS_REALES",
            "MINUTA",
            "OTRO"
        };

        var estado = Limpiar(datos.EstadoVida)?
            .ToUpperInvariant();

        var tipoDocumento =
            Limpiar(datos.TipoDocumentoPropiedad)?
                .ToUpperInvariant();

        if (estado is not null &&
            !estadosPermitidos.Contains(estado))
        {
            return BadRequest(new
            {
                mensaje = "Estado de vida no válido."
            });
        }

        if (tipoDocumento is not null &&
            !documentosPermitidos.Contains(tipoDocumento))
        {
            return BadRequest(new
            {
                mensaje = "Tipo de documento no válido."
            });
        }

        if (datos.FechaIngreso.HasValue &&
            datos.FechaIngreso.Value >
                DateOnly.FromDateTime(DateTime.UtcNow))
        {
            return BadRequest(new
            {
                mensaje = "La fecha de ingreso no puede ser futura."
            });
        }

        var kardex = await _db.KardexCasetas
            .FirstOrDefaultAsync(
                k => k.UnidadComercialId == unidadId,
                cancellationToken);

        var nuevo = kardex is null;

        if (kardex is null)
        {
            kardex = new KardexCaseta
            {
                Id = Guid.NewGuid(),
                UnidadComercialId = unidadId,
                FechaRegistro = DateTimeOffset.UtcNow
            };

            _db.KardexCasetas.Add(kardex);
        }

        kardex.SocioId = datos.SocioId;
        kardex.FechaIngreso = datos.FechaIngreso;
        kardex.EstadoVida = estado;
        kardex.TipoDocumentoPropiedad = tipoDocumento;

        kardex.PropietarioAnterior =
            Limpiar(datos.PropietarioAnterior);

        kardex.AporteSocioPagado =
            datos.AporteSocioPagado;

        kardex.Observaciones =
            Limpiar(datos.Observaciones);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Conflict(new
            {
                mensaje =
                    "No se pudo guardar. Comprueba si la ficha " +
                    "fue modificada simultáneamente."
            });
        }

        return Ok(new
        {
            mensaje = nuevo
                ? "Kardex creado correctamente."
                : "Kardex actualizado correctamente.",

            kardex.Id,
            kardex.UnidadComercialId,
            kardex.FechaRegistro
        });
    }

    private static string? Limpiar(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor)
            ? null
            : valor.Trim();
    }
}

public sealed class GuardarKardexRequest
{
    public Guid? SocioId { get; set; }

    public DateOnly? FechaIngreso { get; set; }

    public string? EstadoVida { get; set; }

    public string? TipoDocumentoPropiedad { get; set; }

    public string? PropietarioAnterior { get; set; }

    public bool? AporteSocioPagado { get; set; }

    public string? Observaciones { get; set; }
}
