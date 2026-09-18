using Mercadex.Application.DTOs;
using Mercadex.Application.Interfaces;
using Mercadex.Domain.Caja;
using Mercadex.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mercadex.Infrastructure.Repositories;

public sealed class PagoRepository : IPagoRepository
{
    private readonly MercadexDbContext _context;

    public PagoRepository(
        MercadexDbContext context)
    {
        _context = context;
    }


    public async Task<PagoDto?> GetByIdAsync(Guid id)
    {
        return await _context.Pagos
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new PagoDto(
                p.Id,
                p.MonedaId,
                p.Monto,
                p.FormaPago.ToString(),
                p.NombrePagador,
                p.Referencia,
                p.TipoCambio,
                p.FechaRegistro,
                p.Estado.ToString()
            ))
            .FirstOrDefaultAsync();
    }


    public async Task<PagoDto> CrearAsync(
        CrearPagoDto dto)
    {
        var pago = new Pago(
            dto.MonedaId,
            dto.Monto,
            (FormaPago)dto.FormaPago,
            dto.NombrePagador,
            DateTimeOffset.UtcNow,
            dto.TipoCambio,
            dto.Referencia
        );

        _context.Pagos.Add(pago);

        await _context.SaveChangesAsync();

        return new PagoDto(
            pago.Id,
            pago.MonedaId,
            pago.Monto,
            pago.FormaPago.ToString(),
            pago.NombrePagador,
            pago.Referencia,
            pago.TipoCambio,
            pago.FechaRegistro,
            pago.Estado.ToString()
        );
    }
}