using Mercadex.Application.DTOs.Socios;

namespace Mercadex.Application.Interfaces;

public interface ISocioService
{
    Task<IReadOnlyList<SocioDto>> ObtenerAsync();


    Task<SocioDto?> ObtenerPorIdAsync(
        Guid id);


    Task<SocioDto> CrearAsync(
        CrearSocioDto dto);
}