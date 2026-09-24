
using Mercadex.Domain.Caja;
using Mercadex.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mercadex.Api.Controllers;

[ApiController]
[Route("api/ingresos/catalogos")]
public sealed class CatalogosIngresosController : ControllerBase
{
    private readonly MercadexDbContext _context;

    public CatalogosIngresosController(MercadexDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Obtener()
    {
        var formasPago = Enum.GetValues<FormaPago>()
            .Select(forma => new
            {
                Id = (int)forma,
                Nombre = forma.ToString()
            })
            .ToList();

        var conceptos = await _context.ConceptosCobro
            .AsNoTracking()
            .ToListAsync();

        var conceptosRespuesta = conceptos.Select(concepto =>
        {
            var tipo = concepto.GetType();

            var nombre =
                tipo.GetProperty("Nombre")?.GetValue(concepto)?.ToString()
                ?? tipo.GetProperty("Descripcion")?.GetValue(concepto)?.ToString()
                ?? tipo.GetProperty("Codigo")?.GetValue(concepto)?.ToString()
                ?? concepto.Id.ToString();

            return new
            {
                concepto.Id,
                Nombre = nombre
            };
        });

        return Ok(new
        {
            FormasPago = formasPago,
            Conceptos = conceptosRespuesta
        });
    }
}
