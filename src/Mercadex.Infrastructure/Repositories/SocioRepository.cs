using Mercadex.Application.DTOs;
using Mercadex.Application.Interfaces;
using Mercadex.Domain.Socios;
using Mercadex.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mercadex.Infrastructure.Repositories;

public sealed class SocioRepository : ISocioRepository
{
    private readonly MercadexDbContext _context;

    public SocioRepository(MercadexDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SocioDto>> GetAllAsync()
    {
        return await _context.Socios
            .AsNoTracking()
            .Select(s => new SocioDto(
                s.Id,
                s.Nombres,
                s.Apellidos,
                s.DocumentoIdentidad,
                s.Telefono,
                s.Activo
            ))
            .ToListAsync();
    }

    public async Task<SocioDto?> GetByIdAsync(Guid id)
    {
        return await _context.Socios
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new SocioDto(
                s.Id,
                s.Nombres,
                s.Apellidos,
                s.DocumentoIdentidad,
                s.Telefono,
                s.Activo
            ))
            .FirstOrDefaultAsync();
    }

    public async Task<SocioDto> CrearAsync(
        CrearSocioDto dto)
    {
        var socio = new Socio(
            dto.Nombres,
            dto.Apellidos,
            dto.DocumentoIdentidad,
            dto.Telefono
        );

        _context.Socios.Add(socio);

        await _context.SaveChangesAsync();

        return new SocioDto(
            socio.Id,
            socio.Nombres,
            socio.Apellidos,
            socio.DocumentoIdentidad,
            socio.Telefono,
            socio.Activo
        );
    }

    public async Task<SocioDto?> ActualizarAsync(
        Guid id,
        ActualizarSocioDto dto)
    {
        var socio = await _context.Socios
            .FirstOrDefaultAsync(s => s.Id == id);

        if (socio is null)
        {
            return null;
        }

        socio.CambiarNombre(
            dto.Nombres,
            dto.Apellidos);

        socio.CambiarTelefono(
            dto.Telefono);

        await _context.SaveChangesAsync();

        return new SocioDto(
            socio.Id,
            socio.Nombres,
            socio.Apellidos,
            socio.DocumentoIdentidad,
            socio.Telefono,
            socio.Activo
        );
    }

    public async Task<bool> DesactivarAsync(
        Guid id)
    {
        var socio = await _context.Socios
            .FirstOrDefaultAsync(s => s.Id == id);

        if (socio is null)
        {
            return false;
        }

        socio.Desactivar();

        await _context.SaveChangesAsync();

        return true;
    }
}