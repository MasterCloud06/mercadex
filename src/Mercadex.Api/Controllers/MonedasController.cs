
using Mercadex.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mercadex.Api.Controllers;

[ApiController]
[Route("api/monedas")]
public sealed class MonedasController : ControllerBase
{
    private readonly MercadexDbContext _context;

    public MonedasController(MercadexDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Obtener()
    {
        var monedas = await _context.Monedas
            .AsNoTracking()
            .Where(m => m.Activa)
            .OrderBy(m => m.Codigo)
            .Select(m => new
            {
                m.Id,
                m.Codigo,
                m.Nombre,
                m.Simbolo
            })
            .ToListAsync();

        return Ok(monedas);
    }
}
