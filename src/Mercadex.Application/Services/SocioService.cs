using Mercadex.Application.DTOs.Socios;
using Mercadex.Application.Interfaces;

namespace Mercadex.Application.Services;

public sealed class SocioService : ISocioService
{
    private readonly ISocioRepository _repository;


    public SocioService(
        ISocioRepository repository)
    {
        _repository = repository;
    }


    public async Task<IReadOnlyList<SocioDto>> ObtenerAsync()
    {
        return await _repository.ObtenerAsync();
    }


    public async Task<SocioDto?> ObtenerPorIdAsync(
        Guid id)
    {
        return await _repository.ObtenerPorIdAsync(id);
    }


    public async Task<SocioDto> CrearAsync(
        CrearSocioDto dto)
    {
        return await _repository.CrearAsync(dto);
    }
}