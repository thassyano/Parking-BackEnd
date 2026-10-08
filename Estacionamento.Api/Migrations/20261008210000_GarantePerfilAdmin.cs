using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Estacionamento.Api.Infrastructure.Data;

#nullable disable

namespace Estacionamento.Api.Migrations
{
    /// <summary>
    /// Garante a coluna Admins.Perfil (Admin | AdminMaster).
    /// Idempotente: se a coluna já existir (ex.: migration equivalente aplicada
    /// manualmente ou pela branch Thassyano/AjustesGerais), nada é alterado,
    /// inclusive os perfis já definidos.
    /// </summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20261008210000_GarantePerfilAdmin")]
    public partial class GarantePerfilAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1
                        FROM information_schema.columns
                        WHERE table_schema = current_schema()
                          AND table_name = 'Admins'
                          AND column_name = 'Perfil')
                    THEN
                        ALTER TABLE "Admins"
                            ADD COLUMN "Perfil" character varying(20) NOT NULL DEFAULT 'Admin';

                        UPDATE "Admins"
                        SET "Perfil" = 'AdminMaster'
                        WHERE LOWER("Usuario") IN ('admindev', 'gabriela');
                    END IF;
                END
                $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""ALTER TABLE "Admins" DROP COLUMN IF EXISTS "Perfil";""");
        }
    }
}
