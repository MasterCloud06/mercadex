using Mercadex.Application.DTOs;
using Mercadex.Application.Interfaces;

namespace Mercadex.Application.Services;

public sealed class ConsultaRecibosService 
    : IConsultaRecibosService
{
    private readonly IReciboRepository _repository;


    public ConsultaRecibosService(
        IReciboRepository repository)
    {
        _repository = repository;
    }


    public Task<IEnumerable<ReciboConsultaDto>> ObtenerTodosAsync()
    {
        return _repository.GetAllAsync();
    }


    public Task<IEnumerable<ReciboConsultaDto>> ObtenerPorFechaAsync(
        DateTimeOffset fecha)
    {
        return _repository.GetByFechaAsync(fecha);
    }
}