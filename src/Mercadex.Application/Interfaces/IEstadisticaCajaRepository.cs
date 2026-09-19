using Mercadex.Application.DTOs;

namespace Mercadex.Application.Interfaces;

public interface IEstadisticaCajaRepository
{
    Task<EstadisticasCajaDto> ObtenerAsync(
        DateTimeOffset inicio,
        DateTimeOffset fin);
}