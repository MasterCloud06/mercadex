namespace Mercadex.Application.Interfaces;

public interface IUnidadComercialRepository
{
    Task<IReadOnlyList<Guid>>
        ObtenerIdsActivosAsync();
}