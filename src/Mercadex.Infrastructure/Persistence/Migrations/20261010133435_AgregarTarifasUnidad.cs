using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mercadex.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTarifasUnidad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tarifas_unidad",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UnidadComercialId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConceptoCobroId = table.Column<Guid>(type: "uuid", nullable: false),
                    MonedaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FechaActualizacionUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tarifas_unidad", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tarifas_unidad_conceptos_cobro_ConceptoCobroId",
                        column: x => x.ConceptoCobroId,
                        principalTable: "conceptos_cobro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tarifas_unidad_monedas_MonedaId",
                        column: x => x.MonedaId,
                        principalTable: "monedas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tarifas_unidad_unidades_comerciales_UnidadComercialId",
                        column: x => x.UnidadComercialId,
                        principalTable: "unidades_comerciales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tarifas_unidad_ConceptoCobroId",
                table: "tarifas_unidad",
                column: "ConceptoCobroId");

            migrationBuilder.CreateIndex(
                name: "IX_tarifas_unidad_MonedaId",
                table: "tarifas_unidad",
                column: "MonedaId");

            migrationBuilder.CreateIndex(
                name: "IX_tarifas_unidad_UnidadComercialId_ConceptoCobroId",
                table: "tarifas_unidad",
                columns: new[] { "UnidadComercialId", "ConceptoCobroId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tarifas_unidad");
        }
    }
}
