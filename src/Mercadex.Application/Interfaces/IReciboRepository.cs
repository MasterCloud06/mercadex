using Mercadex.Application.DTOs;

namespace Mercadex.Application.Interfaces;

public interface IReciboRepository
{
    Task<ReciboDto?> GetByIdAsync(Guid id);

    Task<ReciboDto?> GetByPagoIdAsync(
        Guid pagoId);

    Task<ReciboDto> CrearAsync(
        CrearReciboDto dto);

    Task<bool> AnularAsync(
        Guid id,
        string motivo);
}