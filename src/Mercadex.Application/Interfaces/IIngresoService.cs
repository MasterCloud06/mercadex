using Mercadex.Application.DTOs.Ingresos;


namespace Mercadex.Application.Interfaces;


public interface IIngresoService
{

    Task<IReadOnlyList<IngresoMovimientoDto>> ObtenerAsync();

}