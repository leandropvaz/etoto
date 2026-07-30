using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EToto.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarValidadeTreinamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataValidadeTreinamento",
                table: "Usuarios",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataValidadeTreinamento",
                table: "Usuarios");
        }
    }
}
