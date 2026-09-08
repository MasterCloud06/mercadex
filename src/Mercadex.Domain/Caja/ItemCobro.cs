using Mercadex.Domain.Cobranza;

namespace Mercadex.Domain.Caja;

public sealed record ItemCobro(
    Obligacion Obligacion,
    string NombreConcepto);
