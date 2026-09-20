using Mercadex.Application.DTOs.Comercial;
using Mercadex.Application.Interfaces;
using Mercadex.Domain.Comercial;
using Mercadex.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mercadex.Infrastructure.Repositories;

public sealed class AsignacionSocioCasetaRepository
    : IAsignacionSocioCasetaRepository
{
    private readonly MercadexDbContext _context;


    public AsignacionSocioCasetaRepository(
        MercadexDbContext context)
    {
        _context = context;
    }


    public async Task<IReadOnlyList<AsignacionSocioCasetaDto>> ObtenerAsync()
    {
        return await _context.AsignacionesSocioCaseta
            .AsNoTracking()
            .Select(x => new AsignacionSocioCasetaDto(
                x.Id,
                x.SocioId,
                x.UnidadComercialId,
                x.FechaAsignacion,
                x.Activa))
            .ToListAsync();
    }


    public async Task<AsignacionSocioCasetaDto?> ObtenerPorIdAsync(
        Guid id)
    {
        return await _context.AsignacionesSocioCaseta
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new AsignacionSocioCasetaDto(
                x.Id,
                x.SocioId,
                x.UnidadComercialId,
                x.FechaAsignacion,
                x.Activa))
            .FirstOrDefaultAsync();
    }


    public async Task<AsignacionSocioCasetaDto> CrearAsync(
        CrearAsignacionSocioCasetaDto dto)
    {
        var asignacion = new AsignacionSocioCaseta(
            dto.SocioId,
            dto.UnidadComercialId,
            DateTimeOffset.UtcNow);


        _context.AsignacionesSocioCaseta.Add(asignacion);


        await _context.SaveChangesAsync();


        return new AsignacionSocioCasetaDto(
            asignacion.Id,
            asignacion.SocioId,
            asignacion.UnidadComercialId,
            asignacion.FechaAsignacion,
            asignacion.Activa);
    }
}