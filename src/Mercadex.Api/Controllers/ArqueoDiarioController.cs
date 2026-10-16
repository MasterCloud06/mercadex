using System.Security.Claims;
using Mercadex.Api.Seguridad;
using Mercadex.Domain.Caja;
using Mercadex.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mercadex.Api.Controllers;

[ApiController]
[Route("api/arqueo-diario")]
public sealed class ArqueoDiarioController : ControllerBase
{
    private static readonly TimeSpan ZonaBolivia = TimeSpan.FromHours(-4);

    private readonly MercadexDbContext _db;

    public ArqueoDiarioController(MercadexDbContext db)
    {
        _db = db;
    }

    // =============================================================
    // CONSULTA DEL DÍA (con estado de cierre)
    // =============================================================

    /// <param name="caja">
    /// «mia» (por defecto): la caja de quien consulta. «general»: todo el
    /// día, de todas las cajeras. Otro valor: la caja de esa cajera (solo
    /// administrador y consulta). Sin sesión siempre es la caja general.
    /// </param>
    [HttpGet("preliminar")]
    public async Task<IActionResult> ObtenerPreliminar(
        [FromQuery] DateOnly fecha,
        [FromQuery] string? caja,
        CancellationToken cancellationToken)
    {
        if (fecha == default)
        {
            return BadRequest(new
            {
                mensaje = "Debe indicar una fecha válida."
            });
        }

        var (cajeroId, cajeroNombre, error) = ResolverCaja(caja);

        if (error is not null)
            return StatusCode(403, new { mensaje = error });

        var filtro = cajeroId == CierreCaja.CajaGeneral ? null : cajeroId;
        var calculo = await CalcularAsync(fecha, filtro, cancellationToken);

        var cierresDia = await _db.CierresCaja
            .AsNoTracking()
            .Where(c => c.Fecha == fecha)
            .ToListAsync(cancellationToken);

        var cierres = cierresDia.Where(c => c.CajeroId == cajeroId).ToList();

        if (cajeroNombre is null && cajeroId != CierreCaja.CajaGeneral)
        {
            cajeroNombre = calculo.Cajas.FirstOrDefault(c => c.CajeroId == cajeroId)?.Nombre ??
                           cierres.FirstOrDefault()?.CajeroNombre;
        }

        var resumen = calculo.Resumen
            .Select(r =>
            {
                var cierre = cierres.FirstOrDefault(c => c.MonedaId == r.MonedaId);

                return new
                {
                    monedaId = r.MonedaId,
                    moneda = r.Moneda,
                    simbolo = r.Simbolo,
                    efectivo = r.Efectivo,
                    qr = r.Qr,
                    otros = r.Otros,
                    total = r.Total,
                    cantidad = r.Cantidad,
                    cantidadEfectivo = r.CantidadEfectivo,
                    cantidadQr = r.CantidadQr,
                    cantidadOtros = r.CantidadOtros,
                    egresosEfectivo = r.EgresosEfectivo,
                    cantidadEgresos = r.CantidadEgresos,
                    efectivoEsperado = r.EfectivoEsperado,
                    cerrado = cierre != null,
                    cierre = cierre == null ? null : (object?)AResumenCierre(cierre, r.Moneda),
                    // Cobros o anulaciones posteriores al cierre.
                    cambiosPosteriores = cierre != null &&
                        (cierre.Total != r.Total || cierre.CantidadPagos != r.Cantidad ||
                         cierre.EgresosEfectivo != r.EgresosEfectivo)
                };
            })
            .ToList();

        // Monedas cerradas que hoy ya no tienen cobros válidos.
        foreach (var cierre in cierres.Where(c => resumen.All(r => r.monedaId != c.MonedaId)))
        {
            var codigo = await _db.Monedas
                .Where(m => m.Id == cierre.MonedaId)
                .Select(m => new { m.Codigo, m.Simbolo })
                .FirstAsync(cancellationToken);

            resumen.Add(new
            {
                monedaId = cierre.MonedaId,
                moneda = codigo.Codigo,
                simbolo = codigo.Simbolo,
                efectivo = 0m,
                qr = 0m,
                otros = 0m,
                total = 0m,
                cantidad = 0,
                cantidadEfectivo = 0,
                cantidadQr = 0,
                cantidadOtros = 0,
                egresosEfectivo = 0m,
                cantidadEgresos = 0,
                efectivoEsperado = 0m,
                cerrado = true,
                cierre = (object?)AResumenCierre(cierre, codigo.Codigo),
                cambiosPosteriores = true
            });
        }

        // Cajas del día (de cada cajera), para que el administrador vea quién ya cerró.
        var cajasDia = calculo.Cajas
            .Select(c => c.CajeroId)
            .Union(cierresDia.Where(x => !x.EsGeneral).Select(x => x.CajeroId))
            .Distinct()
            .Select(id =>
            {
                var datos = calculo.Cajas.FirstOrDefault(c => c.CajeroId == id);
                var cierresCaja = cierresDia.Where(x => x.CajeroId == id).ToList();
                var monedasConMovimiento = datos?.Monedas ?? new List<Guid>();

                return new
                {
                    cajeroId = id,
                    nombre = datos?.Nombre ?? cierresCaja.FirstOrDefault()?.CajeroNombre ?? "Sin nombre",
                    cantidadCobros = datos?.CantidadCobros ?? 0,
                    cantidadEgresos = datos?.CantidadEgresos ?? 0,
                    totales = datos?.Totales.Select(t => new { moneda = t.Moneda, total = t.Total, efectivo = t.Efectivo }),
                    cerrada = monedasConMovimiento.Count > 0
                        ? monedasConMovimiento.All(m => cierresCaja.Any(x => x.MonedaId == m))
                        : cierresCaja.Count > 0,
                    esMia = id == IdUsuario()
                };
            })
            .OrderBy(c => c.nombre)
            .ToList();

        return Ok(new
        {
            fecha = fecha.ToString("yyyy-MM-dd"),
            zonaHoraria = "Bolivia UTC-04:00",
            caja = cajeroId,
            cajaNombre = cajeroId == CierreCaja.CajaGeneral ? "Caja general (todo el día)" : cajeroNombre,
            esCajaGeneral = cajeroId == CierreCaja.CajaGeneral,
            cajas = cajasDia,
            sinCajera = calculo.CantidadSinCajera,
            estado = cierres.Count > 0 ? "Con cierre" : "Preliminar",
            cantidadPagosValidos = calculo.CantidadValidos,
            cantidadIncidencias = calculo.Incidencias.Sum(i => i.Cantidad),
            resumen,
            pagosPorRevisar = calculo.Incidencias.Select(i => new
            {
                moneda = i.Moneda,
                cantidad = i.Cantidad,
                monto = i.Monto
            }),
            mensaje = calculo.Incidencias.Count > 0
                ? "Existen pagos que requieren revisión antes del cierre."
                : "Consulta generada correctamente."
        });
    }

