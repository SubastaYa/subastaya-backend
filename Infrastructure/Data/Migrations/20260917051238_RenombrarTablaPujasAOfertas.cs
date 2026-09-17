using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenombrarTablaPujasAOfertas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Pujas",
                newName: "Ofertas");

            migrationBuilder.RenameColumn(
                name: "FechaPuja",
                table: "Ofertas",
                newName: "FechaOferta");

            migrationBuilder.RenameIndex(
                name: "IX_Pujas_CompradorId",
                table: "Ofertas",
                newName: "IX_Ofertas_CompradorId");

            migrationBuilder.RenameIndex(
                name: "IX_Pujas_SubastaId",
                table: "Ofertas",
                newName: "IX_Ofertas_SubastaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_Ofertas_SubastaId",
                table: "Ofertas",
                newName: "IX_Pujas_SubastaId");

            migrationBuilder.RenameIndex(
                name: "IX_Ofertas_CompradorId",
                table: "Ofertas",
                newName: "IX_Pujas_CompradorId");

            migrationBuilder.RenameColumn(
                name: "FechaOferta",
                table: "Ofertas",
                newName: "FechaPuja");

            migrationBuilder.RenameTable(
                name: "Ofertas",
                newName: "Pujas");
        }
    }
}
