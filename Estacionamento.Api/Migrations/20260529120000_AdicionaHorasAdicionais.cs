using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Estacionamento.Api.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaHorasAdicionais : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Novos campos na tabela Precos
            migrationBuilder.AddColumn<decimal>(
                name: "ValorHorasAdicionaisAte6h",
                table: "Precos",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorHorasAdicionaisAte12h",
                table: "Precos",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            // Novo campo na tabela Reservas
            migrationBuilder.AddColumn<decimal>(
                name: "ValorHorasAdicionais",
                table: "Reservas",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ValorHorasAdicionaisAte6h",
                table: "Precos");

            migrationBuilder.DropColumn(
                name: "ValorHorasAdicionaisAte12h",
                table: "Precos");

            migrationBuilder.DropColumn(
                name: "ValorHorasAdicionais",
                table: "Reservas");
        }
    }
}
