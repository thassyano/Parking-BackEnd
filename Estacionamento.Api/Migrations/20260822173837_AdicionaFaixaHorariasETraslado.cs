using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Estacionamento.Api.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaFaixaHorariasETraslado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotente: algumas colunas de faixa ja existiam no banco (criadas fora do
            // controle de migrations). IF NOT EXISTS cria apenas o que falta e ignora o resto.
            migrationBuilder.Sql(@"ALTER TABLE ""Reservas"" ADD COLUMN IF NOT EXISTS ""ComTraslado"" boolean NOT NULL DEFAULT FALSE;");
            migrationBuilder.Sql(@"ALTER TABLE ""Reservas"" ADD COLUMN IF NOT EXISTS ""ValorTraslado"" numeric(18,2) NOT NULL DEFAULT 0;");
            migrationBuilder.Sql(@"ALTER TABLE ""Precos"" ADD COLUMN IF NOT EXISTS ""ValorHorasAdicionaisAte12h"" numeric(18,2) NOT NULL DEFAULT 0;");
            migrationBuilder.Sql(@"ALTER TABLE ""Precos"" ADD COLUMN IF NOT EXISTS ""ValorHorasAdicionaisAte6h"" numeric(18,2) NOT NULL DEFAULT 0;");
            migrationBuilder.Sql(@"ALTER TABLE ""Configuracoes"" ADD COLUMN IF NOT EXISTS ""TrasladoGratisAPartirDiarias"" integer NOT NULL DEFAULT 0;");
            migrationBuilder.Sql(@"ALTER TABLE ""Configuracoes"" ADD COLUMN IF NOT EXISTS ""ValorTraslado"" numeric(18,2) NOT NULL DEFAULT 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"ALTER TABLE ""Reservas"" DROP COLUMN IF EXISTS ""ComTraslado"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Reservas"" DROP COLUMN IF EXISTS ""ValorTraslado"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Precos"" DROP COLUMN IF EXISTS ""ValorHorasAdicionaisAte12h"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Precos"" DROP COLUMN IF EXISTS ""ValorHorasAdicionaisAte6h"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Configuracoes"" DROP COLUMN IF EXISTS ""TrasladoGratisAPartirDiarias"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Configuracoes"" DROP COLUMN IF EXISTS ""ValorTraslado"";");
        }
    }
}
