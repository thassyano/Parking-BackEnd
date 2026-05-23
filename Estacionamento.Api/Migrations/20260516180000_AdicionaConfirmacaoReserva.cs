using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Estacionamento.Api.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaConfirmacaoReserva : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Campos de confirmação na tabela Reservas
            migrationBuilder.AddColumn<Guid>(
                name: "ConfirmacaoToken",
                table: "Reservas",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddColumn<bool>(
                name: "ConfirmadaPeloCliente",
                table: "Reservas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "MensagemConfirmacaoEnviada",
                table: "Reservas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataEnvioConfirmacao",
                table: "Reservas",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_ConfirmacaoToken",
                table: "Reservas",
                column: "ConfirmacaoToken",
                unique: true);

            // Campos da Evolution API na tabela Configuracoes
            migrationBuilder.AddColumn<string>(
                name: "EvolutionApiUrl",
                table: "Configuracoes",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EvolutionApiKey",
                table: "Configuracoes",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EvolutionInstanceName",
                table: "Configuracoes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UrlConfirmacaoFrontend",
                table: "Configuracoes",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reservas_ConfirmacaoToken",
                table: "Reservas");

            migrationBuilder.DropColumn(name: "ConfirmacaoToken", table: "Reservas");
            migrationBuilder.DropColumn(name: "ConfirmadaPeloCliente", table: "Reservas");
            migrationBuilder.DropColumn(name: "MensagemConfirmacaoEnviada", table: "Reservas");
            migrationBuilder.DropColumn(name: "DataEnvioConfirmacao", table: "Reservas");

            migrationBuilder.DropColumn(name: "EvolutionApiUrl", table: "Configuracoes");
            migrationBuilder.DropColumn(name: "EvolutionApiKey", table: "Configuracoes");
            migrationBuilder.DropColumn(name: "EvolutionInstanceName", table: "Configuracoes");
            migrationBuilder.DropColumn(name: "UrlConfirmacaoFrontend", table: "Configuracoes");
        }
    }
}
