using System.Security.Claims;
using Mercadex.Domain.Usuarios;
using Mercadex.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mercadex.Api.Seguridad;

/// <summary>
/// Quién emite un recibo y con qué cargo (observación de la directiva:
/// el recibo debe mostrar el nombre completo y el cargo de quien lo emite).
/// El cargo se toma de la base al momento de cobrar, así un cambio de cargo
/// no exige volver a ingresar y queda guardado tal cual en el recibo.
/// </summary>
public static class EmisorRecibo
{
    public static async Task<(string? Nombre, string? Cargo)> ObtenerAsync(
        ClaimsPrincipal usuario,
        MercadexDbContext db,
        CancellationToken ct = default)
    {
        if (usuario.Identity?.IsAuthenticated != true)
            return (null, null);

        var nombre = usuario.Identity.Name?.Trim();

        // Usuario propio de Mercadex: nombre y cargo actuales de la base.
        if (Guid.TryParse(usuario.FindFirstValue(SeguridadUsuarios.ClaimUsuario), out var id))
        {
            var u = await db.Usuarios.AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.NombreCompleto, x.Cargo, x.Rol })
                .FirstOrDefaultAsync(ct);

            if (u is not null)
                return (u.NombreCompleto, string.IsNullOrWhiteSpace(u.Cargo) ? Usuario.TextoRol(u.Rol) : u.Cargo);
        }

        // Administrador principal (configurado en el servidor).
        var rol = usuario.FindFirstValue(ClaimTypes.Role);
        return (nombre, string.IsNullOrWhiteSpace(rol) ? null : rol);
    }
}
