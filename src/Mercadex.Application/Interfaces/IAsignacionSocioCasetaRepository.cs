using Mercadex.Application.DTOs.Comercial;

namespace Mercadex.Application.Interfaces;

public interface IAsignacionSocioCasetaRepository
{
    Task<IReadOnlyList<AsignacionSocioCasetaDto>> ObtenerAsync();


    Task<AsignacionSocioCasetaDto?> ObtenerPorIdAsync(
        Guid id);


    Task<AsignacionSocioCasetaDto> CrearAsync(
        CrearAsignacionSocioCasetaDto dto);
}