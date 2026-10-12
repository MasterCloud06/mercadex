using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mercadex.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarAppSocios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "configuracion_pago_app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QrImagen = table.Column<byte[]>(type: "bytea", nullable: true),
                    QrTipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Banco = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    NumeroCuenta = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    TitularCuenta = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Instrucciones = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: true),
                    FechaActualizacionUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configuracion_pago_app", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "cuentas_socio_app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SocioId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContrasenaHash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CodigoActivacionHash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CodigoExpiraUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IntentosFallidos = table.Column<int>(type: "integer", nullable: false),
                    BloqueadaHastaUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Activa = table.Column<bool>(type: "boolean", nullable: false),
                    VersionSesion = table.Column<int>(type: "integer", nullable: false),
                    FechaCreacionUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FechaActivacionUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UltimoAccesoUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cuentas_socio_app", x => x.Id);
                    table.ForeignKey(
                        name: "FK_cuentas_socio_app_socios_SocioId",
                        column: x => x.SocioId,
                        principalTable: "socios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pagos_reportados",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SocioId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnidadComercialId = table.Column<Guid>(type: "uuid", nullable: false),
                    MonedaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ObligacionIds = table.Column<List<Guid>>(type: "uuid[]", nullable: false),
                    Referencia = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Comprobante = table.Column<byte[]>(type: "bytea", nullable: false),
                    ComprobanteTipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FechaReporteUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    MotivoRechazo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    RevisadoPor = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    FechaRevisionUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReciboId = table.Column<Guid>(type: "uuid", nullable: true),
                    NumeroRecibo = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pagos_reportados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_pagos_reportados_monedas_MonedaId",
                        column: x => x.MonedaId,
                        principalTable: "monedas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pagos_reportados_socios_SocioId",
                        column: x => x.SocioId,
                        principalTable: "socios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pagos_reportados_unidades_comerciales_UnidadComercialId",
                        column: x => x.UnidadComercialId,
                        principalTable: "unidades_comerciales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cuentas_socio_app_SocioId",
                table: "cuentas_socio_app",
                column: "SocioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pagos_reportados_Estado_FechaReporteUtc",
                table: "pagos_reportados",
                columns: new[] { "Estado", "FechaReporteUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_pagos_reportados_MonedaId",
                table: "pagos_reportados",
                column: "MonedaId");

            migrationBuilder.CreateIndex(
                name: "IX_pagos_reportados_SocioId",
                table: "pagos_reportados",
                column: "SocioId");

            migrationBuilder.CreateIndex(
                name: "IX_pagos_reportados_UnidadComercialId",
                table: "pagos_reportados",
                column: "UnidadComercialId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "configuracion_pago_app");

            migrationBuilder.DropTable(
                name: "cuentas_socio_app");

            migrationBuilder.DropTable(
                name: "pagos_reportados");
        }
    }
}
