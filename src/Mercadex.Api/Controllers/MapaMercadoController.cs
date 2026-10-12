using Mercadex.Domain.Cobranza;
using Mercadex.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mercadex.Api.Controllers;

/// <summary>
/// Mapa del mercado: todas las unidades agrupadas por bloque con su
/// titular y su situación de deuda. Solo lectura. Las deudas se
/// informan por moneda, sin conversión.
/// </summary>
[ApiController]
[Route("api/mapa-mercado")]
public sealed class MapaMercadoController : ControllerBase
{
    private static readonly TimeSpan ZonaBolivia = TimeSpan.FromHours(-4);

    private readonly MercadexDbContext _db;

    public MapaMercadoController(MercadexDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Obtener(CancellationToken ct)
    {
        var hoy = DateOnly.FromDateTime(DateTimeOffset.UtcNow.ToOffset(ZonaBolivia).DateTime);

        var monedas = await _db.Monedas.AsNoTracking()
            .ToDictionaryAsync(m => m.Id, m => m.Codigo, ct);

        var unidades = await _db.UnidadesComerciales.AsNoTracking()
            .Select(u => new { u.Id, u.Codigo, u.Bloque, Tipo = (int)u.Tipo, u.Activa })
            .ToListAsync(ct);

        var titulares = await (
                from a in _db.AsignacionesSocioCaseta.AsNoTracking()
                join s in _db.Socios.AsNoTracking() on a.SocioId equals s.Id
                where a.Activa
                select new { a.UnidadComercialId, Nombre = s.Nombres + " " + s.Apellidos })
            .ToListAsync(ct);

        var titularPorUnidad = titulares
            .GroupBy(t => t.UnidadComercialId)
            .ToDictionary(g => g.Key, g => g.First().Nombre);

        var pendientes = await _db.Obligaciones.AsNoTracking()
            .Where(o => o.Estado == EstadoObligacion.Pendiente)
            .Select(o => new { o.UnidadComercialId, o.MonedaId, o.Monto, o.FechaEmision })
            .ToListAsync(ct);

        var deudaPorUnidad = pendientes
            .GroupBy(o => o.UnidadComercialId)
            .ToDictionary(g => g.Key, g => g.ToList());

        static string Situacion(bool activa, int obligaciones, int dias) =>
            !activa ? "inactiva"
            : obligaciones == 0 ? "al-dia"
            : dias <= 30 ? "reciente"
            : dias <= 90 ? "atrasada"
            : "critica";

        var resultado = unidades
            .Select(u =>
            {
                deudaPorUnidad.TryGetValue(u.Id, out var lista);
                lista ??= [];

                var dias = lista.Count == 0
                    ? 0
                    : hoy.DayNumber - lista.Min(o => o.FechaEmision).DayNumber;

                return new
                {
                    id = u.Id,
                    codigo = u.Codigo,
                    bloque = string.IsNullOrWhiteSpace(u.Bloque) ? "Sin bloque" : u.Bloque.Trim(),
                    tipo = u.Tipo,
                    activa = u.Activa,
                    titular = titularPorUnidad.GetValueOrDefault(u.Id),
                    obligaciones = lista.Count,
                    diasDeuda = dias,
                    situacion = Situacion(u.Activa, lista.Count, dias),
                    deudas = lista
                        .GroupBy(o => o.MonedaId)
                        .Select(g => new { moneda = monedas.GetValueOrDefault(g.Key, "?"), total = g.Sum(o => o.Monto) })
                        .OrderBy(d => d.moneda == "BOB" ? 0 : 1)
                        .ToList()
                };
            })
            .GroupBy(u => u.bloque)
            .OrderBy(g => g.Key == "Sin bloque" ? 1 : 0)
            .ThenBy(g => g.Key)
            .Select(g => new
            {
                bloque = g.Key,
                unidades = g
                    .OrderBy(u => u.tipo)
                    .ThenBy(u => u.codigo.Length)
                    .ThenBy(u => u.codigo)
                    .ToList()
            })
            .ToList();

        Response.Headers["Cache-Control"] = "no-store";

        return Ok(new
        {
            generado = DateTimeOffset.UtcNow.ToOffset(ZonaBolivia),
            bloques = resultado
        });
    }
}
