using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EToto.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarCampanhaRevalidacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CampanhasRevalidacao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Periodicidade = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataFimPrevista = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataFimReal = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CriadoPorId = table.Column<int>(type: "int", nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampanhasRevalidacao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampanhasRevalidacao_Usuarios_CriadoPorId",
                        column: x => x.CriadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ItensCampanhaRevalidacao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CampanhaId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Decisao = table.Column<int>(type: "int", nullable: true),
                    DecididoPorId = table.Column<int>(type: "int", nullable: true),
                    DecididoEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SnapshotUsuarioJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItensCampanhaRevalidacao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItensCampanhaRevalidacao_CampanhasRevalidacao_CampanhaId",
                        column: x => x.CampanhaId,
                        principalTable: "CampanhasRevalidacao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItensCampanhaRevalidacao_Usuarios_DecididoPorId",
                        column: x => x.DecididoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ItensCampanhaRevalidacao_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CampanhasRevalidacao_CriadoPorId",
                table: "CampanhasRevalidacao",
                column: "CriadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_CampanhasRevalidacao_DataInicio",
                table: "CampanhasRevalidacao",
                column: "DataInicio",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_CampanhasRevalidacao_Status",
                table: "CampanhasRevalidacao",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ItensCampanhaRevalidacao_CampanhaId",
                table: "ItensCampanhaRevalidacao",
                column: "CampanhaId");

            migrationBuilder.CreateIndex(
                name: "IX_ItensCampanhaRevalidacao_CampanhaId_UsuarioId",
                table: "ItensCampanhaRevalidacao",
                columns: new[] { "CampanhaId", "UsuarioId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItensCampanhaRevalidacao_DecididoPorId",
                table: "ItensCampanhaRevalidacao",
                column: "DecididoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_ItensCampanhaRevalidacao_UsuarioId",
                table: "ItensCampanhaRevalidacao",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItensCampanhaRevalidacao");

            migrationBuilder.DropTable(
                name: "CampanhasRevalidacao");
        }
    }
}
