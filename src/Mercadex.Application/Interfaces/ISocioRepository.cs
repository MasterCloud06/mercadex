using Mercadex.Application.DTOs.Socios;

namespace Mercadex.Application.Interfaces;

public interface ISocioRepository
{
    Task<IReadOnlyList<SocioDto>> ObtenerAsync();

    Task<SocioDto?> ObtenerPorIdAsync(
        Guid id);

    Task<SocioDto> CrearAsync(
        CrearSocioDto dto);
}