using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Estacionamento.Api.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaLogAtividade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LogsAtividade",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DataHora = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    AdminId = table.Column<int>(type: "integer", nullable: true),
                    AdminUsuario = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Acao = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Entidade = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EntidadeId = table.Column<int>(type: "integer", nullable: true),
                    Detalhes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Sucesso = table.Column<bool>(type: "boolean", nullable: false),
                    Origem = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogsAtividade", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LogsAtividade_Acao",
                table: "LogsAtividade",
                column: "Acao");

            migrationBuilder.CreateIndex(
                name: "IX_LogsAtividade_DataHora",
                table: "LogsAtividade",
                column: "DataHora");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LogsAtividade");
        }
    }
}
