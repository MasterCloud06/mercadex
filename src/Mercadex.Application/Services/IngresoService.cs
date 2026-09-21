using Mercadex.Application.DTOs.Ingresos;
using Mercadex.Application.Interfaces;


namespace Mercadex.Application.Services;


public sealed class IngresoService
    : IIngresoService
{

    private readonly IIngresoRepository _repository;


    public IngresoService(
        IIngresoRepository repository)
    {
        _repository = repository;
    }



    public Task<IReadOnlyList<IngresoMovimientoDto>> ObtenerAsync()
    {
        return _repository.ObtenerAsync();
    }

}