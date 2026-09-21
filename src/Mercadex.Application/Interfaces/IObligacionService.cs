using Mercadex.Application.DTOs;

namespace Mercadex.Application.Interfaces;

public interface IObligacionService
{
    Task<IEnumerable<ObligacionDto>> GetAllAsync();


    Task<ObligacionDto?> GetByIdAsync(
        Guid id);


    Task<ObligacionDto> CrearAsync(
        CrearObligacionDto dto);


    Task<IEnumerable<ObligacionDto>>
        ObtenerPendientesPorUnidadAsync(
            Guid unidadComercialId);


    Task<bool> MarcarComoPagadaAsync(
        Guid id);


    Task<bool> RestaurarComoPendienteAsync(
        Guid id);
}