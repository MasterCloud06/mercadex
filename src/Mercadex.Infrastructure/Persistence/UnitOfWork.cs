
using Mercadex.Application.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace Mercadex.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly MercadexDbContext _context;

    private IDbContextTransaction? _transaction;

    public UnitOfWork(MercadexDbContext context)
    {
        _context = context;
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database
            .BeginTransactionAsync();
    }

    public async Task<long> ObtenerSiguienteNumeroReciboAsync()
    {
        if (_transaction is null)
        {
            throw new InvalidOperationException(
                "Debe iniciarse una transacción antes de generar el número de recibo.");
        }

        var connection = _context.Database.GetDbConnection();

        await using var command = connection.CreateCommand();

        command.Transaction = _transaction.GetDbTransaction();

        command.CommandText =
            "SELECT nextval('numeracion_recibos')";

        var resultado = await command.ExecuteScalarAsync();

        return Convert.ToInt64(resultado);
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

    
    // MERCADEX_ANULACION_AUDITADA_V2
    // Se utiliza la transaccion ya iniciada por CajaService.

    public Task RegistrarComprobanteAnulacionAsync(
        Mercadex.Domain.Caja.ComprobanteAnulacionRecibo comprobante)
    {
        ArgumentNullException.ThrowIfNull(comprobante);

        if (_transaction is null)
        {
            throw new InvalidOperationException(
                "La anulacion requiere una transaccion activa.");
        }

        _context.ComprobantesAnulacion.Add(comprobante);

        return Task.CompletedTask;
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
