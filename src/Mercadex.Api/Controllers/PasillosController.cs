
using Mercadex.Domain.Organizacion;
using Mercadex.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Mercadex.Api.Controllers;

[ApiController]
[Route("api/pasillos")]
public sealed class PasillosController : ControllerBase
{
    private readonly MercadexDbContext _context;

    public PasillosController(MercadexDbContext context)
    {
        _context = context;
    }

    // Consultar todos los pasillos
    [HttpGet]
    public async Task<IActionResult> Obtener()
    {
        var pasillos = await _context.Pasillos
            .AsNoTracking()
            .OrderBy(p => p.Codigo)
            .Select(p => new
            {
                p.Id,
                p.Codigo,
                p.Nombre,
                p.Activo
            })
            .ToListAsync();

        return Ok(pasillos);
    }

    // Registrar un nuevo pasillo
    [HttpPost]
    public async Task<IActionResult> Crear(
        [FromBody] CrearPasilloRequest solicitud)
    {
        if (string.IsNullOrWhiteSpace(solicitud.Codigo) ||
            string.IsNullOrWhiteSpace(solicitud.Nombre))
        {
            return BadRequest(new
            {
                mensaje = "El código y el nombre son obligatorios."
            });
        }

        var codigo = solicitud.Codigo.Trim().ToUpperInvariant();
        var nombre = solicitud.Nombre.Trim();

        if (codigo.Length > 30 || nombre.Length > 100)
        {
            return BadRequest(new
            {
                mensaje = "El código o el nombre supera la longitud permitida."
            });
        }

        var existe = await _context.Pasillos
            .AnyAsync(p => p.Codigo == codigo);

        if (existe)
        {
            return Conflict(new
            {
                mensaje = "Ya existe un pasillo con ese código."
            });
        }

        var pasillo = new Pasillo(codigo, nombre);

        _context.Pasillos.Add(pasillo);

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
                mensaje = "Ya existe un pasillo con ese código."
            });
        }

        return Ok(new
        {
            pasillo.Id,
            pasillo.Codigo,
            pasillo.Nombre,
            pasillo.Activo
        });
    }
}

public sealed record CrearPasilloRequest(
    string Codigo,
    string Nombre
);
