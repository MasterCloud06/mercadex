using Mercadex.Application.DTOs;

namespace Mercadex.Application.Interfaces;

public interface IConsultaRecibosService
{
    Task<IEnumerable<ReciboConsultaDto>> ObtenerTodosAsync();

    Task<IEnumerable<ReciboConsultaDto>> ObtenerPorFechaAsync(
        DateTimeOffset fecha);
}