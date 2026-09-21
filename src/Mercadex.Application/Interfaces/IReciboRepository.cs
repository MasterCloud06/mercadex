using Mercadex.Application.DTOs;
using Mercadex.Domain.Caja;

namespace Mercadex.Application.Interfaces;

public interface IReciboRepository
{
    Task<ReciboDto?> GetByIdAsync(
        Guid id);


    Task<ReciboDto> CrearAsync(
        CrearReciboDto dto);


    Task GuardarAsync(
        Recibo recibo);


    Task<bool> AnularAsync(
        Guid id,
        string motivo);
}