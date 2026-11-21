using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RulesValidator.Infrastructure.Persistencia.Migrations
{
    [DbContext(typeof(RulesDbContext))]
    [Migration("20261121120000_EtapaEjecucionOpcional")]
    public partial class EtapaEjecucionOpcional : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "etapa",
                schema: "reglas",
                table: "ejecucion_regla",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM reglas.ejecucion_regla WHERE etapa IS NULL)
                    THROW 51030, 'No se puede volver obligatoria etapa mientras existan ejecuciones sin etapa.', 1;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "etapa",
                schema: "reglas",
                table: "ejecucion_regla",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}