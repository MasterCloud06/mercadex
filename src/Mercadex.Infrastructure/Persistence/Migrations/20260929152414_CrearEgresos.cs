using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mercadex.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CrearEgresos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "egresos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    Categoria = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Concepto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Beneficiario = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MonedaId = table.Column<Guid>(type: "uuid", nullable: false),
                    FormaPago = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NumeroComprobante = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Observacion = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FechaRegistro = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Anulado = table.Column<bool>(type: "boolean", nullable: false),
                    FechaAnulacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    MotivoAnulacion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_egresos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_egresos_monedas_MonedaId",
                        column: x => x.MonedaId,
                        principalTable: "monedas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_egresos_Categoria",
                table: "egresos",
                column: "Categoria");

            migrationBuilder.CreateIndex(
                name: "IX_egresos_Fecha",
                table: "egresos",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_egresos_MonedaId",
                table: "egresos",
                column: "MonedaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "egresos");
        }
    }
}
