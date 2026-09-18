using Mercadex.Application.DTOs;
using Mercadex.Application.Interfaces;

namespace Mercadex.Application.Services;

public sealed class CajaService : ICajaService
{
    private readonly IPagoRepository _pagoRepository;
    private readonly IReciboRepository _reciboRepository;
    private readonly IObligacionRepository _obligacionRepository;


    public CajaService(
        IPagoRepository pagoRepository,
        IReciboRepository reciboRepository,
        IObligacionRepository obligacionRepository)
    {
        _pagoRepository = pagoRepository;
        _reciboRepository = reciboRepository;
        _obligacionRepository = obligacionRepository;
    }


    public Task<PagoDto> RegistrarPagoAsync(
        CrearPagoDto dto)
    {
        return _pagoRepository.CrearAsync(dto);
    }

public async Task<ReciboDto> GenerarReciboAsync(
    CrearReciboDto dto)
{
    var reciboExistente =
        await _reciboRepository
            .GetByPagoIdAsync(dto.PagoId);


    if (reciboExistente is not null)
    {
        throw new InvalidOperationException(
            "El pago ya tiene un recibo generado.");
    }


    foreach (var detalle in dto.Detalles)
    {
        var obligacion =
            await _obligacionRepository
                .GetByIdAsync(detalle.ObligacionId);


        if (obligacion is null)
        {
            throw new InvalidOperationException(
                $"La obligación {detalle.ObligacionId} no existe.");
        }


        if (obligacion.Estado != "Pendiente")
        {
            throw new InvalidOperationException(
                $"La obligación {detalle.ObligacionId} no está pendiente.");
        }


        if (obligacion.Monto != detalle.Monto)
        {
            throw new InvalidOperationException(
                $"El monto de la obligación {detalle.ObligacionId} no coincide.");
        }
    }


    var recibo = await _reciboRepository
        .CrearAsync(dto);


    foreach (var detalle in dto.Detalles)
    {
        await _obligacionRepository
            .MarcarComoPagadaAsync(
                detalle.ObligacionId);
    }


    return recibo;
}


    public async Task<bool> AnularReciboAsync(
        Guid reciboId,
        AnularReciboDto dto)
    {
        var recibo = await _reciboRepository
            .GetByIdAsync(reciboId);


        if (recibo is null)
        {
            return false;
        }


        var reciboAnulado = await _reciboRepository
            .AnularAsync(
                reciboId,
                dto.Motivo);


        if (!reciboAnulado)
        {
            return false;
        }


        await _pagoRepository
            .AnularAsync(
                recibo.PagoId);


        foreach (var detalle in recibo.Detalles)
        {
            await _obligacionRepository
                .RestaurarComoPendienteAsync(
                    detalle.ObligacionId);
        }


        return true;
    }
}