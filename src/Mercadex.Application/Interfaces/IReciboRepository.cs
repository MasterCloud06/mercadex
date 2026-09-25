using Mercadex.Application.DTOs;
using Mercadex.Domain.Caja;

namespace Mercadex.Application.Interfaces;

public interface IReciboRepository
{
    Task<IEnumerable<ReciboDto>> GetAllAsync();

    Task<ReciboDto?> GetByIdAsync(
        Guid id);

    Task<ReciboDto> CrearAsync(
        CrearReciboDto dto);

    Task GuardarAsync(
        Recibo recibo);

    Task<bool> AnularAsync(
        Guid id,
        string motivo,
        string anuladoPor);
}