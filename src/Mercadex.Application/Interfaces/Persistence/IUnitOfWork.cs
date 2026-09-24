
namespace Mercadex.Application.Interfaces.Persistence;

public interface IUnitOfWork
{
    Task BeginTransactionAsync();

    Task<long> ObtenerSiguienteNumeroReciboAsync();

    Task CommitAsync();

    Task RollbackAsync();
}
