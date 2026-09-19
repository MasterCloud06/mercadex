using Mercadex.Application.DTOs;
using Mercadex.Application.Interfaces;
using Mercadex.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mercadex.Infrastructure.Repositories;

public sealed class EstadisticaCajaRepository 
    : IEstadisticaCajaRepository
{
    private readonly MercadexDbContext _context;


    public EstadisticaCajaRepository(
        MercadexDbContext context)
    {
        _context = context;
    }


    public async Task<EstadisticasCajaDto> ObtenerAsync(
        DateTimeOffset inicio,
        DateTimeOffset fin)
    {
        inicio = inicio.ToUniversalTime();
        fin = fin.ToUniversalTime();


        var pagos = await _context.Pagos
            .AsNoTracking()
            .Where(p =>
                p.FechaRegistro >= inicio &&
                p.FechaRegistro <= fin &&
                p.Estado == Domain.Caja.EstadoPago.Registrado)
            .ToListAsync();


        var recibos = await _context.Recibos
            .AsNoTracking()
            .Where(r =>
                r.FechaEmision >= inicio &&
                r.FechaEmision <= fin &&
                r.Estado == Domain.Caja.EstadoRecibo.Emitido)
            .CountAsync();


        var formasPago = pagos
            .GroupBy(p => p.FormaPago.ToString())
            .Select(g =>
                new EstadisticaFormaPagoDto(
                    g.Key,
                    g.Count(),
                    g.Sum(x => x.Monto)))
            .ToList();


        return new EstadisticasCajaDto(
            inicio,
            fin,
            pagos.Count,
            recibos,
            pagos.Sum(x => x.Monto),
            pagos.Count == 0
                ? 0
                : pagos.Sum(x => x.Monto) / pagos.Count,
            formasPago);
    }
}