using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mercadex.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "conceptos_cobro",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conceptos_cobro", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "monedas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Codigo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Simbolo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Activa = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_monedas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "pasillos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pasillos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "socios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombres = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Apellidos = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DocumentoIdentidad = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Telefono = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    FechaRegistro = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_socios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "pagos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MonedaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FormaPago = table.Column<int>(type: "integer", nullable: false),
                    NombrePagador = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Referencia = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    TipoCambio = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    FechaRegistro = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pagos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_pagos_monedas_MonedaId",
                        column: x => x.MonedaId,
                        principalTable: "monedas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "unidades_comerciales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    PasilloId = table.Column<Guid>(type: "uuid", nullable: true),
                    Bloque = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Activa = table.Column<bool>(type: "boolean", nullable: false),
                    FechaRegistro = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_unidades_comerciales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_unidades_comerciales_pasillos_PasilloId",
                        column: x => x.PasilloId,
                        principalTable: "pasillos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "asignaciones_unidades_comerciales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SocioId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnidadComercialId = table.Column<Guid>(type: "uuid", nullable: false),
                    TipoVinculo = table.Column<int>(type: "integer", nullable: false),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaFin = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asignaciones_unidades_comerciales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_asignaciones_unidades_comerciales_socios_SocioId",
                        column: x => x.SocioId,
                        principalTable: "socios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_asignaciones_unidades_comerciales_unidades_comerciales_Unid~",
                        column: x => x.UnidadComercialId,
                        principalTable: "unidades_comerciales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "obligaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UnidadComercialId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConceptoCobroId = table.Column<Guid>(type: "uuid", nullable: false),
                    MonedaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FechaEmision = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaVencimiento = table.Column<DateOnly>(type: "date", nullable: true),
                    PeriodoAnio = table.Column<int>(type: "integer", nullable: true),
                    PeriodoMes = table.Column<int>(type: "integer", nullable: true),
                    Estado = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_obligaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_obligaciones_conceptos_cobro_ConceptoCobroId",
                        column: x => x.ConceptoCobroId,
                        principalTable: "conceptos_cobro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_obligaciones_monedas_MonedaId",
                        column: x => x.MonedaId,
                        principalTable: "monedas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_obligaciones_unidades_comerciales_UnidadComercialId",
                        column: x => x.UnidadComercialId,
                        principalTable: "unidades_comerciales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "recibos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Numero = table.Column<long>(type: "bigint", nullable: false),
                    UnidadComercialId = table.Column<Guid>(type: "uuid", nullable: false),
                    PagoId = table.Column<Guid>(type: "uuid", nullable: false),
                    FechaEmision = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    MotivoAnulacion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FechaAnulacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recibos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_recibos_pagos_PagoId",
                        column: x => x.PagoId,
                        principalTable: "pagos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_recibos_unidades_comerciales_UnidadComercialId",
                        column: x => x.UnidadComercialId,
                        principalTable: "unidades_comerciales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "detalles_recibo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ObligacionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConceptoCobroId = table.Column<Guid>(type: "uuid", nullable: false),
                    Concepto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PeriodoAnio = table.Column<int>(type: "integer", nullable: true),
                    PeriodoMes = table.Column<int>(type: "integer", nullable: true),
                    ReciboId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_detalles_recibo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_detalles_recibo_conceptos_cobro_ConceptoCobroId",
                        column: x => x.ConceptoCobroId,
                        principalTable: "conceptos_cobro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_detalles_recibo_obligaciones_ObligacionId",
                        column: x => x.ObligacionId,
                        principalTable: "obligaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_detalles_recibo_recibos_ReciboId",
                        column: x => x.ReciboId,
                        principalTable: "recibos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_asignaciones_unidades_comerciales_SocioId",
                table: "asignaciones_unidades_comerciales",
                column: "SocioId");

            migrationBuilder.CreateIndex(
                name: "IX_asignaciones_unidades_comerciales_UnidadComercialId_SocioId~",
                table: "asignaciones_unidades_comerciales",
                columns: new[] { "UnidadComercialId", "SocioId", "FechaInicio" });

            migrationBuilder.CreateIndex(
                name: "IX_conceptos_cobro_Codigo",
                table: "conceptos_cobro",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_conceptos_cobro_Nombre",
                table: "conceptos_cobro",
                column: "Nombre");

            migrationBuilder.CreateIndex(
                name: "IX_detalles_recibo_ConceptoCobroId",
                table: "detalles_recibo",
                column: "ConceptoCobroId");

            migrationBuilder.CreateIndex(
                name: "IX_detalles_recibo_ObligacionId",
                table: "detalles_recibo",
                column: "ObligacionId");

            migrationBuilder.CreateIndex(
                name: "IX_detalles_recibo_ReciboId",
                table: "detalles_recibo",
                column: "ReciboId");

            migrationBuilder.CreateIndex(
                name: "IX_monedas_Codigo",
                table: "monedas",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_obligaciones_ConceptoCobroId_PeriodoAnio_PeriodoMes",
                table: "obligaciones",
                columns: new[] { "ConceptoCobroId", "PeriodoAnio", "PeriodoMes" });

            migrationBuilder.CreateIndex(
                name: "IX_obligaciones_MonedaId",
                table: "obligaciones",
                column: "MonedaId");

            migrationBuilder.CreateIndex(
                name: "IX_obligaciones_UnidadComercialId_Estado",
                table: "obligaciones",
                columns: new[] { "UnidadComercialId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_pagos_FechaRegistro",
                table: "pagos",
                column: "FechaRegistro");

            migrationBuilder.CreateIndex(
                name: "IX_pagos_MonedaId",
                table: "pagos",
                column: "MonedaId");

            migrationBuilder.CreateIndex(
                name: "IX_pagos_Referencia",
                table: "pagos",
                column: "Referencia");

            migrationBuilder.CreateIndex(
                name: "IX_pasillos_Codigo",
                table: "pasillos",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recibos_FechaEmision",
                table: "recibos",
                column: "FechaEmision");

            migrationBuilder.CreateIndex(
                name: "IX_recibos_Numero",
                table: "recibos",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recibos_PagoId",
                table: "recibos",
                column: "PagoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recibos_UnidadComercialId",
                table: "recibos",
                column: "UnidadComercialId");

            migrationBuilder.CreateIndex(
                name: "IX_socios_DocumentoIdentidad",
                table: "socios",
                column: "DocumentoIdentidad");

            migrationBuilder.CreateIndex(
                name: "IX_unidades_comerciales_Codigo",
                table: "unidades_comerciales",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_unidades_comerciales_PasilloId",
                table: "unidades_comerciales",
                column: "PasilloId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "asignaciones_unidades_comerciales");

            migrationBuilder.DropTable(
                name: "detalles_recibo");

            migrationBuilder.DropTable(
                name: "socios");

            migrationBuilder.DropTable(
                name: "obligaciones");

            migrationBuilder.DropTable(
                name: "recibos");

            migrationBuilder.DropTable(
                name: "conceptos_cobro");

            migrationBuilder.DropTable(
                name: "pagos");

            migrationBuilder.DropTable(
                name: "unidades_comerciales");

            migrationBuilder.DropTable(
                name: "monedas");

            migrationBuilder.DropTable(
                name: "pasillos");
        }
    }
}
