using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RulesValidator.Infrastructure.Persistencia.Migrations
{
    [DbContext(typeof(RulesDbContext))]
    [Migration("20261121130000_AlinearAliasesContexto")]
    public partial class AlinearAliasesContexto : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1
                    FROM reglas.version_regla
                    WHERE estado = 'ACTIVA'
                      AND definicion_json LIKE '%plan.codigo%'
                )
                    THROW 51040, 'Hay definiciones activas que referencian plan.codigo; deben migrarse a tipoPlanId antes de retirar el alias.', 1;

                UPDATE reglas.campo_regla SET activo = 0 WHERE codigo = 'plan.codigo';
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE reglas.campo_regla SET activo = 1 WHERE codigo = 'plan.codigo';
                """);
        }
    }
}