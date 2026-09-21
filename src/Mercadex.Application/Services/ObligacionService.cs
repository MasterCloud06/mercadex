using Mercadex.Application.DTOs;
using Mercadex.Application.Interfaces;

namespace Mercadex.Application.Services;

public sealed class ObligacionService : IObligacionService
{
    private readonly IObligacionRepository _repository;


    public ObligacionService(
        IObligacionRepository repository)
    {
        _repository = repository;
    }


    public Task<IEnumerable<ObligacionDto>> GetAllAsync()
    {
        return _repository.GetAllAsync();
    }


    public Task<ObligacionDto?> GetByIdAsync(
        Guid id)
    {
        return _repository.GetByIdAsync(id);
    }


    public Task<ObligacionDto> CrearAsync(
        CrearObligacionDto dto)
    {
        return _repository.CrearAsync(dto);
    }


    public Task<IEnumerable<ObligacionDto>>
        ObtenerPendientesPorUnidadAsync(
            Guid unidadComercialId)
    {
        return _repository
            .ObtenerPendientesPorUnidadAsync(
                unidadComercialId);
    }


    public Task<bool> MarcarComoPagadaAsync(
        Guid id)
    {
        return _repository.MarcarComoPagadaAsync(id);
    }


    public Task<bool> RestaurarComoPendienteAsync(
        Guid id)
    {
        return _repository.RestaurarComoPendienteAsync(id);
    }
}