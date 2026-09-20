using Mercadex.Application.DTOs.Comercial;
using Mercadex.Application.Interfaces;

namespace Mercadex.Application.Services;

public sealed class AsignacionSocioCasetaService
    : IAsignacionSocioCasetaService
{
    private readonly IAsignacionSocioCasetaRepository _repository;


    public AsignacionSocioCasetaService(
        IAsignacionSocioCasetaRepository repository)
    {
        _repository = repository;
    }


    public Task<IReadOnlyList<AsignacionSocioCasetaDto>> ObtenerAsync()
    {
        return _repository.ObtenerAsync();
    }


    public Task<AsignacionSocioCasetaDto?> ObtenerPorIdAsync(
        Guid id)
    {
        return _repository.ObtenerPorIdAsync(id);
    }


    public Task<AsignacionSocioCasetaDto> CrearAsync(
        CrearAsignacionSocioCasetaDto dto)
    {
        return _repository.CrearAsync(dto);
    }
}