    // =============================================================
    // REGISTRAR CIERRE DEL DÍA
    // =============================================================

    public sealed record RegistrarCierreRequest(
        DateOnly Fecha,
        Guid MonedaId,
        decimal EfectivoContado,
        string Responsable,
        string? Observacion,
        string? Caja = null);

    [HttpPost("cierres")]
    public async Task<IActionResult> RegistrarCierre(
        [FromBody] RegistrarCierreRequest solicitud,
        CancellationToken cancellationToken)
    {
        if (solicitud is null || solicitud.Fecha == default || solicitud.MonedaId == Guid.Empty)
        {
            return BadRequest(new { mensaje = "Indique la fecha y la moneda del cierre." });
        }

        var hoy = DateOnly.FromDateTime(DateTimeOffset.UtcNow.ToOffset(ZonaBolivia).DateTime);

        if (solicitud.Fecha > hoy)
        {
            return BadRequest(new { mensaje = "No se puede cerrar una fecha futura." });
        }

        // Cada cajera cierra su propia caja. El administrador puede cerrar
        // la suya o la caja general (todo el día). Sin sesión: caja general.
        var (cajeroId, cajeroNombre, error) = ResolverCaja(solicitud.Caja, paraCerrar: true);

        if (error is not null)
            return StatusCode(403, new { mensaje = error });

        var yaCerrado = await _db.CierresCaja.AnyAsync(c =>
            c.Fecha == solicitud.Fecha &&
            c.MonedaId == solicitud.MonedaId &&
            c.CajeroId == cajeroId, cancellationToken);

        if (yaCerrado)
        {
            return Conflict(new { mensaje = MensajeYaCerrada(cajeroId) });
        }

        // El servidor recalcula los totales: no se confía en el cliente.
        var calculo = await CalcularAsync(
            solicitud.Fecha,
            cajeroId == CierreCaja.CajaGeneral ? null : cajeroId,
            cancellationToken);

        var resumen = calculo.Resumen.FirstOrDefault(r => r.MonedaId == solicitud.MonedaId);

        var moneda = await _db.Monedas
            .AsNoTracking()
            .Where(m => m.Id == solicitud.MonedaId)
            .Select(m => new { m.Codigo })
            .FirstOrDefaultAsync(cancellationToken);

        if (moneda is null)
        {
            return BadRequest(new { mensaje = "La moneda indicada no existe." });
        }

        if (calculo.Incidencias.Any(i => i.Moneda == moneda.Codigo))
        {
            return Conflict(new
            {
                mensaje = "Hay pagos de esta moneda que requieren revisión. " +
                          "Resuélvalos antes de cerrar la caja."
            });
        }

        var cierre = new CierreCaja(
            solicitud.Fecha,
            solicitud.MonedaId,
            resumen?.Efectivo ?? 0,
            resumen?.Qr ?? 0,
            resumen?.Otros ?? 0,
            resumen?.CantidadEfectivo ?? 0,
            resumen?.CantidadQr ?? 0,
            resumen?.CantidadOtros ?? 0,
            solicitud.EfectivoContado,
            // Con sesión iniciada el responsable es el usuario; sin sesión
            // (instaladores anteriores) se usa el nombre escrito.
            User.Identity?.IsAuthenticated == true && !string.IsNullOrWhiteSpace(User.Identity.Name)
                ? User.Identity.Name!
                : solicitud.Responsable,
            solicitud.Observacion,
            DateTimeOffset.UtcNow,
            resumen?.EgresosEfectivo ?? 0,
            resumen?.CantidadEgresos ?? 0,
            cajeroId,
            cajeroNombre);

        _db.CierresCaja.Add(cierre);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Dos cajas cerrando al mismo tiempo: gana la primera.
            return Conflict(new { mensaje = MensajeYaCerrada(cajeroId) });
        }

