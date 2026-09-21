using Mercadex.Application.DTOs;
using Mercadex.Application.DTOs.Caja;
using Mercadex.Application.Interfaces;
using Mercadex.Application.Interfaces.Persistence;
using Mercadex.Domain.Caja;

namespace Mercadex.Application.Services;

public sealed class CajaService : ICajaService
{
    private readonly IPagoRepository _pagoRepository;
    private readonly IReciboRepository _reciboRepository;
    private readonly IObligacionRepository _obligacionRepository;
    private readonly IUnitOfWork _unitOfWork;


    public CajaService(
        IPagoRepository pagoRepository,
        IReciboRepository reciboRepository,
        IObligacionRepository obligacionRepository,
        IUnitOfWork unitOfWork)
    {
        _pagoRepository = pagoRepository;
        _reciboRepository = reciboRepository;
        _obligacionRepository = obligacionRepository;
        _unitOfWork = unitOfWork;
    }


    public Task<PagoDto> RegistrarPagoAsync(
        CrearPagoDto dto)
    {
        return _pagoRepository.CrearAsync(dto);
    }


    public async Task<ReciboDto> GenerarReciboAsync(
        CrearReciboDto dto)
    {
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


    public async Task<ResultadoCobroDto> ProcesarCobroAsync(
        ProcesarCobroDto dto)
    {
        var obligaciones =
            await _obligacionRepository
                .ObtenerPorIdsAsync(
                    dto.Items.Select(x => x.ObligacionId));


        if (obligaciones.Count != dto.Items.Count())
        {
            throw new InvalidOperationException(
                "No se encontraron todas las obligaciones.");
        }


        var items = obligaciones
            .Select(o =>
                new ItemCobro(
                    o,
                    dto.Items
                        .First(x =>
                            x.ObligacionId == o.Id)
                        .NombreConcepto))
            .ToList();


        var totalEsperado =
            obligaciones.Sum(x => x.Monto);


        if (dto.Monto != totalEsperado)
        {
            throw new InvalidOperationException(
                "El monto enviado no coincide con el total de las obligaciones.");
        }


        if (obligaciones.Any(x =>
            x.MonedaId != dto.MonedaId))
        {
            throw new InvalidOperationException(
                "La moneda del pago no coincide con la moneda de las obligaciones.");
        }


        var resultado =
            ServicioCobro.Registrar(
                dto.NumeroRecibo,
                items,
                (FormaPago)dto.FormaPago,
                dto.NombrePagador,
                DateTimeOffset.UtcNow,
                dto.TipoCambio,
                dto.Referencia);


        await _unitOfWork
            .BeginTransactionAsync();

        try
        {
            await _pagoRepository
                .GuardarAsync(
                    resultado.Pago);


            await _reciboRepository
                .GuardarAsync(
                    resultado.Recibo);


            await _unitOfWork
                .CommitAsync();
        }
        catch
        {
            await _unitOfWork
                .RollbackAsync();

            throw;
        }


        return new ResultadoCobroDto(
            new PagoDto(
                resultado.Pago.Id,
                resultado.Pago.MonedaId,
                resultado.Pago.Monto,
                resultado.Pago.FormaPago.ToString(),
                resultado.Pago.NombrePagador,
                resultado.Pago.Referencia,
                resultado.Pago.TipoCambio,
                resultado.Pago.FechaRegistro,
                resultado.Pago.Estado.ToString()),

            new ReciboDto(
                resultado.Recibo.Id,
                resultado.Recibo.Numero,
                resultado.Recibo.UnidadComercialId,
                resultado.Recibo.PagoId,
                resultado.Recibo.FechaEmision,
                resultado.Recibo.Estado.ToString(),
                resultado.Recibo.Total,
                resultado.Recibo.Detalles.Select(d =>
                    new DetalleReciboDto(
                        d.ObligacionId,
                        d.ConceptoCobroId,
                        d.Concepto,
                        d.Monto,
                        d.PeriodoAnio,
                        d.PeriodoMes)))
        );
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