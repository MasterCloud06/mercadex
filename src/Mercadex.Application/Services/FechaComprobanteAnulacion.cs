using System.Globalization;

namespace Mercadex.Application.Services;

// Permite conservar la fecha de emision independientemente
// de si el DTO utiliza DateOnly, DateTime o DateTimeOffset.

internal static class FechaComprobanteAnulacion
{
    public static DateTimeOffset Convertir(object? valor)
    {
        return valor switch
        {
            DateTimeOffset fecha =>
                fecha.ToUniversalTime(),

            DateTime fecha when fecha.Kind == DateTimeKind.Unspecified =>
                new DateTimeOffset(
                    DateTime.SpecifyKind(
                        fecha,
                        DateTimeKind.Utc)),

            DateTime fecha =>
                new DateTimeOffset(fecha.ToUniversalTime()),

            DateOnly fecha =>
                new DateTimeOffset(
                    fecha.ToDateTime(
                        TimeOnly.MinValue,
                        DateTimeKind.Utc)),

            string texto when DateTimeOffset.TryParse(
                texto,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal |
                DateTimeStyles.AdjustToUniversal,
                out var fecha) =>
                    fecha.ToUniversalTime(),

            _ => throw new InvalidOperationException(
                "La fecha de emision del recibo no es valida.")
        };
    }
}