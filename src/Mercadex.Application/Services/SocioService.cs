using Mercadex.Application.DTOs;
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

    public Task<IEnumerable<SocioDto>> GetAllAsync()
    {
        return _repository.GetAllAsync();
    }

    public Task<SocioDto?> GetByIdAsync(
        Guid id)
    {
        return _repository.GetByIdAsync(id);
    }

    public Task<SocioDto> CrearAsync(
        CrearSocioDto dto)
    {
        return _repository.CrearAsync(dto);
    }

    public Task<SocioDto?> ActualizarAsync(
        Guid id,
        ActualizarSocioDto dto)
    {
        return _repository.ActualizarAsync(id, dto);
    }

    public Task<bool> DesactivarAsync(
        Guid id)
    {
        return _repository.DesactivarAsync(id);
    }
}