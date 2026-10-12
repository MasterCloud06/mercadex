using System.Security.Claims;
using System.Security.Cryptography;
using Mercadex.Domain.Organizacion;
using Mercadex.Domain.Socios;
using Mercadex.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mercadex.Api.Controllers;

[ApiController]
[Route("api/kardex-documentos")]
public sealed class KardexDocumentosController : ControllerBase
{
    private const long MaximoBytes = 10L * 1024 * 1024;

    private readonly MercadexDbContext _db;

    public KardexDocumentosController(MercadexDbContext db)
    {
        _db = db;
    }

    private bool EntornoPermitido()
    {
        var ruta = Environment.GetEnvironmentVariable(
            "MERCADEX_KARDEX_STORAGE_PATH");

        // Requiere una carpeta persistente para guardar los archivos
        // (en Render: un Persistent Disk montado, p. ej. /var/data/kardex).
        return !string.IsNullOrWhiteSpace(ruta) &&
               Path.IsPathFullyQualified(ruta);
    }

    private static string CarpetaPrivada =>
        Environment.GetEnvironmentVariable(
            "MERCADEX_KARDEX_STORAGE_PATH")
        ?? throw new InvalidOperationException(
            "Almacenamiento persistente de Kardex no configurado.");

    private static string? ExtensionPermitida(
        string nombre,
        string contenido)
    {
        var ext = Path.GetExtension(nombre).ToLowerInvariant();

        return contenido.ToLowerInvariant() switch
        {
            "application/pdf" when ext == ".pdf" => ext,
            "image/jpeg" when ext is ".jpg" or ".jpeg" => ext,
            "image/png" when ext == ".png" => ext,
            _ => null
        };
    }

    private static bool FirmaValida(
        byte[] bytes,
        string contenido)
    {
        if (contenido == "application/pdf")
        {
            return bytes.Length >= 5 &&
                   bytes[0] == 0x25 &&
                   bytes[1] == 0x50 &&
                   bytes[2] == 0x44 &&
                   bytes[3] == 0x46 &&
                   bytes[4] == 0x2D;
        }

        if (contenido == "image/jpeg")
        {
            return bytes.Length >= 4 &&
                   bytes[0] == 0xFF &&
                   bytes[1] == 0xD8 &&
                   bytes[2] == 0xFF;
        }

        if (contenido == "image/png")
        {
            byte[] firma =
            {
                137, 80, 78, 71, 13, 10, 26, 10
            };

            return bytes.Length >= firma.Length &&
                   bytes.AsSpan(0, firma.Length)
                       .SequenceEqual(firma);
        }

        return false;
    }

