using Mercadex.Api.Seguridad;
using Mercadex.Application.DTOs.Caja;
using Mercadex.Application.Interfaces;
using Mercadex.Domain.AppSocios;
using Mercadex.Domain.Caja;
using Mercadex.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mercadex.Api.Controllers;

/// <summary>
/// Lado de la caja (escritorio) de la app de socios: dar acceso a un
/// socio, revisar los comprobantes enviados y configurar el QR de cobro.
/// Protegido por la clave interna del escritorio (X-Mercadex-Key).
/// </summary>
[ApiController]
public sealed class PagosAppController : ControllerBase
{
    private static readonly TimeSpan ZonaBolivia = TimeSpan.FromHours(-4);

    private readonly MercadexDbContext _db;
    private readonly ICajaService _caja;

    public PagosAppController(MercadexDbContext db, ICajaService caja)
    {
        _db = db;
        _caja = caja;
    }

    public sealed record RevisionRequest(string? RevisadoPor, string? Motivo);

    public sealed record ConfiguracionRequest(
        string? Banco,
        string? NumeroCuenta,
        string? TitularCuenta,
        string? Instrucciones,
        string? QrBase64,
        bool QuitarQr);

    // ===================================================== ACCESO DE SOCIOS

    [HttpGet("api/cuentas-app/{socioId:guid}")]
    public async Task<IActionResult> EstadoCuenta(Guid socioId, CancellationToken ct)
    {
        var ahora = DateTimeOffset.UtcNow;
        var cuenta = await _db.CuentasSocioApp.AsNoTracking().FirstOrDefaultAsync(c => c.SocioId == socioId, ct);

        if (cuenta == null || !cuenta.Activa)
            return Ok(new { estado = "sin-acceso", descripcion = "Sin acceso a la app." });

        if (cuenta.TieneContrasena)
        {
            return Ok(new
            {
                estado = "activa",
                descripcion = "Usa la app" + (cuenta.UltimoAccesoUtc.HasValue
                    ? $". Último ingreso: {cuenta.UltimoAccesoUtc.Value.ToOffset(ZonaBolivia):dd/MM/yyyy HH:mm}."
                    : ".")
            });
        }

        return cuenta.CodigoVigente(ahora)
            ? Ok(new
            {
                estado = "codigo-pendiente",
                descripcion = $"Código entregado, todavía sin activar. Vence el {cuenta.CodigoExpiraUtc!.Value.ToOffset(ZonaBolivia):dd/MM/yyyy}."
            })
            : Ok(new { estado = "codigo-vencido", descripcion = "El código venció sin usarse. Genera uno nuevo." });
    }

    /// <summary>
    /// Genera un código de activación nuevo. Si el socio ya usaba la app,
    /// su contraseña deja de servir y debe activarla otra vez.
    /// El código solo se muestra en esta respuesta.
    /// </summary>
    [HttpPost("api/cuentas-app/{socioId:guid}/codigo")]
    public async Task<IActionResult> GenerarCodigo(Guid socioId, CancellationToken ct)
    {
        var socio = await _db.Socios.AsNoTracking().FirstOrDefaultAsync(s => s.Id == socioId, ct);
        if (socio == null)
            return NotFound(new { mensaje = "El socio no existe." });

        if (!socio.Activo)
            return Conflict(new { mensaje = "El socio está inactivo." });

        if (string.IsNullOrWhiteSpace(socio.DocumentoIdentidad) || socio.DocumentoIdentidad == "SIN_DOCUMENTO")
            return Conflict(new { mensaje = "Registra primero el carnet del socio: con él ingresa a la app." });

        var ahora = DateTimeOffset.UtcNow;
        var cuenta = await _db.CuentasSocioApp.FirstOrDefaultAsync(c => c.SocioId == socioId, ct);
        if (cuenta == null)
        {
            cuenta = new CuentaSocioApp(socioId, ahora);
            _db.CuentasSocioApp.Add(cuenta);
        }

        var codigo = SeguridadApp.NuevoCodigo();
        var expira = ahora + SeguridadApp.DuracionCodigo;
        cuenta.AsignarCodigo(SeguridadApp.Huella(codigo), expira);

        await _db.SaveChangesAsync(ct);

        return Ok(new
        {
            codigo,
            documento = socio.DocumentoIdentidad,
            socio = $"{socio.Nombres} {socio.Apellidos}",
            expira = expira.ToOffset(ZonaBolivia)
        });
    }

