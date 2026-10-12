using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mercadex.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDocumentosKardex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "documentos_kardex",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SocioId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnidadComercialId = table.Column<Guid>(type: "uuid", nullable: true),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    NombreArchivo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    TipoContenido = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TamanoBytes = table.Column<long>(type: "bigint", nullable: false),
                    Sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ClaveArchivo = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RegistradoPor = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    FechaRegistroUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_documentos_kardex", x => x.Id);
                    table.ForeignKey(
                        name: "FK_documentos_kardex_socios_SocioId",
                        column: x => x.SocioId,
                        principalTable: "socios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_documentos_kardex_unidades_comerciales_UnidadComercialId",
                        column: x => x.UnidadComercialId,
                        principalTable: "unidades_comerciales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_documentos_kardex_ClaveArchivo",
                table: "documentos_kardex",
                column: "ClaveArchivo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_documentos_kardex_SocioId_Tipo",
                table: "documentos_kardex",
                columns: new[] { "SocioId", "Tipo" });

            migrationBuilder.CreateIndex(
                name: "IX_documentos_kardex_UnidadComercialId",
                table: "documentos_kardex",
                column: "UnidadComercialId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "documentos_kardex");
        }
    }
}
