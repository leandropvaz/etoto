using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EToto.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarMultiplosPerfisPorUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ===== #1a: Auditoria de cadastro em Usuarios =====

            migrationBuilder.AddColumn<DateTime>(
                name: "AlteradoEm",
                table: "Usuarios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AlteradoPorId",
                table: "Usuarios",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CriadoEm",
                table: "Usuarios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CriadoPorId",
                table: "Usuarios",
                type: "int",
                nullable: true);

            // Backfill: CriadoEm = DataCriacao para registros existentes;
            // CriadoPorId fica nulo (não temos histórico de quem cadastrou os usuários atuais).
            // Envolto em EXEC() para diferir o parse: as colunas acabaram de ser criadas neste
            // mesmo batch e o binder do SQL Server falharia o name resolution em compile-time.
            migrationBuilder.Sql(@"
                EXEC('UPDATE Usuarios SET CriadoEm = DataCriacao, AlteradoEm = DataAtualizacao;');
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_AlteradoPorId",
                table: "Usuarios",
                column: "AlteradoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_CriadoPorId",
                table: "Usuarios",
                column: "CriadoPorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Usuarios_AlteradoPorId",
                table: "Usuarios",
                column: "AlteradoPorId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Usuarios_CriadoPorId",
                table: "Usuarios",
                column: "CriadoPorId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            // ===== #1a: Tabela de junção UsuarioPerfis =====

            migrationBuilder.CreateTable(
                name: "UsuarioPerfis",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Perfil = table.Column<int>(type: "int", nullable: false),
                    DataAssociacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioPerfis", x => new { x.UsuarioId, x.Perfil });
                    table.ForeignKey(
                        name: "FK_UsuarioPerfis_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // ===== #1a: Seed — cada usuário atual vira uma linha em UsuarioPerfis com seu Perfil legado =====
            // EXEC() difere o parse: a tabela UsuarioPerfis foi criada no mesmo batch.
            migrationBuilder.Sql(@"
                EXEC('
                    INSERT INTO UsuarioPerfis (UsuarioId, Perfil, DataAssociacao)
                    SELECT Id, Perfil, SYSUTCDATETIME()
                    FROM Usuarios
                    WHERE NOT EXISTS (
                        SELECT 1 FROM UsuarioPerfis up
                        WHERE up.UsuarioId = Usuarios.Id AND up.Perfil = Usuarios.Perfil
                    );
                ');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UsuarioPerfis");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Usuarios_AlteradoPorId",
                table: "Usuarios");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Usuarios_CriadoPorId",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_AlteradoPorId",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_CriadoPorId",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "AlteradoEm",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "AlteradoPorId",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "CriadoEm",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "CriadoPorId",
                table: "Usuarios");
        }
    }
}