        HttpContext.Detalle(
            $"{(cierre.EsGeneral ? "Caja general" : "Caja de " + (cierre.CajeroNombre ?? cierre.Responsable))} · " +
            $"{cierre.Fecha:dd/MM/yyyy} · {moneda.Codigo} · Esperado {Cambios.Monto(cierre.EfectivoEsperado, moneda.Codigo)} · " +
            $"Contado {Cambios.Monto(cierre.EfectivoContado, moneda.Codigo)} · Diferencia {Cambios.Monto(cierre.Diferencia, moneda.Codigo)}");

        return Ok(AResumenCierre(cierre, moneda.Codigo));
    }

    // =============================================================
    // HISTORIAL DE CIERRES
    // =============================================================

    [HttpGet("cierres")]
    public async Task<IActionResult> ListarCierres(
        [FromQuery] DateOnly? desde,
        [FromQuery] DateOnly? hasta,
        CancellationToken cancellationToken)
    {
        var hoy = DateOnly.FromDateTime(DateTimeOffset.UtcNow.ToOffset(ZonaBolivia).DateTime);
        var fin = hasta ?? hoy;
        var inicio = desde ?? fin.AddDays(-30);

        if (inicio > fin)
        {
            return BadRequest(new { mensaje = "La fecha inicial no puede superar la final." });
        }

        var monedas = await _db.Monedas
            .AsNoTracking()
            .ToDictionaryAsync(m => m.Id, m => m.Codigo, cancellationToken);

        var consulta = _db.CierresCaja
            .AsNoTracking()
            .Where(c => c.Fecha >= inicio && c.Fecha <= fin);

        // La cajera ve sus propios cierres; administración y consulta, todos.
        if (User.IsInRole(SeguridadUsuarios.RolCajero) && IdUsuario() is { } mio)
            consulta = consulta.Where(c => c.CajeroId == mio);

        var cierres = await consulta
            .OrderByDescending(c => c.Fecha)
            .ThenBy(c => c.CajeroNombre)
            .ThenBy(c => c.MonedaId)
            .ToListAsync(cancellationToken);

        return Ok(cierres.Select(c => AResumenCierre(c, monedas.GetValueOrDefault(c.MonedaId, "?"))));
    }

    // =============================================================
    // CÁLCULO COMÚN
    // =============================================================

    private static object AResumenCierre(CierreCaja c, string moneda) => new
    {
        id = c.Id,
        fecha = c.Fecha.ToString("yyyy-MM-dd"),
        monedaId = c.MonedaId,
        moneda,
        efectivo = c.Efectivo,
        qr = c.Qr,
        otros = c.Otros,
        total = c.Total,
        cantidadPagos = c.CantidadPagos,
        cantidadEfectivo = c.CantidadEfectivo,
        cantidadQr = c.CantidadQr,
        cantidadOtros = c.CantidadOtros,
        egresosEfectivo = c.EgresosEfectivo,
        cantidadEgresos = c.CantidadEgresos,
        efectivoEsperado = c.EfectivoEsperado,
        efectivoContado = c.EfectivoContado,
        diferencia = c.Diferencia,
        responsable = c.Responsable,
        observacion = c.Observacion,
        fechaRegistro = c.FechaRegistroUtc,
        caja = c.CajeroId,
        cajaNombre = c.EsGeneral ? "Caja general" : c.CajeroNombre ?? c.Responsable,
        esCajaGeneral = c.EsGeneral
    };

    // =============================================================
    // DE QUIÉN ES LA CAJA
    // =============================================================

    /// <summary>Identificador de quien consulta (usuario o «administrador»); null sin sesión.</summary>
    private string? IdUsuario() => SeguridadUsuarios.IdResponsable(User);

    private static string MensajeYaCerrada(string cajeroId) =>
        cajeroId == CierreCaja.CajaGeneral
            ? "La caja general de esa fecha y moneda ya fue cerrada."
            : "Esta caja ya fue cerrada para esa fecha y moneda.";

    private (string CajeroId, string? Nombre, string? Error) ResolverCaja(string? caja, bool paraCerrar = false)
    {
        var mio = IdUsuario();
        var nombre = User.Identity?.IsAuthenticated == true ? User.Identity.Name : null;
        caja = string.IsNullOrWhiteSpace(caja) ? "mia" : caja.Trim();

        // Sin sesión (instaladores anteriores): como antes, la caja de todo el día.
        if (mio is null)
            return (CierreCaja.CajaGeneral, null, null);

        if (caja.Equals("mia", StringComparison.OrdinalIgnoreCase) || caja == mio)
            return (mio, nombre, null);

        var esAdmin = User.IsInRole(SeguridadUsuarios.RolAdministrador);
        var puedeVer = esAdmin || User.IsInRole(SeguridadUsuarios.RolConsulta);

        if (caja.Equals(CierreCaja.CajaGeneral, StringComparison.OrdinalIgnoreCase))
        {
            if (paraCerrar ? !esAdmin : !puedeVer)
                return (mio, nombre, "Solo el administrador puede ver o cerrar la caja general.");

            return (CierreCaja.CajaGeneral, null, null);
        }

        // La caja de otra cajera: se puede ver, pero la cierra ella misma.
        if (paraCerrar)
            return (mio, nombre, "Cada cajera cierra su propia caja.");

        if (!puedeVer)
            return (mio, nombre, "Solo puedes ver tu propia caja.");

        return (caja.Length > 64 ? caja[..64] : caja, null, null);
    }

    private sealed record ResumenMoneda(
        Guid MonedaId,
        string Moneda,
        string Simbolo,
        decimal Efectivo,
        decimal Qr,
        decimal Otros,
        decimal Total,
        int Cantidad,
        int CantidadEfectivo,
        int CantidadQr,
        int CantidadOtros,
        decimal EgresosEfectivo = 0m,
        int CantidadEgresos = 0)
    {
        /// <summary>Lo que debe haber en caja: cobrado en efectivo menos gastado en efectivo.</summary>
        public decimal EfectivoEsperado => Efectivo - EgresosEfectivo;
    }

    private sealed record Incidencia(string Moneda, int Cantidad, decimal Monto);

    private sealed record TotalCaja(string Moneda, decimal Total, decimal Efectivo);

    private sealed record CajaDia(
        string CajeroId,
        string Nombre,
        int CantidadCobros,
        int CantidadEgresos,
        List<TotalCaja> Totales,
        List<Guid> Monedas);

    private sealed record Calculo(
        List<ResumenMoneda> Resumen,
        List<Incidencia> Incidencias,
        int CantidadValidos,
        List<CajaDia> Cajas,
        int CantidadSinCajera);

    /// <param name="cajeroId">null: todo el día. Si no, solo lo que registró esa cajera.</param>
    private async Task<Calculo> CalcularAsync(DateOnly fecha, string? cajeroId, CancellationToken ct)
    {
        // Horario de Bolivia: inicio inclusivo, día siguiente exclusivo.
        var inicio = new DateTimeOffset(fecha.ToDateTime(TimeOnly.MinValue), ZonaBolivia);
        var inicioUtc = inicio.ToUniversalTime();
        var finUtc = inicio.AddDays(1).ToUniversalTime();

        // Una fila por pago y recibo (sin asignaciones de socios,
        // para no duplicar por titularidad).
        var registros = await (
            from pago in _db.Pagos.AsNoTracking()
            join moneda in _db.Monedas.AsNoTracking() on pago.MonedaId equals moneda.Id
            join recibo in _db.Recibos.AsNoTracking() on pago.Id equals recibo.PagoId into grupoRecibos
            from recibo in grupoRecibos.DefaultIfEmpty()
            where pago.FechaRegistro >= inicioUtc && pago.FechaRegistro < finUtc
            select new
            {
                pago.MonedaId,
                pago.Monto,
                pago.FormaPago,
                pago.Estado,
                Moneda = moneda.Codigo,
                moneda.Simbolo,
                EstadoRecibo = recibo == null ? (EstadoRecibo?)null : recibo.Estado,
                CajeroId = recibo == null ? null : recibo.RegistradoPorId,
                Cajero = recibo == null ? null : recibo.RegistradoPor
            }).ToListAsync(ct);

        var egresosDia = await (
                from e in _db.Egresos.AsNoTracking()
                join m in _db.Monedas.AsNoTracking() on e.MonedaId equals m.Id
                where e.Fecha == fecha && !e.Anulado && e.FormaPago.ToLower() == "efectivo"
                select new { e.MonedaId, m.Codigo, m.Simbolo, e.Monto, e.RegistradoPorId, e.RegistradoPor })
            .ToListAsync(ct);

        // Cajas de cada cajera (cobros y egresos con usuario identificado).
        var validosDia = registros
            .Where(x => x.Estado == EstadoPago.Registrado && x.EstadoRecibo == EstadoRecibo.Emitido)
            .ToList();

        var cajas = validosDia.Where(x => x.CajeroId != null).Select(x => x.CajeroId!)
            .Union(egresosDia.Where(x => x.RegistradoPorId != null).Select(x => x.RegistradoPorId!))
            .Distinct()
            .Select(id => new CajaDia(
                id,
                validosDia.Where(x => x.CajeroId == id).Select(x => x.Cajero).LastOrDefault(n => !string.IsNullOrWhiteSpace(n)) ??
                egresosDia.Where(x => x.RegistradoPorId == id).Select(x => x.RegistradoPor).LastOrDefault(n => !string.IsNullOrWhiteSpace(n)) ??
                "Sin nombre",
                validosDia.Count(x => x.CajeroId == id),
                egresosDia.Count(x => x.RegistradoPorId == id),
                validosDia.Where(x => x.CajeroId == id)
                    .GroupBy(x => x.Moneda)
                    .Select(g => new TotalCaja(g.Key, g.Sum(x => x.Monto),
                        g.Where(x => x.FormaPago == FormaPago.Efectivo).Sum(x => x.Monto)))
                    .ToList(),
                validosDia.Where(x => x.CajeroId == id).Select(x => x.MonedaId)
                    .Union(egresosDia.Where(x => x.RegistradoPorId == id).Select(x => x.MonedaId))
                    .Distinct()
                    .ToList()))
            .ToList();

        var sinCajera = validosDia.Count(x => x.CajeroId == null);

        if (cajeroId is not null)
        {
            registros = registros.Where(x => x.CajeroId == cajeroId).ToList();
            egresosDia = egresosDia.Where(x => x.RegistradoPorId == cajeroId).ToList();
        }

        var validos = registros
            .Where(x => x.Estado == EstadoPago.Registrado && x.EstadoRecibo == EstadoRecibo.Emitido)
            .ToList();

        var incidencias = registros
            .Where(x => x.Estado == EstadoPago.Registrado && x.EstadoRecibo != EstadoRecibo.Emitido)
            .GroupBy(x => x.Moneda)
            .Select(g => new Incidencia(g.Key, g.Count(), g.Sum(x => x.Monto)))
            .OrderBy(x => x.Moneda)
            .ToList();

        // "Otros" incluye Transferencia (histórico) y Otro.
        var resumen = validos
            .GroupBy(x => new { x.MonedaId, x.Moneda, x.Simbolo })
            .Select(g => new ResumenMoneda(
                g.Key.MonedaId,
                g.Key.Moneda,
                g.Key.Simbolo,
                g.Where(x => x.FormaPago == FormaPago.Efectivo).Sum(x => x.Monto),
                g.Where(x => x.FormaPago == FormaPago.QR).Sum(x => x.Monto),
                g.Where(x => x.FormaPago != FormaPago.Efectivo && x.FormaPago != FormaPago.QR).Sum(x => x.Monto),
                g.Sum(x => x.Monto),
                g.Count(),
                g.Count(x => x.FormaPago == FormaPago.Efectivo),
                g.Count(x => x.FormaPago == FormaPago.QR),
                g.Count(x => x.FormaPago != FormaPago.Efectivo && x.FormaPago != FormaPago.QR)))
            .ToList();

        // Egresos (gastos) pagados en efectivo ese día: salen de la caja.
        var egresos = egresosDia
            .GroupBy(e => new { e.MonedaId, e.Codigo, e.Simbolo })
            .Select(g => new { g.Key.MonedaId, g.Key.Codigo, g.Key.Simbolo, Total = g.Sum(x => x.Monto), Cantidad = g.Count() })
            .ToList();

        foreach (var eg in egresos)
        {
            var i = resumen.FindIndex(r => r.MonedaId == eg.MonedaId);

            resumen = i >= 0
                ? resumen.Select((r, j) => j == i ? r with { EgresosEfectivo = eg.Total, CantidadEgresos = eg.Cantidad } : r).ToList()
                : resumen.Append(new ResumenMoneda(eg.MonedaId, eg.Codigo, eg.Simbolo, 0, 0, 0, 0, 0, 0, 0, 0, eg.Total, eg.Cantidad)).ToList();
        }

        resumen = resumen
            .OrderBy(x => x.Moneda == "BOB" ? 0 : 1)
            .ThenBy(x => x.Moneda)
            .ToList();

        return new Calculo(resumen, incidencias, validos.Count, cajas, sinCajera);
    }
}
