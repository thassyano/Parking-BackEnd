using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Estacionamento.Api.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Usuario = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SenhaHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Configuracoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NomeEstacionamento = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Endereco = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Contato = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Cnpj = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    TotalVagasCoberta = table.Column<int>(type: "integer", nullable: false),
                    TotalVagasDescoberta = table.Column<int>(type: "integer", nullable: false),
                    TelefoneWhatsApp = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    MensagemWhatsApp = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    HorasAntecedenciaConfirmacao = table.Column<int>(type: "integer", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configuracoes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Precos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TipoVaga = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ValorDiaria = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DescontoPixDinheiro = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DataInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataFim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Precos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reservas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NomeCliente = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TelefoneCliente = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CpfCliente = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    PlacaVeiculo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    CorVeiculo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    TipoVaga = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DataEntrada = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    QtdDias = table.Column<int>(type: "integer", nullable: false),
                    DataSaidaPrevista = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValorDiaria = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DescontoAplicado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorFinal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FormaPagamento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Pago = table.Column<bool>(type: "boolean", nullable: false),
                    DataPagamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Origem = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DataCheckin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataCheckout = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Observacoes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Admins_Email",
                table: "Admins",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Admins_Usuario",
                table: "Admins",
                column: "Usuario",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_DataEntrada",
                table: "Reservas",
                column: "DataEntrada");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_Status",
                table: "Reservas",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Admins");

            migrationBuilder.DropTable(
                name: "Configuracoes");

            migrationBuilder.DropTable(
                name: "Precos");

            migrationBuilder.DropTable(
                name: "Reservas");
        }
    }
}
