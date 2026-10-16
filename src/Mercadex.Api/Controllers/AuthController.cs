using System.Security.Claims;
using Mercadex.Api.Seguridad;
using Mercadex.Domain.Usuarios;
using Mercadex.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Mercadex.Api.Controllers;

/// <summary>
/// Inicio de sesión de Mercadex de escritorio y del panel de caja.
/// Primero se buscan los usuarios guardados en la base; el administrador
/// configurado en Render (MERCADEX_ADMIN_USER / MERCADEX_ADMIN_PASSWORD)
/// sigue funcionando como administrador principal, para crear a los demás
/// y para recuperar el acceso si se olvidan todas las contraseñas.
/// </summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private const string MensajeCredenciales = "Usuario o contraseña incorrectos.";

    private readonly IConfiguration _config;
    private readonly MercadexDbContext _db;

    public AuthController(IConfiguration config, MercadexDbContext db)
    {
        _config = config;
        _db = db;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest? request, CancellationToken ct)
    {
        var jwtKey = _config["MERCADEX_JWT_KEY"];

        if (string.IsNullOrWhiteSpace(jwtKey))
            return StatusCode(503, new { mensaje = "Autenticación no configurada." });

        var nombreUsuario = (request?.Usuario ?? "").Trim().ToLowerInvariant();
        var contrasena = request?.Password ?? "";

        if (nombreUsuario.Length == 0 || contrasena.Length == 0)
            return BadRequest(new { mensaje = "Escribe tu usuario y tu contraseña." });

        var ahora = DateTimeOffset.UtcNow;

        // ---------- Usuarios guardados en la base ----------

        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario, ct);

        if (usuario is not null)
        {
            if (usuario.EstaBloqueado(ahora))
            {
                var minutos = (int)Math.Ceiling((usuario.BloqueadoHastaUtc!.Value - ahora).TotalMinutes);
                return StatusCode(429, new
                {
                    mensaje = $"Demasiados intentos fallidos. Vuelve a intentar en {minutos} minuto(s)."
                });
            }

            if (!SeguridadApp.Verificar(contrasena, usuario.ContrasenaHash))
            {
                usuario.RegistrarFallo(ahora);
                Cambios.Agregar(_db, HttpContext, "Sesión", "Contraseña incorrecta",
                    usuario.EstaBloqueado(ahora)
                        ? $"Usuario «{usuario.NombreUsuario}». Quedó bloqueado por varios intentos fallidos."
                        : $"Usuario «{usuario.NombreUsuario}»",
                    usuario: usuario.NombreCompleto, usuarioId: usuario.Id.ToString(), rol: usuario.Rol.ToString());
                await _db.SaveChangesAsync(ct);
                return Unauthorized(new { mensaje = MensajeCredenciales });
            }

            if (!usuario.Activo)
                return Unauthorized(new { mensaje = "Tu usuario está desactivado. Habla con el administrador." });

            usuario.RegistrarAcceso(ahora);
            Cambios.Agregar(_db, HttpContext, "Sesión", "Ingresó al sistema", null,
                usuario: usuario.NombreCompleto, usuarioId: usuario.Id.ToString(), rol: usuario.Rol.ToString());
            await _db.SaveChangesAsync(ct);

            var (token, expira) = SeguridadUsuarios.CrearToken(
                usuario.Id.ToString(),
                usuario.NombreUsuario,
                usuario.NombreCompleto,
                usuario.Rol,
                usuario.VersionSesion,
                jwtKey,
                usuario.DebeCambiarContrasena);

            return Ok(new
            {
                token,
                usuario = usuario.NombreUsuario,
                nombre = usuario.NombreCompleto,
                rol = usuario.Rol.ToString(),
                expira,
                debeCambiarContrasena = usuario.DebeCambiarContrasena,
                principal = false
            });
        }

        // ---------- Administrador principal (Render) ----------

        var adminUsuario = _config["MERCADEX_ADMIN_USER"];
        var adminContrasena = _config["MERCADEX_ADMIN_PASSWORD"];

        if (!string.IsNullOrWhiteSpace(adminUsuario) &&
            !string.IsNullOrWhiteSpace(adminContrasena) &&
            SeguridadUsuarios.CompararTexto(nombreUsuario, adminUsuario.Trim().ToLowerInvariant()) &&
            SeguridadUsuarios.CompararTexto(contrasena, adminContrasena))
        {
            var nombre = string.IsNullOrWhiteSpace(_config["MERCADEX_ADMIN_NOMBRE"])
                ? "Administrador"
                : _config["MERCADEX_ADMIN_NOMBRE"]!.Trim();

            var (token, expira) = SeguridadUsuarios.CrearToken(
                SeguridadUsuarios.IdAdministradorPrincipal,
                adminUsuario.Trim().ToLowerInvariant(),
                nombre,
                RolUsuario.Administrador,
                version: null,
                jwtKey);

            Cambios.Agregar(_db, HttpContext, "Sesión", "Ingresó al sistema", "Administrador principal",
                usuario: nombre, usuarioId: SeguridadUsuarios.IdAdministradorPrincipal,
                rol: RolUsuario.Administrador.ToString());
            await _db.SaveChangesAsync(ct);

            return Ok(new
            {
                token,
                usuario = adminUsuario.Trim().ToLowerInvariant(),
                nombre,
                rol = RolUsuario.Administrador.ToString(),
                expira,
                debeCambiarContrasena = false,
                principal = true
            });
        }

        // Usuario que no existe: también queda registrado (posible intento de adivinar).
        Cambios.Agregar(_db, HttpContext, "Sesión", "Intento con usuario inexistente",
            $"Usuario «{(nombreUsuario.Length > 60 ? nombreUsuario[..60] : nombreUsuario)}»");
        await _db.SaveChangesAsync(ct);

        return Unauthorized(new { mensaje = MensajeCredenciales });
    }

    /// <summary>Datos de la sesión actual.</summary>
    [HttpGet("yo")]
    [Authorize(Roles = $"{SeguridadUsuarios.RolAdministrador},{SeguridadUsuarios.RolCajero},{SeguridadUsuarios.RolConsulta}")]
    public IActionResult Yo()
    {
        return Ok(new
        {
            usuario = User.FindFirstValue(SeguridadUsuarios.ClaimNombreUsuario),
            nombre = User.Identity?.Name,
            rol = User.FindFirstValue(ClaimTypes.Role),
            principal = User.FindFirstValue(ClaimTypes.NameIdentifier) == SeguridadUsuarios.IdAdministradorPrincipal
        });
    }

    public sealed record CambiarContrasenaRequest(string? Actual, string? Nueva);

    /// <summary>El usuario cambia su propia contraseña. Cierra sus otras sesiones.</summary>
    [HttpPost("contrasena")]
    [Authorize(Roles = $"{SeguridadUsuarios.RolAdministrador},{SeguridadUsuarios.RolCajero},{SeguridadUsuarios.RolConsulta}")]
    public async Task<IActionResult> CambiarContrasena([FromBody] CambiarContrasenaRequest? r, CancellationToken ct)
    {
        var jwtKey = _config["MERCADEX_JWT_KEY"]!;

        if (!Guid.TryParse(User.FindFirstValue(SeguridadUsuarios.ClaimUsuario), out var id))
        {
            return BadRequest(new
            {
                mensaje = "La contraseña del administrador principal se cambia en Render (MERCADEX_ADMIN_PASSWORD)."
            });
        }

        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == id, ct);

        if (usuario is null || !usuario.Activo)
            return Unauthorized(new { mensaje = "Tu sesión terminó. Vuelve a ingresar." });

        if (!SeguridadApp.Verificar(r?.Actual ?? "", usuario.ContrasenaHash))
            return BadRequest(new { mensaje = "La contraseña actual no es correcta." });

        var problema = SeguridadApp.ProblemaContrasena(r?.Nueva);
        if (problema is not null)
            return BadRequest(new { mensaje = problema });

        if (SeguridadApp.Verificar(r!.Nueva!, usuario.ContrasenaHash))
            return BadRequest(new { mensaje = "La contraseña nueva debe ser distinta de la actual." });

        usuario.CambiarContrasena(SeguridadApp.Huella(r.Nueva!));
        await _db.SaveChangesAsync(ct);

        // Nueva sesión con la versión actualizada.
        var (token, expira) = SeguridadUsuarios.CrearToken(
            usuario.Id.ToString(),
            usuario.NombreUsuario,
            usuario.NombreCompleto,
            usuario.Rol,
            usuario.VersionSesion,
            jwtKey);

        return Ok(new
        {
            mensaje = "Contraseña actualizada.",
            token,
            usuario = usuario.NombreUsuario,
            nombre = usuario.NombreCompleto,
            rol = usuario.Rol.ToString(),
            expira,
            debeCambiarContrasena = false,
            principal = false
        });
    }
}

public sealed class LoginRequest
{
    public string Usuario { get; set; } = "";

    public string Password { get; set; } = "";
}
