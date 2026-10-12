using Mercadex.Domain.Comercial;
using Mercadex.Domain.Organizacion;
using Mercadex.Domain.Socios;
using Mercadex.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mercadex.Api.Controllers;

/// <summary>
/// Importación del padrón (casetas, bloques, socios y titulares) desde
/// una planilla. Primero se valida sin guardar nada; solo si no hay
/// errores se aplica, todo junto en una sola transacción.
/// Nunca borra datos: crea lo que falta, actualiza el bloque y cambia
/// el titular dejando el anterior en el historial.
/// </summary>
[ApiController]
[Route("api/importacion")]
public sealed class ImportacionController : ControllerBase
{
    private const int MaximoFilas = 3000;

    private readonly MercadexDbContext _db;

    public ImportacionController(MercadexDbContext db)
    {
        _db = db;
    }

    public sealed record FilaPadron(
        int Fila,
        string? Codigo,
        string? Tipo,
        string? Bloque,
        string? Nombres,
        string? Apellidos,
        string? Documento,
        string? Telefono);

    public sealed record ImportarPadronRequest(List<FilaPadron>? Filas);

    private sealed class Resultado
    {
        public int Fila { get; init; }
        public string Codigo { get; init; } = "";
        public bool Valida { get; set; } = true;
        public List<string> Acciones { get; } = new();
        public List<string> Errores { get; } = new();
    }

