using Mercadex.Application.DTOs;

namespace Mercadex.Application.Interfaces;

public interface ISocioService
{
    Task<IEnumerable<SocioDto>> GetAllAsync();

    Task<SocioDto?> GetByIdAsync(
        Guid id);

    Task<SocioDto> CrearAsync(
        CrearSocioDto dto);

    Task<SocioDto?> ActualizarAsync(
        Guid id,
        ActualizarSocioDto dto);

    Task<bool> DesactivarAsync(
        Guid id);
}