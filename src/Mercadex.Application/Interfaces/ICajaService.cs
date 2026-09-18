using Mercadex.Application.DTOs;

namespace Mercadex.Application.Interfaces;

public interface ICajaService
{
    Task<PagoDto> RegistrarPagoAsync(
        CrearPagoDto dto);

    Task<ReciboDto> GenerarReciboAsync(
    CrearReciboDto dto);
}