namespace Mercadex.Application.Interfaces.Persistence;

public interface IUnitOfWork
{
    Task BeginTransactionAsync();

    Task CommitAsync();

    Task RollbackAsync();
}