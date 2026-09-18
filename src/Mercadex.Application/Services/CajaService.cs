using Mercadex.Application.DTOs;
using Mercadex.Application.Interfaces;

namespace Mercadex.Application.Services;

public sealed class CajaService : ICajaService
{
    private readonly IPagoRepository _pagoRepository;
    private readonly IReciboRepository _reciboRepository;

    public CajaService(
        IPagoRepository pagoRepository,
        IReciboRepository reciboRepository)
    {
        _pagoRepository = pagoRepository;
        _reciboRepository = reciboRepository;
    }


    public Task<PagoDto> RegistrarPagoAsync(
        CrearPagoDto dto)
    {
        return _pagoRepository.CrearAsync(dto);
    }


public Task<ReciboDto> GenerarReciboAsync(
    CrearReciboDto dto)
{
    return _reciboRepository.CrearAsync(dto);
}
}