    /// <summary>
    /// aplicar=false: solo valida y describe lo que se haría.
    /// aplicar=true: valida y, si no hay errores, guarda.
    /// </summary>
    [HttpPost("padron")]
    public async Task<IActionResult> Padron(
        [FromBody] ImportarPadronRequest solicitud,
        [FromQuery] bool aplicar,
        CancellationToken ct)
    {
        var filas = solicitud.Filas ?? new List<FilaPadron>();

        if (filas.Count == 0)
            return BadRequest(new { mensaje = "La planilla no tiene filas con datos." });

        if (filas.Count > MaximoFilas)
            return BadRequest(new { mensaje = $"Se pueden importar hasta {MaximoFilas} filas por vez." });

        static string? Limpio(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();

        var unidades = await _db.UnidadesComerciales.ToListAsync(ct);
        var unidadPorCodigo = unidades.ToDictionary(u => u.Codigo, StringComparer.OrdinalIgnoreCase);

        var socios = await _db.Socios.ToListAsync(ct);
        var socioPorDocumento = socios
            .Where(s => !string.IsNullOrWhiteSpace(s.DocumentoIdentidad) && s.DocumentoIdentidad != "SIN_DOCUMENTO")
            .GroupBy(s => s.DocumentoIdentidad.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var asignacionesActivas = await _db.AsignacionesSocioCaseta
            .Where(a => a.Activa)
            .ToListAsync(ct);

        var resultados = new List<Resultado>();
        var codigosVistos = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var sociosNuevosPorDocumento = new Dictionary<string, Socio>(StringComparer.OrdinalIgnoreCase);

        int unidadesNuevas = 0, bloquesActualizados = 0, sociosNuevos = 0, titularesAsignados = 0;

        foreach (var f in filas)
        {
            var codigo = Limpio(f.Codigo)?.ToUpperInvariant() ?? "";
            var r = new Resultado { Fila = f.Fila, Codigo = codigo };
            resultados.Add(r);

            void Error(string mensaje)
            {
                r.Valida = false;
                r.Errores.Add(mensaje);
            }

            // ---------- Unidad ----------

            if (codigo.Length == 0)
            {
                Error("Falta el número de caseta.");
                continue;
            }

            if (codigo.Length > 50)
                Error("El número de caseta no puede superar 50 caracteres.");

            if (codigosVistos.TryGetValue(codigo, out var filaAnterior))
                Error($"La caseta {codigo} ya aparece en la fila {filaAnterior}.");
            else
                codigosVistos[codigo] = f.Fila;

            TipoUnidadComercial tipo = TipoUnidadComercial.Caseta;
            var tipoTexto = Limpio(f.Tipo)?.ToLowerInvariant();
            if (tipoTexto != null)
            {
                switch (tipoTexto)
                {
                    case "caseta": tipo = TipoUnidadComercial.Caseta; break;
                    case "comedor": tipo = TipoUnidadComercial.Comedor; break;
                    case "ambulante": tipo = TipoUnidadComercial.Ambulante; break;
                    default: Error($"Tipo \"{f.Tipo}\" no válido. Use Caseta, Comedor o Ambulante."); break;
                }
            }

            var bloque = Limpio(f.Bloque);
            if (bloque?.Length > 50)
                Error("El bloque no puede superar 50 caracteres.");

            // ---------- Titular ----------

            var nombres = Limpio(f.Nombres);
            var apellidos = Limpio(f.Apellidos);
            var documento = Limpio(f.Documento);
            var telefono = Limpio(f.Telefono);

            var hayTitular = nombres != null || apellidos != null || documento != null || telefono != null;

            if (hayTitular)
            {
                if (nombres == null) Error("Faltan los nombres del titular.");
                if (apellidos == null) Error("Faltan los apellidos del titular.");
                if (documento == null) Error("Falta el documento de identidad del titular.");
                if (nombres?.Length > 150 || apellidos?.Length > 150) Error("Nombres o apellidos demasiado largos (máx. 150).");
                if (documento?.Length > 50 || telefono?.Length > 50) Error("Documento o teléfono demasiado largo (máx. 50).");
            }

            if (!r.Valida)
                continue;

            // ---------- Aplicar (en memoria) ----------

            if (!unidadPorCodigo.TryGetValue(codigo, out var unidad))
            {
                unidad = new UnidadComercial(codigo, tipo, null, bloque);
                unidadPorCodigo[codigo] = unidad;
                _db.UnidadesComerciales.Add(unidad);
                unidadesNuevas++;
                r.Acciones.Add($"Registrar {tipo.ToString().ToLowerInvariant()} nueva" + (bloque != null ? $" en el bloque {bloque}" : ""));
            }
            else
            {
                if (tipoTexto != null && unidad.Tipo != tipo)
                    r.Acciones.Add($"Aviso: ya está registrada como {unidad.Tipo.ToString().ToLowerInvariant()}; el tipo no se cambia");

                if (bloque != null && !string.Equals(unidad.Bloque, bloque, StringComparison.OrdinalIgnoreCase))
                {
                    r.Acciones.Add(unidad.Bloque == null
                        ? $"Asignar el bloque {bloque}"
                        : $"Cambiar el bloque de {unidad.Bloque} a {bloque}");
                    unidad.CambiarUbicacion(unidad.PasilloId, bloque);
                    bloquesActualizados++;
                }
            }

            if (hayTitular)
            {
                if (!socioPorDocumento.TryGetValue(documento!, out var socio) &&
                    !sociosNuevosPorDocumento.TryGetValue(documento!, out socio))
                {
                    socio = new Socio(nombres!, apellidos!, documento!, telefono);
                    sociosNuevosPorDocumento[documento!] = socio;
                    _db.Socios.Add(socio);
                    sociosNuevos++;
                    r.Acciones.Add($"Registrar socio {nombres} {apellidos} (CI {documento})");
                }
                else if (!socio.Activo)
                {
                    Error($"El socio con documento {documento} está inactivo. Actívalo en Socios antes de importar.");
                    continue;
                }
                else if (!string.Equals($"{socio.Nombres} {socio.Apellidos}", $"{nombres} {apellidos}", StringComparison.OrdinalIgnoreCase))
                {
                    r.Acciones.Add($"Aviso: el documento {documento} ya es de {socio.Nombres} {socio.Apellidos}; se usa ese socio");
                }

                var activas = asignacionesActivas.Where(a => a.UnidadComercialId == unidad.Id).ToList();

                if (activas.Count == 1 && activas[0].SocioId == socio.Id)
                {
                    r.Acciones.Add("El titular ya está registrado");
                }
                else
                {
                    foreach (var anterior in activas)
                    {
                        anterior.Desactivar();
                        asignacionesActivas.Remove(anterior);
                    }

                    var nueva = new AsignacionSocioCaseta(socio.Id, unidad.Id, DateTimeOffset.UtcNow);
                    _db.AsignacionesSocioCaseta.Add(nueva);
                    asignacionesActivas.Add(nueva);
                    titularesAsignados++;
                    r.Acciones.Add(activas.Count == 0
                        ? "Asignar titular"
                        : "Cambiar de titular (el anterior queda en el historial)");
                }
            }

            if (r.Acciones.Count == 0)
                r.Acciones.Add("Sin cambios");
        }

        var errores = resultados.Count(r => !r.Valida);
        var aplicado = false;

        if (aplicar && errores == 0)
        {
            await using var transaccion = await _db.Database.BeginTransactionAsync(ct);
            await _db.SaveChangesAsync(ct);
            await transaccion.CommitAsync(ct);
            aplicado = true;
        }

        return Ok(new
        {
            aplicado,
            filas = resultados.Count,
            errores,
            unidadesNuevas,
            bloquesActualizados,
            sociosNuevos,
            titularesAsignados,
            resultados = resultados.Select(r => new
            {
                fila = r.Fila,
                codigo = r.Codigo,
                valida = r.Valida,
                detalle = r.Valida ? string.Join(". ", r.Acciones) : string.Join(" ", r.Errores)
            })
        });
    }
}