    // MERCADEX_KARDEX_ARCHIVOS_LOCAL_V1

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] Guid socioId,
        [FromQuery] Guid? unidadComercialId,
        CancellationToken ct)
    {
        if (!EntornoPermitido())
            return StatusCode(503, new
            {
                mensaje =
                    "Almacenamiento de documentos Kardex no configurado " +
                    "(MERCADEX_KARDEX_STORAGE_PATH)."
            });

        if (socioId == Guid.Empty ||
            unidadComercialId == Guid.Empty)
        {
            return BadRequest(new
            {
                mensaje = "El socio o la caseta no son validos."
            });
        }

        var existe = await _db.Socios.AsNoTracking()
            .AnyAsync(x => x.Id == socioId, ct);

        if (!existe)
            return NotFound(new
            {
                mensaje = "Socio no encontrado."
            });

        var consulta = _db.DocumentosKardex
            .AsNoTracking()
            .Where(x => x.SocioId == socioId);

        if (unidadComercialId.HasValue)
        {
            consulta = consulta.Where(x =>
                x.UnidadComercialId == unidadComercialId.Value);
        }

        var documentos = await consulta
            .OrderByDescending(x => x.FechaRegistroUtc)
            .ToListAsync(ct);

        return Ok(new
        {
            socioId,
            cantidad = documentos.Count,
            documentos = documentos.Select(x => new
            {
                x.Id,
                x.SocioId,
                x.UnidadComercialId,
                tipo = x.Tipo.ToString(),
                x.NombreArchivo,
                x.TipoContenido,
                x.TamanoBytes,
                x.Sha256,
                x.RegistradoPor,
                x.FechaRegistroUtc
            })
        });
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(11 * 1024 * 1024)]
    [RequestFormLimits(
        MultipartBodyLengthLimit = 11 * 1024 * 1024)]
    public async Task<IActionResult> Adjuntar(
        [FromForm] SubirDocumentoKardexRequest solicitud,
        CancellationToken ct)
    {
        if (!EntornoPermitido())
            return StatusCode(503, new
            {
                mensaje =
                    "Almacenamiento de documentos Kardex no configurado " +
                    "(MERCADEX_KARDEX_STORAGE_PATH)."
            });

        if (solicitud.SocioId == Guid.Empty ||
            solicitud.UnidadComercialId == Guid.Empty ||
            !Enum.IsDefined(solicitud.Tipo) ||
            solicitud.Archivo is null)
        {
            return BadRequest(new
            {
                mensaje = "Datos del documento incompletos."
            });
        }

        var archivo = solicitud.Archivo;

        if (archivo.Length <= 0 ||
            archivo.Length > MaximoBytes)
        {
            return BadRequest(new
            {
                mensaje = "El archivo debe tener entre 1 byte y 10 MB."
            });
        }

        var nombre = archivo.FileName;

        if (string.IsNullOrWhiteSpace(nombre) ||
            nombre.Length > 255 ||
            nombre.Contains('/') ||
            nombre.Contains('\\') ||
            nombre.Contains(".."))
        {
            return BadRequest(new
            {
                mensaje = "Nombre de archivo no permitido."
            });
        }

        var contenido = archivo.ContentType
            .Trim()
            .ToLowerInvariant();

        var extension = ExtensionPermitida(nombre, contenido);

        if (extension is null)
        {
            return BadRequest(new
            {
                mensaje = "Solo se admiten PDF, JPG y PNG."
            });
        }

        if (solicitud.Tipo == TipoDocumentoKardex.Fotografia &&
            contenido is not ("image/jpeg" or "image/png"))
        {
            return BadRequest(new
            {
                mensaje = "Las fotografias deben ser JPG o PNG."
            });
        }

        var socioExiste = await _db.Socios
            .AsNoTracking()
            .AnyAsync(x => x.Id == solicitud.SocioId, ct);

        if (!socioExiste)
            return NotFound(new
            {
                mensaje = "El socio no existe."
            });

        if (solicitud.UnidadComercialId.HasValue)
        {
            var unidadId = solicitud.UnidadComercialId.Value;

            var casetaExiste = await _db.UnidadesComerciales
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == unidadId &&
                    x.Tipo == TipoUnidadComercial.Caseta,
                    ct);

            if (!casetaExiste)
                return BadRequest(new
                {
                    mensaje = "La caseta no existe."
                });

            var asignacion = await _db.AsignacionesSocioCaseta
                .AsNoTracking()
                .AnyAsync(x =>
                    x.SocioId == solicitud.SocioId &&
                    x.UnidadComercialId == unidadId,
                    ct);

            if (!asignacion)
            {
                asignacion = await _db.AsignacionesUnidades
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.SocioId == solicitud.SocioId &&
                        x.UnidadComercialId == unidadId,
                        ct);
            }

            if (!asignacion)
                return Conflict(new
                {
                    mensaje =
                        "No existe una asignacion entre el socio " +
                        "y la caseta seleccionada."
                });
        }

        var responsable =
            User.Identity?.Name ??
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
            User.FindFirst("sub")?.Value ??
            "Sistema";

        if (string.IsNullOrWhiteSpace(responsable) ||
            responsable.Length > 120)
            return StatusCode(403, new
            {
                mensaje = "No se identifico al responsable."
            });

        await using var memoria = new MemoryStream();

        await archivo.CopyToAsync(memoria, ct);

        if (memoria.Length != archivo.Length ||
            memoria.Length > MaximoBytes)
            return BadRequest(new
            {
                mensaje = "La lectura del archivo fue incompleta."
            });

        var bytes = memoria.ToArray();

        if (!FirmaValida(bytes, contenido))
            return BadRequest(new
            {
                mensaje =
                    "El contenido no coincide con el tipo " +
                    "de archivo indicado."
            });

        var hash = Convert.ToHexString(
            SHA256.HashData(bytes));

        var documento = new DocumentoKardex(
            solicitud.SocioId,
            solicitud.UnidadComercialId,
            solicitud.Tipo,
            nombre,
            contenido,
            bytes.LongLength,
            hash,
            responsable);

        Directory.CreateDirectory(CarpetaPrivada);

        // El nombre fisico lo genera el sistema.
        // Nunca se utiliza el nombre enviado como ruta.

        var ruta = Path.Combine(
            CarpetaPrivada,
            documento.ClaveArchivo + extension);

        var guardado = false;

        try
        {
            await using (var flujo = new FileStream(
                ruta,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                81920,
                FileOptions.Asynchronous | FileOptions.WriteThrough))
            {
                await flujo.WriteAsync(bytes, ct);
                await flujo.FlushAsync(ct);
            }

            await using var transaccion =
                await _db.Database.BeginTransactionAsync(ct);

            _db.DocumentosKardex.Add(documento);

            await _db.SaveChangesAsync(ct);
            await transaccion.CommitAsync(ct);

            guardado = true;
        }
        finally
        {
            if (!guardado && System.IO.File.Exists(ruta))
            {
                try
                {
                    System.IO.File.Delete(ruta);
                }
                catch
                {
                    // El mantenimiento local debe revisar
                    // archivos huerfanos tras fallos de disco.
                }
            }
        }

        return Created(
            $"/api/kardex-documentos/{documento.Id}/descargar",
            new
            {
                documento.Id,
                documento.SocioId,
                documento.UnidadComercialId,
                tipo = documento.Tipo.ToString(),
                documento.NombreArchivo,
                documento.TamanoBytes,
                documento.Sha256,
                documento.FechaRegistroUtc
            });
    }

    [HttpGet("{id:guid}/descargar")]
    public async Task<IActionResult> Descargar(
        Guid id,
        CancellationToken ct)
    {
        if (!EntornoPermitido())
            return StatusCode(503, new
            {
                mensaje =
                    "Almacenamiento de documentos Kardex no configurado " +
                    "(MERCADEX_KARDEX_STORAGE_PATH)."
            });

        var documento = await _db.DocumentosKardex
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, ct);

        if (documento is null)
            return NotFound();

        if (documento.ClaveArchivo.Length != 32 ||
            !documento.ClaveArchivo.All(Uri.IsHexDigit))
            return Conflict(new
            {
                mensaje = "Referencia de archivo invalida."
            });

        var extension = ExtensionPermitida(
            documento.NombreArchivo,
            documento.TipoContenido);

        if (extension is null)
            return Conflict(new
            {
                mensaje = "Metadatos del archivo invalidos."
            });

        var ruta = Path.Combine(
            CarpetaPrivada,
            documento.ClaveArchivo + extension);

        if (!System.IO.File.Exists(ruta))
            return NotFound(new
            {
                mensaje = "El archivo fisico no fue encontrado."
            });

        var info = new FileInfo(ruta);

        if (info.Length <= 0 ||
            info.Length > MaximoBytes ||
            info.Length != documento.TamanoBytes)
            return Conflict(new
            {
                mensaje = "El tamano del archivo no coincide."
            });

        var bytes = await System.IO.File.ReadAllBytesAsync(
            ruta, ct);

        var hash = Convert.ToHexString(
            SHA256.HashData(bytes));

        if (!string.Equals(
            hash,
            documento.Sha256,
            StringComparison.OrdinalIgnoreCase))
        {
            return Conflict(new
            {
                mensaje =
                    "La integridad del archivo no coincide con SHA-256."
            });
        }

        Response.Headers["X-Content-Type-Options"] = "nosniff";
        Response.Headers["Cache-Control"] = "no-store";

        return File(
            bytes,
            documento.TipoContenido,
            documento.NombreArchivo);
    }
}

public sealed class SubirDocumentoKardexRequest
{
    public Guid SocioId { get; set; }

    public Guid? UnidadComercialId { get; set; }

    public TipoDocumentoKardex Tipo { get; set; }

    public IFormFile? Archivo { get; set; }
}