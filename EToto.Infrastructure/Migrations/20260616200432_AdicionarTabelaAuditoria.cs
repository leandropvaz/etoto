using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EToto.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarTabelaAuditoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditoriaEntradas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NomeTabela = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ChaveRegistro = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Acao = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: true),
                    ExecutadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValoresAntes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValoresDepois = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditoriaEntradas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditoriaEntradas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaEntradas_ExecutadoEm",
                table: "AuditoriaEntradas",
                column: "ExecutadoEm",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaEntradas_NomeTabela",
                table: "AuditoriaEntradas",
                column: "NomeTabela");

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaEntradas_UsuarioId",
                table: "AuditoriaEntradas",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditoriaEntradas");
        }
    }
}
