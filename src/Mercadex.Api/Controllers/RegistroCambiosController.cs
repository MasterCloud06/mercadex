using System.Text;
using Mercadex.Api.Seguridad;
using Mercadex.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mercadex.Api.Controllers;

/// <summary>
/// Registro de cambios (auditoría): quién hizo qué, cuándo y desde dónde.
/// Solo lectura y solo con sesión de administrador o de consulta
/// (directiva). Las líneas no se pueden editar ni borrar desde el sistema.
/// </summary>
[ApiController]
[Route("api/registro-cambios")]
[Authorize(Roles = SeguridadUsuarios.RolAdministrador + "," + SeguridadUsuarios.RolConsulta)]
public sealed class RegistroCambiosController : ControllerBase
{
    private static readonly TimeSpan ZonaBolivia = TimeSpan.FromHours(-4);
    private const int MaximoPorPagina = 200;

    private readonly MercadexDbContext _db;

    public RegistroCambiosController(MercadexDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] DateOnly? desde,
        [FromQuery] DateOnly? hasta,
        [FromQuery] string? modulo,
        [FromQuery] string? usuario,
        [FromQuery] string? texto,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 50,
        CancellationToken ct = default)
    {
        if (User.HasClaim(c => c.Type == SeguridadUsuarios.ClaimDebeCambiar))
            return StatusCode(403, new { mensaje = "Primero cambia tu contraseña temporal." });

        if (desde is { } d0 && hasta is { } h0 && d0 > h0)
            return BadRequest(new { mensaje = "La fecha inicial no puede superar la final." });

        pagina = Math.Max(1, pagina);
        tamano = Math.Clamp(tamano, 10, MaximoPorPagina);

        var consulta = Filtrar(desde, hasta, modulo, usuario, texto);

        var total = await consulta.CountAsync(ct);

        var filas = await consulta
            .OrderByDescending(x => x.FechaUtc)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .Select(x => new
            {
                x.Id,
                x.FechaUtc,
                x.Usuario,
                x.Rol,
                x.Origen,
                x.Modulo,
                x.Accion,
                x.Detalle
            })
            .ToListAsync(ct);

        // Para los filtros de la pantalla.
        var modulos = await _db.RegistroCambios.AsNoTracking()
            .Select(x => x.Modulo).Distinct().OrderBy(x => x).ToListAsync(ct);
        var usuarios = await _db.RegistroCambios.AsNoTracking()
            .Select(x => x.Usuario).Distinct().OrderBy(x => x).ToListAsync(ct);

        Response.Headers["Cache-Control"] = "no-store";

        return Ok(new
        {
            total,
            pagina,
            tamano,
            paginas = (int)Math.Ceiling(total / (double)tamano),
            modulos,
            usuarios,
            filas = filas.Select(x => new
            {
                id = x.Id,
                fecha = x.FechaUtc.ToOffset(ZonaBolivia),
                usuario = x.Usuario,
                rol = x.Rol,
                origen = x.Origen,
                modulo = x.Modulo,
                accion = x.Accion,
                detalle = x.Detalle
            })
        });
    }

    /// <summary>Lo mismo que la lista, en CSV para abrir en Excel (máximo 20 000 líneas).</summary>
    [HttpGet("csv")]
    public async Task<IActionResult> Csv(
        [FromQuery] DateOnly? desde,
        [FromQuery] DateOnly? hasta,
        [FromQuery] string? modulo,
        [FromQuery] string? usuario,
        [FromQuery] string? texto,
        CancellationToken ct = default)
    {
        if (User.HasClaim(c => c.Type == SeguridadUsuarios.ClaimDebeCambiar))
            return StatusCode(403, new { mensaje = "Primero cambia tu contraseña temporal." });

        var filas = await Filtrar(desde, hasta, modulo, usuario, texto)
            .OrderByDescending(x => x.FechaUtc)
            .Take(20_000)
            .Select(x => new { x.FechaUtc, x.Usuario, x.Rol, x.Origen, x.Modulo, x.Accion, x.Detalle, x.Ip })
            .ToListAsync(ct);

        var csv = new StringBuilder();
        csv.AppendLine("Fecha;Hora;Usuario;Rol;Desde;Módulo;Acción;Detalle;IP");

        foreach (var f in filas)
        {
            var local = f.FechaUtc.ToOffset(ZonaBolivia);
            csv.AppendLine(string.Join(';',
                local.ToString("dd/MM/yyyy"),
                local.ToString("HH:mm:ss"),
                Celda(f.Usuario), Celda(f.Rol), Celda(f.Origen), Celda(f.Modulo),
                Celda(f.Accion), Celda(f.Detalle), Celda(f.Ip)));
        }

        // BOM para que Excel lea bien los acentos.
        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
        Response.Headers["Cache-Control"] = "no-store";
        return File(bytes, "text/csv; charset=utf-8", "registro-de-cambios.csv");
    }

    private IQueryable<Mercadex.Domain.Auditoria.RegistroCambio> Filtrar(
        DateOnly? desde, DateOnly? hasta, string? modulo, string? usuario, string? texto)
    {
        var consulta = _db.RegistroCambios.AsNoTracking().AsQueryable();

        if (desde is { } d)
        {
            var inicio = new DateTimeOffset(d.ToDateTime(TimeOnly.MinValue), ZonaBolivia).ToUniversalTime();
            consulta = consulta.Where(x => x.FechaUtc >= inicio);
        }

        if (hasta is { } h)
        {
            var fin = new DateTimeOffset(h.AddDays(1).ToDateTime(TimeOnly.MinValue), ZonaBolivia).ToUniversalTime();
            consulta = consulta.Where(x => x.FechaUtc < fin);
        }

        if (!string.IsNullOrWhiteSpace(modulo))
            consulta = consulta.Where(x => x.Modulo == modulo.Trim());

        if (!string.IsNullOrWhiteSpace(usuario))
            consulta = consulta.Where(x => x.Usuario == usuario.Trim());

        if (!string.IsNullOrWhiteSpace(texto))
        {
            var t = "%" + texto.Trim().Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_") + "%";
            consulta = consulta.Where(x =>
                EF.Functions.ILike(x.Accion, t) ||
                (x.Detalle != null && EF.Functions.ILike(x.Detalle, t)));
        }

        return consulta;
    }

    /// <summary>Celda CSV segura: entre comillas y sin fórmulas de Excel.</summary>
    private static string Celda(string? valor)
    {
        if (string.IsNullOrEmpty(valor))
            return "";

        var v = valor.Replace("\"", "\"\"");
        if (v[0] is '=' or '+' or '-' or '@')
            v = "'" + v;

        return $"\"{v}\"";
    }
}
