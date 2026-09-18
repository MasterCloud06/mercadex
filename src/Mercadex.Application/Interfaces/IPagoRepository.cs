using Mercadex.Application.DTOs;

namespace Mercadex.Application.Interfaces;

public interface IPagoRepository
{
    Task<PagoDto?> GetByIdAsync(Guid id);

    Task<PagoDto> CrearAsync(
        CrearPagoDto dto);
}