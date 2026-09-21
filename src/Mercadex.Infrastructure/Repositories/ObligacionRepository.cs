using Mercadex.Application.DTOs;
using Mercadex.Application.Interfaces;
using Mercadex.Domain.Cobranza;
using Mercadex.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mercadex.Infrastructure.Repositories;

public sealed class ObligacionRepository : IObligacionRepository
{
    private readonly MercadexDbContext _context;

    public ObligacionRepository(
        MercadexDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ObligacionDto>> GetAllAsync()
    {
        return await _context.Obligaciones
            .AsNoTracking()
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

    public async Task<ObligacionDto?> GetByIdAsync(Guid id)
    {
        return await _context.Obligaciones
            .AsNoTracking()
            .Where(o => o.Id == id)
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
            .FirstOrDefaultAsync();
    }

    public async Task<ObligacionDto> CrearAsync(
        CrearObligacionDto dto)
    {
        var obligacion = new Obligacion(
            dto.UnidadComercialId,
            dto.ConceptoCobroId,
            dto.MonedaId,
            dto.Monto,
            dto.FechaEmision,
            dto.FechaVencimiento,
            dto.PeriodoAnio,
            dto.PeriodoMes
        );

        _context.Obligaciones.Add(obligacion);

        await _context.SaveChangesAsync();

        return new ObligacionDto(
            obligacion.Id,
            obligacion.UnidadComercialId,
            obligacion.ConceptoCobroId,
            obligacion.MonedaId,
            obligacion.Monto,
            obligacion.FechaEmision,
            obligacion.FechaVencimiento,
            obligacion.PeriodoAnio,
            obligacion.PeriodoMes,
            obligacion.Estado.ToString()
        );
    }

public async Task<IReadOnlyList<Obligacion>> ObtenerPorIdsAsync(
    IEnumerable<Guid> ids)
{
    var listaIds = ids.ToList();

    return await _context.Obligaciones
        .Where(o => listaIds.Contains(o.Id))
        .ToListAsync();
}

    public async Task<bool> MarcarComoPagadaAsync(Guid id)
    {
        var obligacion = await _context.Obligaciones
            .FirstOrDefaultAsync(o => o.Id == id);

        if (obligacion is null)
        {
            return false;
        }

        obligacion.MarcarComoPagada();

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RestaurarComoPendienteAsync(Guid id)
    {
        var obligacion = await _context.Obligaciones
            .FirstOrDefaultAsync(o => o.Id == id);

        if (obligacion is null)
        {
            return false;
        }

        obligacion.RestaurarComoPendiente();

        await _context.SaveChangesAsync();

        return true;
    }
public async Task<bool> ExisteAsync(
    Guid unidadComercialId,
    Guid conceptoCobroId,
    int periodoAnio,
    int periodoMes)
{
    return await _context.Obligaciones
        .AsNoTracking()
        .AnyAsync(o =>
            o.UnidadComercialId == unidadComercialId &&
            o.ConceptoCobroId == conceptoCobroId &&
            o.PeriodoAnio == periodoAnio &&
            o.PeriodoMes == periodoMes);
}
}