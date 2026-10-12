using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mercadex.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarComprobantesAnulacionRecibo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "comprobantes_anulacion_recibo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReciboId = table.Column<Guid>(type: "uuid", nullable: false),
                    NumeroRecibo = table.Column<long>(type: "bigint", nullable: false),
                    UnidadComercialId = table.Column<Guid>(type: "uuid", nullable: false),
                    PagoId = table.Column<Guid>(type: "uuid", nullable: false),
                    MonedaId = table.Column<Guid>(type: "uuid", nullable: false),
                    MontoTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FechaEmisionRecibo = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FechaAnulacionUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Motivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Responsable = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Referencia = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comprobantes_anulacion_recibo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_comprobantes_anulacion_recibo_recibos_ReciboId",
                        column: x => x.ReciboId,
                        principalTable: "recibos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_comprobantes_anulacion_recibo_FechaAnulacionUtc",
                table: "comprobantes_anulacion_recibo",
                column: "FechaAnulacionUtc");

            migrationBuilder.CreateIndex(
                name: "IX_comprobantes_anulacion_recibo_ReciboId",
                table: "comprobantes_anulacion_recibo",
                column: "ReciboId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_comprobantes_anulacion_recibo_Referencia",
                table: "comprobantes_anulacion_recibo",
                column: "Referencia",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "comprobantes_anulacion_recibo");
        }
    }
}
