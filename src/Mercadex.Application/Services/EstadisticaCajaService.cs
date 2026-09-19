using Mercadex.Application.DTOs;
using Mercadex.Application.Interfaces;

namespace Mercadex.Application.Services;

public sealed class EstadisticaCajaService 
    : IEstadisticaCajaService
{
    private readonly IEstadisticaCajaRepository _repository;


    public EstadisticaCajaService(
        IEstadisticaCajaRepository repository)
    {
        _repository = repository;
    }


    public Task<EstadisticasCajaDto> ObtenerAsync(
        DateTimeOffset inicio,
        DateTimeOffset fin)
    {
        return _repository
            .ObtenerAsync(
                inicio,
                fin);
    }
}