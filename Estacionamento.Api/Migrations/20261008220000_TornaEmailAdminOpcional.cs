using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Estacionamento.Api.Infrastructure.Data;

#nullable disable

namespace Estacionamento.Api.Migrations
{
    /// <summary>
    /// Torna Admins.Email opcional. O índice único continua valendo: no PostgreSQL
    /// vários NULL não conflitam entre si, só e-mails preenchidos precisam ser distintos.
    /// </summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20261008220000_TornaEmailAdminOpcional")]
    public partial class TornaEmailAdminOpcional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""ALTER TABLE "Admins" ALTER COLUMN "Email" DROP NOT NULL;""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Preenche quem ficou sem e-mail para o NOT NULL poder voltar (valor único por usuário).
            migrationBuilder.Sql("""UPDATE "Admins" SET "Email" = "Usuario" || '@sem-email.invalid' WHERE "Email" IS NULL;""");
            migrationBuilder.Sql("""ALTER TABLE "Admins" ALTER COLUMN "Email" SET NOT NULL;""");
        }
    }
}
