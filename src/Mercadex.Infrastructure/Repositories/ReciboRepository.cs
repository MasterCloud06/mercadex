using Mercadex.Application.DTOs;
using Mercadex.Application.Interfaces;
using Mercadex.Domain.Caja;
using Mercadex.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mercadex.Infrastructure.Repositories;

public sealed class ReciboRepository : IReciboRepository
{
    private readonly MercadexDbContext _context;

    public ReciboRepository(
        MercadexDbContext context)
    {
        _context = context;
    }


    public async Task<ReciboDto?> GetByIdAsync(Guid id)
    {
        return await _context.Recibos
            .AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new ReciboDto(
                r.Id,
                r.Numero,
                r.UnidadComercialId,
                r.PagoId,
                r.FechaEmision,
                r.Estado.ToString(),
                r.Total,
                r.Detalles.Select(d =>
                    new DetalleReciboDto(
                        d.ObligacionId,
                        d.ConceptoCobroId,
                        d.Concepto,
                        d.Monto,
                        d.PeriodoAnio,
                        d.PeriodoMes))
            ))
            .FirstOrDefaultAsync();
    }


    public async Task<ReciboDto> CrearAsync(
        CrearReciboDto dto)
    {
        var detallesDominio = dto.Detalles
            .Select(d => new DetalleRecibo(
                d.ObligacionId,
                d.ConceptoCobroId,
                d.Concepto,
                d.Monto,
                d.PeriodoAnio,
                d.PeriodoMes))
            .ToList();


        var recibo = new Recibo(
            dto.Numero,
            dto.UnidadComercialId,
            dto.PagoId,
            DateTimeOffset.UtcNow,
            detallesDominio);


        _context.Recibos.Add(recibo);

        await _context.SaveChangesAsync();


        return new ReciboDto(
            recibo.Id,
            recibo.Numero,
            recibo.UnidadComercialId,
            recibo.PagoId,
            recibo.FechaEmision,
            recibo.Estado.ToString(),
            recibo.Total,
            recibo.Detalles.Select(d =>
                new DetalleReciboDto(
                    d.ObligacionId,
                    d.ConceptoCobroId,
                    d.Concepto,
                    d.Monto,
                    d.PeriodoAnio,
                    d.PeriodoMes))
        );
    }
public async Task<bool> AnularAsync(
    Guid id,
    string motivo)
{
    var recibo = await _context.Recibos
        .FirstOrDefaultAsync(r => r.Id == id);

    if (recibo is null)
    {
        return false;
    }

    recibo.Anular(
        motivo,
        DateTimeOffset.UtcNow);

    await _context.SaveChangesAsync();

    return true;
}
public async Task<IEnumerable<ReciboConsultaDto>> GetAllAsync()
{
    return await _context.Recibos
        .AsNoTracking()
        .OrderByDescending(r => r.FechaEmision)
        .Select(r => new ReciboConsultaDto(
            r.Id,
            r.Numero,
            r.UnidadComercialId,
            r.PagoId,
            r.FechaEmision,
            r.Estado.ToString(),
            r.Total
        ))
        .ToListAsync();
}


public async Task<IEnumerable<ReciboConsultaDto>> GetByFechaAsync(
    DateTimeOffset fecha)
    {
        var fechaConsulta = fecha.Date;

        return await _context.Recibos
            .AsNoTracking()
            .Where(r =>
                r.FechaEmision.Date == fechaConsulta)
            .OrderByDescending(r => r.FechaEmision)
            .Select(r => new ReciboConsultaDto(
                r.Id,
                r.Numero,
                r.UnidadComercialId,
                r.PagoId,
                r.FechaEmision,
                r.Estado.ToString(),
                r.Total
            ))
            .ToListAsync();
    }
}