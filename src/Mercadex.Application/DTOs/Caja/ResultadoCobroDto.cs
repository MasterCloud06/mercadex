using Mercadex.Application.DTOs;

namespace Mercadex.Application.DTOs.Caja;

public sealed record ResultadoCobroDto(
    PagoDto Pago,
    ReciboDto Recibo
);