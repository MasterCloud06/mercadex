using Mercadex.Domain.Organizacion;
using Mercadex.Infrastructure.Persistence;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Npgsql;

namespace Mercadex.Api.Controllers;

[ApiController]
[Route("api/unidades-comerciales")]
public sealed class UnidadesComercialesController : ControllerBase
{
    private readonly MercadexDbContext _context;

    public UnidadesComercialesController(
        MercadexDbContext context)
    {
        _context = context;
    }

    // =========================================================
    // CONSULTAR TODAS LAS UNIDADES
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Obtener()
    {
        var unidades = await _context.UnidadesComerciales
            .AsNoTracking()
            .OrderBy(u => u.Tipo)
            .ThenBy(u => u.Codigo)
            .Select(u => new
            {
                u.Id,
                u.Codigo,
                u.Tipo,
                u.PasilloId,
                u.Bloque,
                u.Activa,
                u.FechaRegistro
            })
            .ToListAsync();

        return Ok(unidades);
    }

    // =========================================================
    // CONSULTAR UNA UNIDAD POR SU ID
    // =========================================================

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObtenerPorId(Guid id)
    {
        var unidad = await _context.UnidadesComerciales
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new
            {
                u.Id,
                u.Codigo,
                u.Tipo,
                u.PasilloId,
                u.Bloque,
                u.Activa,
                u.FechaRegistro
            })
            .FirstOrDefaultAsync();

        if (unidad is null)
        {
            return NotFound(new
            {
                mensaje = "La unidad comercial no existe."
            });
        }

        return Ok(unidad);
    }

    // =========================================================
    // REGISTRAR CASETA, COMEDOR O AMBULANTE
    // =========================================================

    [HttpPost]
    public async Task<IActionResult> Crear(
        [FromBody] CrearUnidadComercialRequest solicitud)
    {
        if (string.IsNullOrWhiteSpace(solicitud.Codigo))
        {
            return BadRequest(new
            {
                mensaje = "El código es obligatorio."
            });
        }

        if (!Enum.IsDefined(solicitud.Tipo))
        {
            return BadRequest(new
            {
                mensaje = "El tipo de unidad comercial no es válido."
            });
        }

        var codigo =
            solicitud.Codigo
                .Trim()
                .ToUpperInvariant();

        if (codigo.Length > 50)
        {
            return BadRequest(new
            {
                mensaje =
                    "El código no puede superar los 50 caracteres."
            });
        }

        var bloque =
            string.IsNullOrWhiteSpace(solicitud.Bloque)
                ? null
                : solicitud.Bloque.Trim();

        if (bloque?.Length > 50)
        {
            return BadRequest(new
            {
                mensaje =
                    "El bloque no puede superar los 50 caracteres."
            });
        }

        // Evitar códigos repetidos.

        var codigoExiste =
            await _context.UnidadesComerciales
                .AnyAsync(u => u.Codigo == codigo);

        if (codigoExiste)
        {
            return Conflict(new
            {
                mensaje =
                    "Ya existe una unidad comercial con ese código."
            });
        }

        // Verificar el pasillo, si se indicó uno.

        if (solicitud.PasilloId.HasValue)
        {
            var pasilloValido =
                await _context.Pasillos
                    .AnyAsync(p =>
                        p.Id == solicitud.PasilloId.Value &&
                        p.Activo);

            if (!pasilloValido)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El pasillo no existe o está inactivo."
                });
            }
        }

        var unidad = new UnidadComercial(
            codigo,
            solicitud.Tipo,
            solicitud.PasilloId,
            bloque
        );

        _context.UnidadesComerciales.Add(unidad);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation
            })
        {
            return Conflict(new
            {
                mensaje =
                    "Ya existe una unidad comercial con ese código."
            });
        }

        return CreatedAtAction(
            nameof(ObtenerPorId),
            new
            {
                id = unidad.Id
            },
            new
            {
                unidad.Id,
                unidad.Codigo,
                unidad.Tipo,
                unidad.PasilloId,
                unidad.Bloque,
                unidad.Activa,
                unidad.FechaRegistro
            });
    }

    // =========================================================
    // ACTUALIZAR PASILLO / BLOQUE DE UNA UNIDAD EXISTENTE
    // =========================================================

    [HttpPut("{id:guid}/ubicacion")]
    public async Task<IActionResult> ActualizarUbicacion(
        Guid id,
        [FromBody] ActualizarUbicacionUnidadRequest solicitud)
    {
        var unidad =
            await _context.UnidadesComerciales
                .FirstOrDefaultAsync(u => u.Id == id);

        if (unidad is null)
        {
            return NotFound(new
            {
                mensaje =
                    "La unidad comercial no existe."
            });
        }

        var bloque =
            string.IsNullOrWhiteSpace(solicitud.Bloque)
                ? null
                : solicitud.Bloque.Trim();

        if (bloque?.Length > 50)
        {
            return BadRequest(new
            {
                mensaje =
                    "El bloque no puede superar los 50 caracteres."
            });
        }

        // Para una caseta queremos mantener
        // obligatoriamente la relación con un pasillo.

        if (unidad.Tipo == TipoUnidadComercial.Caseta &&
            !solicitud.PasilloId.HasValue)
        {
            return BadRequest(new
            {
                mensaje =
                    "Debes seleccionar un pasillo para la caseta."
            });
        }

        // Verificar que el pasillo exista y esté activo.

        if (solicitud.PasilloId.HasValue)
        {
            var pasilloValido =
                await _context.Pasillos
                    .AnyAsync(p =>
                        p.Id == solicitud.PasilloId.Value &&
                        p.Activo);

            if (!pasilloValido)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El pasillo seleccionado no existe o está inactivo."
                });
            }
        }

        try
        {
            unidad.CambiarUbicacion(
                solicitud.PasilloId,
                bloque);

            await _context.SaveChangesAsync();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                mensaje = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                mensaje = ex.Message
            });
        }

        return Ok(new
        {
            mensaje =
                "Ubicación actualizada correctamente.",

            unidad = new
            {
                unidad.Id,
                unidad.Codigo,
                unidad.Tipo,
                unidad.PasilloId,
                unidad.Bloque,
                unidad.Activa,
                unidad.FechaRegistro
            }
        });
    }
}

// =============================================================
// SOLICITUD PARA CREAR UNA UNIDAD
// =============================================================

public sealed record CrearUnidadComercialRequest(
    string Codigo,
    TipoUnidadComercial Tipo,
    Guid? PasilloId,
    string? Bloque
);

// =============================================================
// SOLICITUD PARA CAMBIAR PASILLO / BLOQUE
// =============================================================

public sealed record ActualizarUbicacionUnidadRequest(
    Guid? PasilloId,
    string? Bloque
);