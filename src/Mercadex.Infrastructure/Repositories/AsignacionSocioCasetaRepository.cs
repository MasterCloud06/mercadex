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
        // Validaciones de negocio: socio activo y unidad existente.
        var socio = await _context.Socios
            .AsNoTracking()
            .Where(x => x.Id == dto.SocioId)
            .Select(x => new { x.Id, x.Activo })
            .FirstOrDefaultAsync();

        if (socio is null)
            throw new ArgumentException("El socio seleccionado no existe.");

        if (!socio.Activo)
            throw new InvalidOperationException(
                "El socio esta inactivo. Activalo antes de asignarlo como titular.");

        var unidadExiste = await _context.UnidadesComerciales
            .AnyAsync(x => x.Id == dto.UnidadComercialId);

        if (!unidadExiste)
            throw new ArgumentException("La unidad comercial no existe.");

        // Una unidad tiene un solo titular activo.
        // Al cambiar de titular, la asignacion anterior se desactiva
        // (queda en el historial).
        var activas = await _context.AsignacionesSocioCaseta
            .Where(x =>
                x.UnidadComercialId == dto.UnidadComercialId &&
                x.Activa)
            .ToListAsync();

        var existente = activas.FirstOrDefault(x => x.SocioId == dto.SocioId);

        if (existente is not null && activas.Count == 1)
        {
            return new AsignacionSocioCasetaDto(
                existente.Id,
                existente.SocioId,
                existente.UnidadComercialId,
                existente.FechaAsignacion,
                existente.Activa);
        }

        foreach (var anterior in activas)
            anterior.Desactivar();

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
