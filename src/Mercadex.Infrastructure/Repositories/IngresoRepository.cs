using Mercadex.Application.DTOs.Ingresos;
using Mercadex.Application.Interfaces;
using Mercadex.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;


namespace Mercadex.Infrastructure.Repositories;


public sealed class IngresoRepository
    : IIngresoRepository
{

    private readonly MercadexDbContext _context;


    public IngresoRepository(
        MercadexDbContext context)
    {
        _context = context;
    }



    public async Task<IReadOnlyList<IngresoMovimientoDto>> ObtenerAsync()
    {

        var ingresos =
            await
            (
                from recibo in _context.Recibos

                join pago in _context.Pagos
                    on recibo.PagoId equals pago.Id


                join unidad in _context.UnidadesComerciales
                    on recibo.UnidadComercialId equals unidad.Id


                join asignacion in _context.AsignacionesSocioCaseta
                    on unidad.Id equals asignacion.UnidadComercialId
                    into asignaciones


                from asignacion in asignaciones
                    .Where(x => x.Activa)
                    .DefaultIfEmpty()


                join socio in _context.Socios
                    on asignacion.SocioId equals socio.Id
                    into socios


                from socio in socios.DefaultIfEmpty()


                select new IngresoMovimientoDto
                (
                    pago.FechaRegistro,
                    recibo.Numero,
                    unidad.Codigo,
                    unidad.Bloque ?? "",
                    socio == null
                        ? ""
                        : socio.Nombres + " " + socio.Apellidos,

                    pago.NombrePagador,

                    pago.FormaPago.ToString(),

                    pago.Monto,

                    recibo.Estado.ToString()
                )

            )
            .AsNoTracking()
            .ToListAsync();



        return ingresos;

    }

}