    [HttpPost("api/cuentas-app/{socioId:guid}/desactivar")]
    public async Task<IActionResult> Desactivar(Guid socioId, CancellationToken ct)
    {
        var cuenta = await _db.CuentasSocioApp.FirstOrDefaultAsync(c => c.SocioId == socioId, ct);
        if (cuenta == null)
            return Ok(new { mensaje = "El socio no tenía acceso a la app." });

        cuenta.Desactivar();
        await _db.SaveChangesAsync(ct);
        return Ok(new { mensaje = "Acceso a la app retirado. Se cerraron sus sesiones." });
    }

    // ===================================================== COMPROBANTES

    [HttpGet("api/pagos-app")]
    public async Task<IActionResult> Listar([FromQuery] string? estado, CancellationToken ct)
    {
        var filtro = estado?.ToLowerInvariant() switch
        {
            "aprobado" => EstadoPagoReportado.Aprobado,
            "rechazado" => EstadoPagoReportado.Rechazado,
            "todos" => (EstadoPagoReportado?)null,
            _ => EstadoPagoReportado.Pendiente
        };

        var monedas = await _db.Monedas.AsNoTracking().ToDictionaryAsync(m => m.Id, m => m.Codigo, ct);

        var consulta =
            from p in _db.PagosReportados.AsNoTracking()
            join s in _db.Socios.AsNoTracking() on p.SocioId equals s.Id
            join u in _db.UnidadesComerciales.AsNoTracking() on p.UnidadComercialId equals u.Id
            select new { p, s.Nombres, s.Apellidos, s.DocumentoIdentidad, u.Codigo, u.Bloque };

        if (filtro.HasValue)
            consulta = consulta.Where(x => x.p.Estado == filtro.Value);

        var filas = await consulta
            .OrderBy(x => x.p.Estado)
            .ThenByDescending(x => x.p.FechaReporteUtc)
            .Take(200)
            .Select(x => new
            {
                x.p.Id, x.p.Monto, x.p.MonedaId, x.p.Referencia, x.p.FechaReporteUtc, x.p.Estado,
                x.p.MotivoRechazo, x.p.RevisadoPor, x.p.NumeroRecibo, x.p.ObligacionIds,
                x.Nombres, x.Apellidos, x.DocumentoIdentidad, x.Codigo, x.Bloque
            })
            .ToListAsync(ct);

        var todasIds = filas.SelectMany(f => f.ObligacionIds).Distinct().ToList();
        var conceptos = await (
                from o in _db.Obligaciones.AsNoTracking()
                join c in _db.ConceptosCobro.AsNoTracking() on o.ConceptoCobroId equals c.Id
                where todasIds.Contains(o.Id)
                select new { o.Id, c.Nombre, o.PeriodoAnio, o.PeriodoMes, o.Monto })
            .ToDictionaryAsync(x => x.Id, ct);

        return Ok(filas.Select(f => new
        {
            id = f.Id,
            socio = $"{f.Nombres} {f.Apellidos}",
            documento = f.DocumentoIdentidad,
            caseta = f.Codigo,
            bloque = f.Bloque,
            moneda = monedas.GetValueOrDefault(f.MonedaId, "?"),
            monto = f.Monto,
            referencia = f.Referencia,
            fecha = f.FechaReporteUtc.ToOffset(ZonaBolivia),
            estado = f.Estado switch
            {
                EstadoPagoReportado.Aprobado => "Aprobado",
                EstadoPagoReportado.Rechazado => "Rechazado",
                _ => "Pendiente"
            },
            motivoRechazo = f.MotivoRechazo,
            revisadoPor = f.RevisadoPor,
            numeroRecibo = f.NumeroRecibo,
            deudas = string.Join(", ", f.ObligacionIds.Select(id =>
                conceptos.TryGetValue(id, out var c)
                    ? c.Nombre + (c.PeriodoMes.HasValue ? $" {c.PeriodoMes:00}/{c.PeriodoAnio}" : "")
                    : "?"))
        }));
    }

    [HttpGet("api/pagos-app/{id:guid}/comprobante")]
    public async Task<IActionResult> Comprobante(Guid id, CancellationToken ct)
    {
        var p = await _db.PagosReportados.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new { x.Comprobante, x.ComprobanteTipo })
            .FirstOrDefaultAsync(ct);

