using System.Security.Cryptography;
using System.Text;

namespace Mercadex.Api.Middleware;

/// <summary>
/// Exige la clave interna de la aplicación (encabezado X-Mercadex-Key)
/// cuando la variable MERCADEX_API_KEY está configurada en el servidor.
/// Sin la variable (desarrollo local) no se exige nada.
/// /api/health queda libre para el monitoreo de Render.
/// /api/app/* (app móvil de socios) no usa esta clave: una app publicada
/// en tiendas no puede guardar secretos; se protege con la cuenta del socio.
/// </summary>
public sealed class ClaveAppMiddleware
{
    public const string Encabezado = "X-Mercadex-Key";

    private readonly RequestDelegate _next;
    private readonly byte[]? _clave;

    public ClaveAppMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;

        var clave = configuration["MERCADEX_API_KEY"];
        _clave = string.IsNullOrWhiteSpace(clave)
            ? null
            : Encoding.UTF8.GetBytes(clave.Trim());
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (_clave is null ||
            context.Request.Path.StartsWithSegments("/api/health") ||
            context.Request.Path.StartsWithSegments("/api/app"))
        {
            await _next(context);
            return;
        }

        var recibida = context.Request.Headers[Encabezado].ToString();

        if (string.IsNullOrEmpty(recibida) ||
            !CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(recibida.Trim()), _clave))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                mensaje = "Aplicación no autorizada. Actualice Mercadex a la última versión."
            });
            return;
        }

        await _next(context);
    }
}
