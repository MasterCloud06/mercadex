using Mercadex.Application.DTOs;
using Mercadex.Application.DTOs.Caja;

namespace Mercadex.Application.Interfaces;

public interface ICajaService
{
    Task<PagoDto> RegistrarPagoAsync(
        CrearPagoDto dto);


    Task<ReciboDto> GenerarReciboAsync(
        CrearReciboDto dto);


    Task<bool> AnularReciboAsync(
        Guid reciboId,
        AnularReciboDto dto);


    Task<ResultadoCobroDto> ProcesarCobroAsync(
        ProcesarCobroDto dto);
}