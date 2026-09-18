using Mercadex.Application.DTOs;
using Mercadex.Application.Interfaces;
using Mercadex.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mercadex.Infrastructure.Repositories;

public sealed class ConsultaCajaRepository 
    : IConsultaCajaRepository
{
    private readonly MercadexDbContext _context;


    public ConsultaCajaRepository(
        MercadexDbContext context)
    {
        _context = context;
    }


    public async Task<IEnumerable<ObligacionDto>> 
        GetDeudasPorCasetaAsync(Guid unidadComercialId)
    {
        return await _context.Obligaciones
            .AsNoTracking()
            .Where(o =>
                o.UnidadComercialId == unidadComercialId &&
                o.Estado.ToString() == "Pendiente")
            .Select(o => new ObligacionDto(
                o.Id,
                o.UnidadComercialId,
                o.ConceptoCobroId,
                o.MonedaId,
                o.Monto,
                o.FechaEmision,
                o.FechaVencimiento,
                o.PeriodoAnio,
                o.PeriodoMes,
                o.Estado.ToString()
            ))
            .ToListAsync();
    }
}