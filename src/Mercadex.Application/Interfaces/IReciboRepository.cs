using Mercadex.Application.DTOs;

namespace Mercadex.Application.Interfaces;

public interface IReciboRepository
{
    Task<ReciboDto?> GetByIdAsync(Guid id);

    Task<ReciboDto> CrearAsync(
        CrearReciboDto dto);

    Task<bool> AnularAsync(
        Guid id,
        string motivo);

Task<IEnumerable<ReciboConsultaDto>> GetAllAsync();

Task<IEnumerable<ReciboConsultaDto>> 
    GetByFechaAsync(DateTimeOffset fecha);
}