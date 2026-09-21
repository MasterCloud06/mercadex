using Mercadex.Application.DTOs.Ingresos;


namespace Mercadex.Application.Interfaces;


public interface IIngresoRepository
{
    Task<IReadOnlyList<IngresoMovimientoDto>> ObtenerAsync();
}