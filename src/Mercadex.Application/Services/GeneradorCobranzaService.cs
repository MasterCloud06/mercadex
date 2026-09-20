using Mercadex.Application.DTOs;
using Mercadex.Application.DTOs.Cobranza;
using Mercadex.Application.Interfaces;

namespace Mercadex.Application.Services;

public sealed class GeneradorCobranzaService
{
    private readonly IUnidadComercialRepository _unidadRepository;
    private readonly IObligacionRepository _obligacionRepository;


    public GeneradorCobranzaService(
        IUnidadComercialRepository unidadRepository,
        IObligacionRepository obligacionRepository)
    {
        _unidadRepository = unidadRepository;
        _obligacionRepository = obligacionRepository;
    }


    public async Task<ResultadoGeneracionCobranzaDto>
        GenerarAsync(
            GenerarCobranzaDto dto)
    {
        var unidades =
            await _unidadRepository
                .ObtenerIdsActivosAsync();


        var cantidad = 0;

        decimal total = 0;


        foreach (var unidadId in unidades)
{
    var existe =
        await _obligacionRepository.ExisteAsync(
            unidadId,
            dto.ConceptoCobroId,
            dto.PeriodoAnio,
            dto.PeriodoMes);


    if (existe)
    {
        continue;
    }


    await _obligacionRepository.CrearAsync(
        new CrearObligacionDto(
            unidadId,
            dto.ConceptoCobroId,
            dto.MonedaId,
            dto.Monto,
            dto.FechaEmision,
            dto.FechaVencimiento,
            dto.PeriodoAnio,
            dto.PeriodoMes));


    cantidad++;

    total += dto.Monto;
}

        return new ResultadoGeneracionCobranzaDto(
            cantidad,
            total);
    }
}