using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Mercadex.Domain.Usuarios;
using Microsoft.IdentityModel.Tokens;

namespace Mercadex.Api.Seguridad;

/// <summary>
/// Sesiones de Mercadex de escritorio y del panel de caja.
/// </summary>
public static class SeguridadUsuarios
{
    public const string AudienciaEscritorio = "Mercadex.Desktop";

    public const string RolAdministrador = nameof(RolUsuario.Administrador);
    public const string RolCajero = nameof(RolUsuario.Cajero);
    public const string RolConsulta = nameof(RolUsuario.Consulta);

    /// <summary>Identificador del usuario guardado en la base.</summary>
    public const string ClaimUsuario = "mx_uid";

    /// <summary>Versión de sesión del usuario guardado en la base.</summary>
    public const string ClaimVersion = "mx_uv";

    public const string ClaimNombreUsuario = "mx_usr";

    /// <summary>Presente mientras el usuario tiene una contraseña temporal.</summary>
    public const string ClaimDebeCambiar = "mx_dc";

    /// <summary>Identificador del administrador configurado en Render.</summary>
    public const string IdAdministradorPrincipal = "administrador";

    /// <summary>Un turno de trabajo; después se vuelve a ingresar.</summary>
    public static readonly TimeSpan DuracionSesion = TimeSpan.FromHours(12);

    public static (string Token, DateTime Expira) CrearToken(
        string id,
        string nombreUsuario,
        string nombreCompleto,
        RolUsuario rol,
        int? version,
        string claveJwt,
        bool debeCambiarContrasena = false)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, id),
            new(ClaimTypes.Name, nombreCompleto),
            new(ClaimTypes.Role, rol.ToString()),
            new(ClaimNombreUsuario, nombreUsuario)
        };

        if (version.HasValue)
        {
            claims.Add(new Claim(ClaimUsuario, id));
            claims.Add(new Claim(ClaimVersion, version.Value.ToString()));
        }

        if (debeCambiarContrasena)
            claims.Add(new Claim(ClaimDebeCambiar, "1"));

        var expira = DateTime.UtcNow.Add(DuracionSesion);

        var credenciales = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(claveJwt)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "Mercadex",
            audience: AudienciaEscritorio,
            claims: claims,
            expires: expira,
            signingCredentials: credenciales);

        return (new JwtSecurityTokenHandler().WriteToken(token), expira);
    }

    /// <summary>
    /// Contraseña temporal legible para dictarla o escribirla en un papel:
    /// sin letras que se confunden (l, 1, O, 0).
    /// </summary>
    public static string NuevaContrasenaTemporal()
    {
        const string letras = "abcdefghjkmnpqrstuvwxyz";
        const string numeros = "23456789";

        var texto = new StringBuilder();

        for (var i = 0; i < 6; i++)
            texto.Append(letras[RandomNumberGenerator.GetInt32(letras.Length)]);

        for (var i = 0; i < 4; i++)
            texto.Append(numeros[RandomNumberGenerator.GetInt32(numeros.Length)]);

        return texto.ToString();
    }

    public static bool CompararTexto(string? recibido, string esperado)
    {
        var a = SHA256.HashData(Encoding.UTF8.GetBytes(recibido ?? ""));
        var b = SHA256.HashData(Encoding.UTF8.GetBytes(esperado));
        return CryptographicOperations.FixedTimeEquals(a, b);
    }

    /// <summary>
    /// Identificador de quien registra un movimiento de caja (usuario de la
    /// base o "administrador"); null sin sesión o con una sesión de socio.
    /// </summary>
    public static string? IdResponsable(ClaimsPrincipal usuario) =>
        usuario.Identity?.IsAuthenticated == true && !usuario.IsInRole(SeguridadApp.RolSocio)
            ? usuario.FindFirstValue(ClaimTypes.NameIdentifier)
            : null;

    public static string NombreResponsable(ClaimsPrincipal usuario) =>
        usuario.Identity?.IsAuthenticated == true &&
        !string.IsNullOrWhiteSpace(usuario.Identity.Name)
            ? usuario.Identity.Name!
            : "Sistema";
}
