
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;

namespace Mercadex.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IConfiguration _config;

    public AuthController(IConfiguration config)
    {
        _config = config;
    }

    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var usuario = _config["MERCADEX_ADMIN_USER"];
        var password = _config["MERCADEX_ADMIN_PASSWORD"];
        var jwtKey = _config["MERCADEX_JWT_KEY"];

        // Comprobar que el administrador esté configurado.

        if (string.IsNullOrWhiteSpace(usuario) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(jwtKey))
        {
            return StatusCode(
                503,
                new
                {
                    mensaje = "Autenticación no configurada."
                });
        }

        // Comprobar las credenciales.

        if (request is null ||
            !Comparar(request.Usuario, usuario) ||
            !Comparar(request.Password, password))
        {
            return Unauthorized(
                new
                {
                    mensaje = "Usuario o contraseña incorrectos."
                });
        }

        // Identidad y permisos del administrador.

        var claims = new[]
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                "administrador"),

            new Claim(
                ClaimTypes.Name,
                usuario),

            new Claim(
                ClaimTypes.Role,
                "Administrador")
        };

        // Crear el token JWT.

        var llave = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey));

        var credenciales = new SigningCredentials(
            llave,
            SecurityAlgorithms.HmacSha256);

        var vencimiento = DateTime.UtcNow.AddHours(2);

        var token = new JwtSecurityToken(
            issuer: "Mercadex",
            audience: "Mercadex.Desktop",
            claims: claims,
            expires: vencimiento,
            signingCredentials: credenciales);

        var tokenTexto = new JwtSecurityTokenHandler()
            .WriteToken(token);

        return Ok(new
        {
            token = tokenTexto,
            usuario = usuario,
            rol = "Administrador",
            expira = vencimiento
        });
    }

    private static bool Comparar(
        string? recibido,
        string esperado)
    {
        var a = SHA256.HashData(
            Encoding.UTF8.GetBytes(recibido ?? ""));

        var b = SHA256.HashData(
            Encoding.UTF8.GetBytes(esperado));

        return CryptographicOperations.FixedTimeEquals(a, b);
    }
}

public sealed class LoginRequest
{
    public string Usuario { get; set; } = "";

    public string Password { get; set; } = "";
}
