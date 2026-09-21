using Mercadex.Application.DTOs;
using Mercadex.Domain.Cobranza;

namespace Mercadex.Application.Interfaces;

public interface IObligacionRepository
{
    Task<IEnumerable<ObligacionDto>> GetAllAsync();


    Task<ObligacionDto?> GetByIdAsync(
        Guid id);


    Task<ObligacionDto> CrearAsync(
        CrearObligacionDto dto);


    Task<IReadOnlyList<Obligacion>> ObtenerPorIdsAsync(
        IEnumerable<Guid> ids);


    Task<bool> ExisteAsync(
        Guid unidadComercialId,
        Guid conceptoCobroId,
        int periodoAnio,
        int periodoMes);


    Task<bool> MarcarComoPagadaAsync(
        Guid id);


    Task<bool> RestaurarComoPendienteAsync(
        Guid id);

Task<IEnumerable<ObligacionDto>> ObtenerPendientesPorUnidadAsync(
    Guid unidadComercialId);
}