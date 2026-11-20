using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RulesValidator.Infrastructure.Persistencia.Migrations
{
    [DbContext(typeof(RulesDbContext))]
    [Migration("20261120193000_AgregarEsquemaReglas")]
    public partial class AgregarEsquemaReglas : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "reglas");

            migrationBuilder.RenameTable(name: "campo_regla", schema: null, newName: "campo_regla", newSchema: "reglas");
            migrationBuilder.RenameTable(name: "campo_regla_operador", schema: null, newName: "campo_regla_operador", newSchema: "reglas");
            migrationBuilder.RenameTable(name: "negocio", schema: null, newName: "negocio", newSchema: "reglas");
            migrationBuilder.RenameTable(name: "flujo_reglas", schema: null, newName: "flujo_reglas", newSchema: "reglas");
            migrationBuilder.RenameTable(name: "regla", schema: null, newName: "regla", newSchema: "reglas");
            migrationBuilder.RenameTable(name: "version_regla", schema: null, newName: "version_regla", newSchema: "reglas");
            migrationBuilder.RenameTable(name: "ejecucion_regla", schema: null, newName: "ejecucion_regla", newSchema: "reglas");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(name: "ejecucion_regla", schema: "reglas", newName: "ejecucion_regla", newSchema: null);
            migrationBuilder.RenameTable(name: "version_regla", schema: "reglas", newName: "version_regla", newSchema: null);
            migrationBuilder.RenameTable(name: "regla", schema: "reglas", newName: "regla", newSchema: null);
            migrationBuilder.RenameTable(name: "flujo_reglas", schema: "reglas", newName: "flujo_reglas", newSchema: null);
            migrationBuilder.RenameTable(name: "negocio", schema: "reglas", newName: "negocio", newSchema: null);
            migrationBuilder.RenameTable(name: "campo_regla_operador", schema: "reglas", newName: "campo_regla_operador", newSchema: null);
            migrationBuilder.RenameTable(name: "campo_regla", schema: "reglas", newName: "campo_regla", newSchema: null);
        }
    }
}