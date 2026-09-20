using Mercadex.Application.Interfaces;
using Mercadex.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mercadex.Infrastructure.Repositories;

public sealed class UnidadComercialRepository
    : IUnidadComercialRepository
{
    private readonly MercadexDbContext _context;


    public UnidadComercialRepository(
        MercadexDbContext context)
    {
        _context = context;
    }


    public async Task<IReadOnlyList<Guid>>
        ObtenerIdsActivosAsync()
    {
        return await _context.UnidadesComerciales
            .AsNoTracking()
            .Where(x => x.Activa)
            .Select(x => x.Id)
            .ToListAsync();
    }
}