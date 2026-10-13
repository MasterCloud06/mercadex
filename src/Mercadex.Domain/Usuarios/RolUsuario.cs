namespace Mercadex.Domain.Usuarios;

/// <summary>Roles de Mercadex de escritorio y del panel de caja.</summary>
public enum RolUsuario
{
    /// <summary>Todo: usuarios, tarifas, generador, modificar deudas, anular cobros.</summary>
    Administrador = 1,

    /// <summary>Cobrar, reimprimir, extracto, arqueo del día y reportes.</summary>
    Cajero = 2,

    /// <summary>Solo ver reportes y listas, sin modificar nada.</summary>
    Consulta = 3
}
