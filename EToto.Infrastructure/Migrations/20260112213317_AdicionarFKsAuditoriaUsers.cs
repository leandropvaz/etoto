using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EToto.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarFKsAuditoriaUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ===== PARTE 1: CRIAR COLUNAS =====

            migrationBuilder.AddColumn<int>(
                name: "CreateUserId",
                table: "Equipamento",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdateUserId",
                table: "Equipamento",
                type: "int",
                nullable: true);

            // ===== PARTE 2: CRIAR ÍNDICES =====

            migrationBuilder.CreateIndex(
                name: "IX_Equipamento_CreateUserId",
                table: "Equipamento",
                column: "CreateUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipamento_UpdateUserId",
                table: "Equipamento",
                column: "UpdateUserId");

            // ===== PARTE 3: CRIAR FOREIGN KEYS COM NO ACTION =====

            // IMPORTANTE: Ajuste "Usuarios" para "Usuario" se sua tabela for singular

            migrationBuilder.AddForeignKey(
                name: "FK_Equipamento_Usuarios_CreateUserId",
                table: "Equipamento",
                column: "CreateUserId",
                principalTable: "Usuarios",  // ← Mude para "Usuario" se necessário
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Equipamento_Usuarios_UpdateUserId",
                table: "Equipamento",
                column: "UpdateUserId",
                principalTable: "Usuarios",  // ← Mude para "Usuario" se necessário
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1. Remover Foreign Keys
            migrationBuilder.DropForeignKey(
                name: "FK_Equipamento_Usuarios_CreateUserId",
                table: "Equipamento");

            migrationBuilder.DropForeignKey(
                name: "FK_Equipamento_Usuarios_UpdateUserId",
                table: "Equipamento");

            // 2. Remover Índices
            migrationBuilder.DropIndex(
                name: "IX_Equipamento_CreateUserId",
                table: "Equipamento");

            migrationBuilder.DropIndex(
                name: "IX_Equipamento_UpdateUserId",
                table: "Equipamento");

            // 3. Remover Colunas
            migrationBuilder.DropColumn(
                name: "CreateUserId",
                table: "Equipamento");

            migrationBuilder.DropColumn(
                name: "UpdateUserId",
                table: "Equipamento");
        }
    }
}
