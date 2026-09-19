using Mercadex.Application.DTOs;

namespace Mercadex.Application.Interfaces;

public interface IEstadisticaCajaService
{
    Task<EstadisticasCajaDto> ObtenerAsync(
        DateTimeOffset inicio,
        DateTimeOffset fin);
}