using Mercadex.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mercadex.Api.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    private readonly MercadexDbContext _dbContext;

    public HealthController(
        MercadexDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        CancellationToken cancellationToken)
    {
        var databaseDisponible = false;
        var migracionesPendientes = Array.Empty<string>();

        try
        {
            databaseDisponible =
                await _dbContext.Database.CanConnectAsync(
                    cancellationToken);

            if (databaseDisponible)
            {
                migracionesPendientes =
                    (await _dbContext.Database
                        .GetPendingMigrationsAsync(
                            cancellationToken))
                    .ToArray();
            }
        }
        catch
        {
            databaseDisponible = false;
        }

        var resultado = new
        {
            status = databaseDisponible
                ? "ok"
                : "degraded",

            sistema = "Mercadex",
            servicio = "Mercadex API",

            api = new
            {
                disponible = true
            },

            database = new
            {
                disponible = databaseDisponible,
                proveedor = "PostgreSQL"
            },

            migrations = new
            {
                pendientes = migracionesPendientes.Length,
                nombres = migracionesPendientes
            },

            utc = DateTimeOffset.UtcNow
        };

        if (!databaseDisponible)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                resultado);
        }

        return Ok(resultado);
    }
}