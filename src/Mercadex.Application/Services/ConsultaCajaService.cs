using Mercadex.Application.DTOs;
using Mercadex.Application.Interfaces;

namespace Mercadex.Application.Services;

public sealed class ConsultaCajaService
{
    private readonly IConsultaCajaRepository _repository;

    public ConsultaCajaService(
        IConsultaCajaRepository repository)
    {
        _repository = repository;
    }


    public Task<IEnumerable<ObligacionDto>> 
        ObtenerDeudasAsync(Guid unidadComercialId)
    {
        return _repository
            .GetDeudasPorCasetaAsync(unidadComercialId);
    }
}