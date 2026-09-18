using Mercadex.Application.DTOs;

namespace Mercadex.Application.Interfaces;

public interface IConsultaCajaRepository
{
    Task<IEnumerable<ObligacionDto>> 
        GetDeudasPorCasetaAsync(Guid unidadComercialId);
}