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
    if (dto.Items is null)
    {
        throw new ArgumentException(
            "Debe seleccionar al menos una obligación.");
    }

    var solicitudes = dto.Items.ToList();

    if (solicitudes.Count == 0)
    {
        throw new ArgumentException(
            "Debe seleccionar al menos una obligación.");
    }

    if (solicitudes
        .GroupBy(x => x.ObligacionId)
        .Any(g => g.Count() > 1))
    {
        throw new InvalidOperationException(
            "No puede seleccionar dos veces la misma obligación.");
    }

    await _unitOfWork.BeginTransactionAsync();

    try
    {
        var obligaciones =
            await _obligacionRepository.ObtenerPorIdsAsync(
                solicitudes.Select(x => x.ObligacionId));

        if (obligaciones.Count != solicitudes.Count)
        {
            throw new InvalidOperationException(
                "No se encontraron todas las obligaciones.");
        }

        if (obligaciones.Any(x =>
            x.UnidadComercialId != dto.UnidadComercialId))
        {
            throw new InvalidOperationException(
                "Las obligaciones no pertenecen a la unidad seleccionada.");
        }

        if (obligaciones.Any(x =>
            x.MonedaId != dto.MonedaId))
        {
            throw new InvalidOperationException(
                "La moneda del pago no coincide con las obligaciones.");
        }

        var totalEsperado = obligaciones.Sum(x => x.Monto);

        if (dto.Monto != totalEsperado)
        {
            throw new InvalidOperationException(
                "El importe no coincide con las obligaciones seleccionadas.");
        }

        var items = obligaciones
            .Select(o => new ItemCobro(
                o,
                solicitudes.First(x =>
                    x.ObligacionId == o.Id).NombreConcepto))
            .ToList();

        // El número se asigna en el servidor.
        // El valor NumeroRecibo enviado por el escritorio se ignora.
        var numeroRecibo =
            await _unitOfWork.ObtenerSiguienteNumeroReciboAsync();

        var resultado = ServicioCobro.Registrar(
            numeroRecibo,
            items,
            (FormaPago)dto.FormaPago,
            dto.NombrePagador,
            DateTimeOffset.UtcNow,
            dto.TipoCambio,
            dto.Referencia);

        await _pagoRepository.GuardarAsync(resultado.Pago);

        await _reciboRepository.GuardarAsync(resultado.Recibo);

        await _unitOfWork.CommitAsync();

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
    catch
    {
        await _unitOfWork.RollbackAsync();
        throw;
    }
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