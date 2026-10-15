using System.Security.Claims;
using Mercadex.Api.Seguridad;
using Mercadex.Domain.Usuarios;
using Mercadex.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mercadex.Api.Controllers;

/// <summary>
/// Usuarios de Mercadex de escritorio y del panel. Solo administradores.
/// Las contraseñas temporales se muestran una sola vez; el usuario elige
/// la suya al ingresar por primera vez.
/// </summary>
[ApiController]
[Route("api/usuarios")]
[Authorize(Roles = SeguridadUsuarios.RolAdministrador)]
public sealed class UsuariosController : ControllerBase
{
    private readonly MercadexDbContext _db;
    private readonly IConfiguration _config;

    public UsuariosController(MercadexDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public sealed record CrearUsuarioRequest(string? NombreUsuario, string? NombreCompleto, string? Rol, string? Cargo = null);

    public sealed record ActualizarUsuarioRequest(string? NombreCompleto, string? Rol, string? Cargo = null);

    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        if (DebeCambiar())
            return CambiarPrimero();

        var ahora = DateTimeOffset.UtcNow;

        var usuarios = await _db.Usuarios.AsNoTracking()
            .OrderBy(u => u.Activo ? 0 : 1)
            .ThenBy(u => u.NombreCompleto)
            .ToListAsync(ct);

        Response.Headers["Cache-Control"] = "no-store";

        return Ok(usuarios.Select(u => new
        {
            id = u.Id,
            nombreUsuario = u.NombreUsuario,
            nombreCompleto = u.NombreCompleto,
            rol = u.Rol.ToString(),
            cargo = u.Cargo,
            activo = u.Activo,
            bloqueado = u.EstaBloqueado(ahora),
            debeCambiarContrasena = u.DebeCambiarContrasena,
            ultimoAcceso = u.UltimoAccesoUtc,
            fechaCreacion = u.FechaCreacionUtc
        }));
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearUsuarioRequest? r, CancellationToken ct)
    {
        if (DebeCambiar())
            return CambiarPrimero();

        if (!TryRol(r?.Rol, out var rol))
            return BadRequest(new { mensaje = "Elige el rol: Administrador, Cajero o Consulta." });

        string nombreUsuario;
        try
        {
            nombreUsuario = Usuario.NormalizarNombreUsuario(r?.NombreUsuario);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = Mensaje(ex) });
        }

        var adminPrincipal = _config["MERCADEX_ADMIN_USER"]?.Trim().ToLowerInvariant();
        if (nombreUsuario == adminPrincipal)
            return Conflict(new { mensaje = "Ese nombre de usuario ya lo usa el administrador principal." });

        if (await _db.Usuarios.AnyAsync(u => u.NombreUsuario == nombreUsuario, ct))
            return Conflict(new { mensaje = $"Ya existe el usuario «{nombreUsuario}»." });

        var temporal = SeguridadUsuarios.NuevaContrasenaTemporal();

        Usuario usuario;
        try
        {
            usuario = new Usuario(nombreUsuario, r?.NombreCompleto ?? "", rol, SeguridadApp.Huella(temporal), DateTimeOffset.UtcNow);
            usuario.AsignarCargo(r?.Cargo);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = Mensaje(ex) });
        }

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync(ct);

        return Ok(new
        {
            id = usuario.Id,
            nombreUsuario = usuario.NombreUsuario,
            nombreCompleto = usuario.NombreCompleto,
            rol = usuario.Rol.ToString(),
            contrasenaTemporal = temporal
        });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Actualizar(Guid id, [FromBody] ActualizarUsuarioRequest? r, CancellationToken ct)
    {
        if (DebeCambiar())
            return CambiarPrimero();

        if (!TryRol(r?.Rol, out var rol))
            return BadRequest(new { mensaje = "Elige el rol: Administrador, Cajero o Consulta." });

        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (usuario is null)
            return NotFound(new { mensaje = "Usuario no encontrado." });

        if (usuario.Rol == RolUsuario.Administrador && rol != RolUsuario.Administrador && EsUnoMismo(usuario))
            return BadRequest(new { mensaje = "No puedes quitarte el rol de administrador a ti mismo." });

        try
        {
            usuario.Actualizar(r?.NombreCompleto ?? "", rol);
            usuario.AsignarCargo(r?.Cargo);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = Mensaje(ex) });
        }

        await _db.SaveChangesAsync(ct);
        return Ok(new { mensaje = "Usuario actualizado." });
    }

    /// <summary>Contraseña temporal nueva. Cierra las sesiones del usuario y lo desbloquea.</summary>
    [HttpPost("{id:guid}/restablecer")]
    public async Task<IActionResult> Restablecer(Guid id, CancellationToken ct)
    {
        if (DebeCambiar())
            return CambiarPrimero();

        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (usuario is null)
            return NotFound(new { mensaje = "Usuario no encontrado." });

        if (EsUnoMismo(usuario))
            return BadRequest(new { mensaje = "Para cambiar tu propia contraseña usa «Cambiar mi contraseña»." });

        var temporal = SeguridadUsuarios.NuevaContrasenaTemporal();
        usuario.RestablecerContrasena(SeguridadApp.Huella(temporal));
        await _db.SaveChangesAsync(ct);

        return Ok(new
        {
            nombreUsuario = usuario.NombreUsuario,
            nombreCompleto = usuario.NombreCompleto,
            contrasenaTemporal = temporal
        });
    }

    [HttpPost("{id:guid}/desactivar")]
    public async Task<IActionResult> Desactivar(Guid id, CancellationToken ct)
    {
        if (DebeCambiar())
            return CambiarPrimero();

        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (usuario is null)
            return NotFound(new { mensaje = "Usuario no encontrado." });

        if (EsUnoMismo(usuario))
            return BadRequest(new { mensaje = "No puedes desactivar tu propio usuario." });

        usuario.Desactivar();
        await _db.SaveChangesAsync(ct);
        return Ok(new { mensaje = $"{usuario.NombreCompleto} ya no puede ingresar." });
    }

    [HttpPost("{id:guid}/activar")]
    public async Task<IActionResult> Activar(Guid id, CancellationToken ct)
    {
        if (DebeCambiar())
            return CambiarPrimero();

        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (usuario is null)
            return NotFound(new { mensaje = "Usuario no encontrado." });

        usuario.Activar();
        await _db.SaveChangesAsync(ct);
        return Ok(new { mensaje = $"{usuario.NombreCompleto} puede volver a ingresar." });
    }

    private bool EsUnoMismo(Usuario usuario) =>
        User.FindFirstValue(SeguridadUsuarios.ClaimUsuario) == usuario.Id.ToString();

    private bool DebeCambiar() =>
        User.HasClaim(c => c.Type == SeguridadUsuarios.ClaimDebeCambiar);

    private ObjectResult CambiarPrimero() =>
        StatusCode(403, new { mensaje = "Primero cambia tu contraseña temporal." });

    private static bool TryRol(string? texto, out RolUsuario rol) =>
        Enum.TryParse(texto?.Trim(), ignoreCase: true, out rol) && Enum.IsDefined(rol);

    /// <summary>Mensaje de una validación del dominio sin el nombre del parámetro.</summary>
    private static string Mensaje(ArgumentException ex) =>
        ex.ParamName is null ? ex.Message : ex.Message.Replace($" (Parameter '{ex.ParamName}')", "");
}
