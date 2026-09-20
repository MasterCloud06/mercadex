using Mercadex.Application.DTOs.Socios;
using Mercadex.Application.Interfaces;
using Mercadex.Domain.Socios;
using Mercadex.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mercadex.Infrastructure.Repositories;

public sealed class SocioRepository : ISocioRepository
{
    private readonly MercadexDbContext _context;


    public SocioRepository(
        MercadexDbContext context)
    {
        _context = context;
    }


    public async Task<IReadOnlyList<SocioDto>> ObtenerAsync()
    {
        return await _context.Socios
            .AsNoTracking()
            .Select(s => new SocioDto(
                s.Id,
                s.NombreCompleto,
                s.DocumentoIdentidad,
                s.Telefono,
                s.FechaRegistro,
                s.Activo))
            .ToListAsync();
    }


    public async Task<SocioDto?> ObtenerPorIdAsync(
        Guid id)
    {
        return await _context.Socios
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new SocioDto(
                s.Id,
                s.NombreCompleto,
                s.DocumentoIdentidad,
                s.Telefono,
                s.FechaRegistro,
                s.Activo))
            .FirstOrDefaultAsync();
    }


    public async Task<SocioDto> CrearAsync(
        CrearSocioDto dto)
    {
        var socio = new Socio(
            dto.Nombres,
            dto.Apellidos,
            dto.DocumentoIdentidad,
            dto.Telefono);


        _context.Socios.Add(socio);

        await _context.SaveChangesAsync();


        return new SocioDto(
            socio.Id,
            socio.NombreCompleto,
            socio.DocumentoIdentidad,
            socio.Telefono,
            socio.FechaRegistro,
            socio.Activo);
    }
}