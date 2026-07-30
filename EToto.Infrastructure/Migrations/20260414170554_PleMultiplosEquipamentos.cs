using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EToto.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PleMultiplosEquipamentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ple_Equipamento_EquipamentoId",
                table: "Ple");

            migrationBuilder.DropIndex(
                name: "IX_Ple_EquipamentoId",
                table: "Ple");

            migrationBuilder.DropColumn(
                name: "EquipamentoId",
                table: "Ple");

            migrationBuilder.CreateTable(
                name: "PleEquipamento",
                columns: table => new
                {
                    PleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EquipamentoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PleEquipamento", x => new { x.PleId, x.EquipamentoId });
                    table.ForeignKey(
                        name: "FK_PleEquipamento_Equipamento_EquipamentoId",
                        column: x => x.EquipamentoId,
                        principalTable: "Equipamento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PleEquipamento_Ple_PleId",
                        column: x => x.PleId,
                        principalTable: "Ple",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PleEquipamento_EquipamentoId",
                table: "PleEquipamento",
                column: "EquipamentoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PleEquipamento");

            migrationBuilder.AddColumn<Guid>(
                name: "EquipamentoId",
                table: "Ple",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Ple_EquipamentoId",
                table: "Ple",
                column: "EquipamentoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ple_Equipamento_EquipamentoId",
                table: "Ple",
                column: "EquipamentoId",
                principalTable: "Equipamento",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
