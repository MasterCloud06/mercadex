
using Mercadex.Domain.Organizacion;
using Mercadex.Domain.Socios;
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

        // ---------- Ficha completa (manual: Kardex de cada caseta) ----------

        var pasillo = unidad.PasilloId is null
            ? null
            : await _db.Pasillos.AsNoTracking()
                .Where(p => p.Id == unidad.PasilloId)
                .Select(p => new { p.Codigo, p.Nombre })
                .FirstOrDefaultAsync(cancellationToken);

        var titular = await (
                from a in _db.AsignacionesSocioCaseta.AsNoTracking()
                join so in _db.Socios.AsNoTracking() on a.SocioId equals so.Id
                where a.UnidadComercialId == unidadId && a.Activa
                orderby a.FechaAsignacion descending
                select new
                {
                    so.Id,
                    nombre = so.Nombres + " " + so.Apellidos,
                    documento = so.DocumentoIdentidad,
                    telefono = so.Telefono,
                    fechaRegistro = so.FechaRegistro
                })
            .FirstOrDefaultAsync(cancellationToken);

        // Los documentos se buscan del socio del Kardex o, si no tiene, del titular.
        var socioDocumentos = kardex?.SocioId ?? titular?.Id;

        var documentos = socioDocumentos is null
            ? new List<DocumentoResumen>()
            : await _db.DocumentosKardex.AsNoTracking()
                .Where(d => d.SocioId == socioDocumentos &&
                            (d.UnidadComercialId == null || d.UnidadComercialId == unidadId))
                .OrderByDescending(d => d.FechaRegistroUtc)
                .Select(d => new DocumentoResumen(
                    d.Id, (int)d.Tipo, d.Tipo.ToString(), d.NombreArchivo, d.TipoContenido,
                    d.RegistradoPor, d.FechaRegistroUtc))
                .ToListAsync(cancellationToken);

        var tipos = documentos.Select(d => d.TipoNumero).ToHashSet();
        var faltantes = new List<object>();

        void Falta(TipoDocumentoKardex? tipo, string texto)
        {
            if (tipo is null || !tipos.Contains((int)tipo.Value))
                faltantes.Add(new { tipo = (int?)tipo, texto });
        }

        if (socioDocumentos is null)
        {
            faltantes.Add(new { tipo = (int?)null, texto = "La caseta no tiene titular ni socio en el Kardex." });
        }
        else
        {
            Falta(TipoDocumentoKardex.Identidad, "CI escaneado");
            Falta(TipoDocumentoKardex.Fotografia, "Foto 3x2");

            if (!string.IsNullOrWhiteSpace(kardex?.TipoDocumentoPropiedad))
                Falta(TipoDocumentoKardex.Titularidad, "Documento de propiedad escaneado (Derechos Reales o Minuta)");

            if (string.Equals(kardex?.EstadoVida, "FALLECIDO", StringComparison.OrdinalIgnoreCase))
                Falta(TipoDocumentoKardex.Defuncion, "Certificado de óbito escaneado");

            if (kardex?.AporteSocioPagado == true)
                Falta(TipoDocumentoKardex.ComprobanteAporte, "Comprobante del aporte de socio escaneado");
        }

        if (kardex is null || kardex.FechaIngreso is null)
            faltantes.Add(new { tipo = (int?)null, texto = "Fecha de ingreso" });

        if (string.Equals(kardex?.TipoDocumentoPropiedad, "MINUTA", StringComparison.OrdinalIgnoreCase) &&
            string.IsNullOrWhiteSpace(kardex?.PropietarioAnterior))
            faltantes.Add(new { tipo = (int?)null, texto = "Nombre del propietario anterior (Minuta)" });

        Response.Headers["Cache-Control"] = "no-store";

        return Ok(new
        {
            unidad,
            kardex,
            pasillo,
            titular,
            socioDocumentosId = socioDocumentos,
            documentos = documentos.Select(d => new
            {
                id = d.Id,
                tipo = d.TipoNumero,
                tipoNombre = d.TipoNombre,
                nombreArchivo = d.NombreArchivo,
                tipoContenido = d.TipoContenido,
                registradoPor = d.RegistradoPor,
                fechaRegistroUtc = d.FechaRegistroUtc
            }),
            faltantes
        });
    }

    private sealed record DocumentoResumen(
        Guid Id,
        int TipoNumero,
        string TipoNombre,
        string NombreArchivo,
        string TipoContenido,
        string RegistradoPor,
        DateTimeOffset FechaRegistroUtc);

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