        return p == null ? NotFound() : File(p.Comprobante, p.ComprobanteTipo);
    }

    /// <summary>
    /// Aprueba el comprobante: registra el cobro como pago QR con el número
    /// de transacción informado y emite el recibo, igual que en la caja.
    /// </summary>
    [HttpPost("api/pagos-app/{id:guid}/aprobar")]
    public async Task<IActionResult> Aprobar(Guid id, [FromBody] RevisionRequest r, CancellationToken ct)
    {
        var pago = await _db.PagosReportados.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (pago == null)
            return NotFound(new { mensaje = "El pago no existe." });

        if (pago.Estado != EstadoPagoReportado.Pendiente)
            return Conflict(new { mensaje = "Este pago ya fue revisado." });

        var revisadoPor = r.RevisadoPor?.Trim() ?? "";
        if (revisadoPor.Length < 3)
            return BadRequest(new { mensaje = "Escribe tu nombre (quien revisa)." });

        var socio = await _db.Socios.AsNoTracking().FirstAsync(s => s.Id == pago.SocioId, ct);

        var conceptos = await (
                from o in _db.Obligaciones.AsNoTracking()
                join c in _db.ConceptosCobro.AsNoTracking() on o.ConceptoCobroId equals c.Id
                where pago.ObligacionIds.Contains(o.Id)
                select new { o.Id, c.Nombre })
            .ToListAsync(ct);

        var resultado = await _caja.ProcesarCobroAsync(new ProcesarCobroDto(
            NumeroRecibo: 0,
            UnidadComercialId: pago.UnidadComercialId,
            MonedaId: pago.MonedaId,
            Monto: pago.Monto,
            FormaPago: (int)FormaPago.QR,
            NombrePagador: $"{socio.Nombres} {socio.Apellidos}",
            TipoCambio: 1m,
            Referencia: pago.Referencia,
            Items: conceptos.Select(c => new ItemCobroDto(c.Id, c.Nombre)).ToList()));

        // Recargar: el cobro usó su propia transacción.
        _db.ChangeTracker.Clear();
        pago = await _db.PagosReportados.FirstAsync(x => x.Id == id, ct);
        pago.Aprobar(revisadoPor, resultado.Recibo.Id, resultado.Recibo.Numero, DateTimeOffset.UtcNow);
        await _db.SaveChangesAsync(ct);

        return Ok(new
        {
            mensaje = $"Pago aprobado. Recibo N.º {resultado.Recibo.Numero} emitido.",
            reciboId = resultado.Recibo.Id,
            numeroRecibo = resultado.Recibo.Numero
        });
    }

    [HttpPost("api/pagos-app/{id:guid}/rechazar")]
    public async Task<IActionResult> Rechazar(Guid id, [FromBody] RevisionRequest r, CancellationToken ct)
    {
        var pago = await _db.PagosReportados.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (pago == null)
            return NotFound(new { mensaje = "El pago no existe." });

        pago.Rechazar(r.RevisadoPor ?? "", r.Motivo ?? "", DateTimeOffset.UtcNow);
        await _db.SaveChangesAsync(ct);

        return Ok(new { mensaje = "Pago rechazado. El socio verá el motivo en la app." });
    }

    // ===================================================== QR DE COBRO

    [HttpGet("api/configuracion-pago-app")]
    public async Task<IActionResult> Configuracion(CancellationToken ct)
    {
        var c = await _db.ConfiguracionPagoApp.AsNoTracking().FirstOrDefaultAsync(ct);
        return Ok(new
        {
            banco = c?.Banco,
            numeroCuenta = c?.NumeroCuenta,
            titularCuenta = c?.TitularCuenta,
            instrucciones = c?.Instrucciones,
            tieneQr = c?.QrImagen != null,
            qrBase64 = c?.QrImagen == null ? null : Convert.ToBase64String(c.QrImagen)
        });
    }

    [HttpPut("api/configuracion-pago-app")]
    [RequestSizeLimit(4 * 1024 * 1024)]
    public async Task<IActionResult> GuardarConfiguracion([FromBody] ConfiguracionRequest r, CancellationToken ct)
    {
        var ahora = DateTimeOffset.UtcNow;
        var c = await _db.ConfiguracionPagoApp.FirstOrDefaultAsync(ct);
        if (c == null)
        {
            c = new ConfiguracionPagoApp(ahora);
            _db.ConfiguracionPagoApp.Add(c);
        }

        c.Actualizar(r.Banco, r.NumeroCuenta, r.TitularCuenta, r.Instrucciones, ahora);

        if (r.QuitarQr)
        {
            c.CambiarQr(null, null, ahora);
        }
        else if (!string.IsNullOrWhiteSpace(r.QrBase64))
        {
            byte[] imagen;
            try { imagen = Convert.FromBase64String(r.QrBase64); }
            catch (FormatException) { return BadRequest(new { mensaje = "No se pudo leer la imagen del QR." }); }

            var tipo = imagen.Length > 3 && imagen[0] == 0xFF && imagen[1] == 0xD8 ? "image/jpeg"
                     : imagen.Length > 8 && imagen[0] == 0x89 && imagen[1] == 0x50 ? "image/png"
                     : null;

            c.CambiarQr(imagen, tipo, ahora);
        }

        await _db.SaveChangesAsync(ct);
        return Ok(new { mensaje = "Datos de pago guardados. Los socios ya los ven en la app." });
    }
}
