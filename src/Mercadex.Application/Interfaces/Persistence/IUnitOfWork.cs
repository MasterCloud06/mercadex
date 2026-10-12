
namespace Mercadex.Application.Interfaces.Persistence;

public interface IUnitOfWork
{
    Task BeginTransactionAsync();

    Task<long> ObtenerSiguienteNumeroReciboAsync();

    Task CommitAsync();

    
    // MERCADEX_ANULACION_AUDITADA_V2
    Task RegistrarComprobanteAnulacionAsync(
        Mercadex.Domain.Caja.ComprobanteAnulacionRecibo comprobante);
    Task RollbackAsync();
}
