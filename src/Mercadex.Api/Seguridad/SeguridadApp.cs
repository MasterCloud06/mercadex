using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Mercadex.Domain.AppSocios;
using Microsoft.IdentityModel.Tokens;

namespace Mercadex.Api.Seguridad;

/// <summary>
/// Huellas de contraseñas y códigos (PBKDF2-SHA256) y tokens de la app móvil.
/// </summary>
public static class SeguridadApp
{
    public const string AudienciaApp = "Mercadex.App";
    public const string RolSocio = "Socio";
    public const string ClaimVersion = "mx_sv";
    public const string ClaimCuenta = "mx_cid";

    private const int Iteraciones = 210_000;
    private const int LargoSal = 16;
    private const int LargoHuella = 32;

    public static readonly TimeSpan DuracionSesion = TimeSpan.FromDays(7);
    public static readonly TimeSpan DuracionCodigo = TimeSpan.FromDays(7);

    public static string Huella(string secreto)
    {
        var sal = RandomNumberGenerator.GetBytes(LargoSal);
        var huella = Rfc2898DeriveBytes.Pbkdf2(secreto, sal, Iteraciones, HashAlgorithmName.SHA256, LargoHuella);
        return $"pbkdf2-sha256${Iteraciones}${Convert.ToBase64String(sal)}${Convert.ToBase64String(huella)}";
    }

    public static bool Verificar(string secreto, string? guardado)
    {
        if (string.IsNullOrEmpty(guardado))
            return false;

        var partes = guardado.Split('$');
        if (partes.Length != 4 || partes[0] != "pbkdf2-sha256" || !int.TryParse(partes[1], out var iteraciones))
            return false;

        var sal = Convert.FromBase64String(partes[2]);
        var esperada = Convert.FromBase64String(partes[3]);
        var calculada = Rfc2898DeriveBytes.Pbkdf2(secreto, sal, iteraciones, HashAlgorithmName.SHA256, esperada.Length);
        return CryptographicOperations.FixedTimeEquals(calculada, esperada);
    }

    /// <summary>Código de activación de 8 dígitos.</summary>
    public static string NuevoCodigo() =>
        RandomNumberGenerator.GetInt32(0, 100_000_000).ToString("D8");

    /// <summary>Reglas mínimas: 8 caracteres, con letras y números.</summary>
    public static string? ProblemaContrasena(string? contrasena)
    {
        if (string.IsNullOrEmpty(contrasena) || contrasena.Length < 8)
            return "La contraseña debe tener al menos 8 caracteres.";
        if (contrasena.Length > 100)
            return "La contraseña es demasiado larga.";
        if (!contrasena.Any(char.IsLetter) || !contrasena.Any(char.IsDigit))
            return "La contraseña debe tener letras y números.";
        return null;
    }

    public static (string Token, DateTime Expira) CrearToken(CuentaSocioApp cuenta, string nombre, string claveJwt)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, cuenta.SocioId.ToString()),
            new Claim(ClaimTypes.Name, nombre),
            new Claim(ClaimTypes.Role, RolSocio),
            new Claim(ClaimCuenta, cuenta.Id.ToString()),
            new Claim(ClaimVersion, cuenta.VersionSesion.ToString())
        };

        var expira = DateTime.UtcNow.Add(DuracionSesion);
        var credenciales = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(claveJwt)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "Mercadex",
            audience: AudienciaApp,
            claims: claims,
            expires: expira,
            signingCredentials: credenciales);

        return (new JwtSecurityTokenHandler().WriteToken(token), expira);
    }
}
