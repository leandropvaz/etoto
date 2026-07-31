using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EToto.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarFKsAuditoriaUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Criar índice para CreateUserdAt


            // 3. Adicionar FK para CreateUserdAt com NO ACTION
            migrationBuilder.AddForeignKey(
                name: "FK_Equipamento_Usuarios_CreateUserdAt",
                table: "Equipamento",
                column: "CreateUserdAt",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction); // ← NO ACTION (Restrict)

            // 4. Adicionar FK para UpdateUserdAt com NO ACTION
            migrationBuilder.AddForeignKey(
                name: "FK_Equipamento_Usuarios_UpdateUserdAt",
                table: "Equipamento",
                column: "UpdateUserdAt",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction); // ← NO ACTION (Restrict)
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1. Remover FK CreateUserdAt
            migrationBuilder.DropForeignKey(
                name: "FK_Equipamento_Usuarios_CreateUserdAt",
                table: "Equipamento");

            // 2. Remover FK UpdateUserdAt
            migrationBuilder.DropForeignKey(
                name: "FK_Equipamento_Usuarios_UpdateUserdAt",
                table: "Equipamento");

        }
    }
}
