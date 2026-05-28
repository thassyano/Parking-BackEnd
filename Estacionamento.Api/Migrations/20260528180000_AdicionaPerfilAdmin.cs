using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Estacionamento.Api.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaPerfilAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Perfil",
                table: "Admins",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Admin");

            migrationBuilder.Sql("""
                UPDATE "Admins"
                SET "Perfil" = 'AdminMaster'
                WHERE LOWER("Usuario") IN ('admindev', 'gabriela');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Perfil",
                table: "Admins");
        }
    }
}
