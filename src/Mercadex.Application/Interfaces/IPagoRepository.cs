using Mercadex.Application.DTOs;
using Mercadex.Domain.Caja;

namespace Mercadex.Application.Interfaces;

public interface IPagoRepository
{
    Task<PagoDto?> GetByIdAsync(
        Guid id);


    Task<PagoDto> CrearAsync(
        CrearPagoDto dto);


    Task GuardarAsync(
        Pago pago);


    Task<bool> AnularAsync(
        Guid id);
}