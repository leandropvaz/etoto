using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EToto.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarModuloPle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ple",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PlantaId = table.Column<int>(type: "int", nullable: false),
                    EquipamentoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Departamento = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Operacao = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DataInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataFim = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    MotivoSuspensao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Observacoes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CriadoPorId = table.Column<int>(type: "int", nullable: false),
                    ModificadoPorId = table.Column<int>(type: "int", nullable: true),
                    FinalizadoPorId = table.Column<int>(type: "int", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    DataModificacao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataFinalizacao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ple", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ple_Equipamento_EquipamentoId",
                        column: x => x.EquipamentoId,
                        principalTable: "Equipamento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ple_Plantas_PlantaId",
                        column: x => x.PlantaId,
                        principalTable: "Plantas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ple_Usuarios_CriadoPorId",
                        column: x => x.CriadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ple_Usuarios_FinalizadoPorId",
                        column: x => x.FinalizadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ple_Usuarios_ModificadoPorId",
                        column: x => x.ModificadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PleHistorico",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Acao = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StatusAnterior = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StatusNovo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Detalhes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DataAcao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PleHistorico", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PleHistorico_Ple_PleId",
                        column: x => x.PleId,
                        principalTable: "Ple",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PleHistorico_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PleItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Tarefa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Perigo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NumeroExpostos = table.Column<int>(type: "int", nullable: false),
                    GravidadeAntes = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProbabilidadeAntes = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NivelRiscoAntes = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MedidasProtecao = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    GravidadeDepois = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProbabilidadeDepois = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NivelRiscoDepois = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PleItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PleItem_Ple_PleId",
                        column: x => x.PleId,
                        principalTable: "Ple",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ple_CriadoPorId",
                table: "Ple",
                column: "CriadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Ple_EquipamentoId",
                table: "Ple",
                column: "EquipamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Ple_FinalizadoPorId",
                table: "Ple",
                column: "FinalizadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Ple_ModificadoPorId",
                table: "Ple",
                column: "ModificadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Ple_Numero",
                table: "Ple",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ple_PlantaId",
                table: "Ple",
                column: "PlantaId");

            migrationBuilder.CreateIndex(
                name: "IX_PleHistorico_PleId",
                table: "PleHistorico",
                column: "PleId");

            migrationBuilder.CreateIndex(
                name: "IX_PleHistorico_UsuarioId",
                table: "PleHistorico",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_PleItem_PleId",
                table: "PleItem",
                column: "PleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PleHistorico");

            migrationBuilder.DropTable(
                name: "PleItem");

            migrationBuilder.DropTable(
                name: "Ple");
        }
    }
}
