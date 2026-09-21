using Mercadex.Application.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace Mercadex.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly MercadexDbContext _context;

    private IDbContextTransaction? _transaction;


    public UnitOfWork(
        MercadexDbContext context)
    {
        _context = context;
    }


    public async Task BeginTransactionAsync()
    {
        _transaction =
            await _context.Database
                .BeginTransactionAsync();
    }


    public async Task CommitAsync()
    {
        if (_transaction is null)
        {
            return;
        }

        await _context.SaveChangesAsync();

        await _transaction.CommitAsync();

        await _transaction.DisposeAsync();

        _transaction = null;
    }


    public async Task RollbackAsync()
    {
        if (_transaction is null)
        {
            return;
        }

        await _transaction.RollbackAsync();

        await _transaction.DisposeAsync();

        _transaction = null;
    }
}