using Mercadex.Application.DTOs;

namespace Mercadex.Application.Interfaces;

public interface IObligacionRepository
{
    Task<IEnumerable<ObligacionDto>> GetAllAsync();

    Task<ObligacionDto?> GetByIdAsync(Guid id);

    Task<ObligacionDto> CrearAsync(
        CrearObligacionDto dto);

    Task<bool> MarcarComoPagadaAsync(
        Guid id);

    Task<bool> RestaurarComoPendienteAsync(
        Guid id);
Task<bool> ExisteAsync(
    Guid unidadComercialId,
    Guid conceptoCobroId,
    int periodoAnio,
    int periodoMes);
}