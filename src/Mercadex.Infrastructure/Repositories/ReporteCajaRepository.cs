using Mercadex.Application.DTOs;
using Mercadex.Application.Interfaces;
using Mercadex.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mercadex.Infrastructure.Repositories;

public sealed class ReporteCajaRepository 
    : IReporteCajaRepository
{
    private readonly MercadexDbContext _context;


    public ReporteCajaRepository(
        MercadexDbContext context)
    {
        _context = context;
    }


    public async Task<ReporteIngresosDto> ObtenerIngresosAsync(
        DateTimeOffset inicio,
        DateTimeOffset fin)
    {
        inicio = inicio.ToUniversalTime();
        fin = fin.ToUniversalTime();


        var recibos = await _context.Recibos
            .AsNoTracking()
            .Where(r =>
                r.FechaEmision >= inicio &&
                r.FechaEmision <= fin &&
                r.Estado == Domain.Caja.EstadoRecibo.Emitido)
            .ToListAsync();


        return new ReporteIngresosDto(
            inicio,
            fin,
            recibos.Count,
            recibos.Sum(r => r.Total));
    }


    public async Task<IReadOnlyList<ReporteConceptoDto>>
        ObtenerPorConceptoAsync(
            DateTimeOffset inicio,
            DateTimeOffset fin)
    {
        inicio = inicio.ToUniversalTime();
        fin = fin.ToUniversalTime();


        return await _context.Recibos
            .AsNoTracking()
            .Where(r =>
                r.FechaEmision >= inicio &&
                r.FechaEmision <= fin &&
                r.Estado == Domain.Caja.EstadoRecibo.Emitido)
            .SelectMany(r => r.Detalles)
            .GroupBy(d => d.Concepto)
            .Select(g =>
                new ReporteConceptoDto(
                    g.Key,
                    g.Count(),
                    g.Sum(x => x.Monto)))
            .ToListAsync();
    }


    public async Task<IReadOnlyList<ReporteDetalleCobroDto>>
        ObtenerDetalleAsync(
            DateTimeOffset inicio,
            DateTimeOffset fin)
    {
        inicio = inicio.ToUniversalTime();
        fin = fin.ToUniversalTime();


        return await _context.Recibos
            .AsNoTracking()
            .Where(r =>
                r.FechaEmision >= inicio &&
                r.FechaEmision <= fin &&
                r.Estado == Domain.Caja.EstadoRecibo.Emitido)
            .SelectMany(
                r => r.Detalles,
                (r, d) =>
                    new ReporteDetalleCobroDto(
                        r.Numero,
                        r.UnidadComercialId,
                        r.FechaEmision,
                        d.Concepto,
                        d.Monto))
            .ToListAsync();
    }
}