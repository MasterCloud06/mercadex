using Mercadex.Application.DTOs;

namespace Mercadex.Application.Interfaces;

public interface IReciboRepository
{
    Task<ReciboDto?> GetByIdAsync(Guid id);

Task<ReciboDto> CrearAsync(
    CrearReciboDto dto);
}