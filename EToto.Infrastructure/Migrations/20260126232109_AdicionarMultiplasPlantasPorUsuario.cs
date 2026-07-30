using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EToto.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarMultiplasPlantasPorUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UsuarioPlantas",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    PlantaId = table.Column<int>(type: "int", nullable: false),
                    DataAssociacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioPlantas", x => new { x.UsuarioId, x.PlantaId });
                    table.ForeignKey(
                        name: "FK_UsuarioPlantas_Plantas_PlantaId",
                        column: x => x.PlantaId,
                        principalTable: "Plantas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsuarioPlantas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioPlantas_PlantaId",
                table: "UsuarioPlantas",
                column: "PlantaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UsuarioPlantas");
        }
    }
}
