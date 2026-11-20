using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RulesValidator.Infrastructure.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class NormalizarCatalogoScopes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM ejecucion_regla WHERE etapa IS NULL)
                    THROW 51001, 'No se puede hacer obligatoria ejecucion_regla.etapa mientras existan ejecuciones sin etapa.', 1;

                UPDATE campo_regla
                SET negocio_codigo = 'GLOBAL'
                WHERE negocio_codigo = '';

                UPDATE campo_regla
                SET negocio_codigo = 'PENSIONADOS', proceso = 'VINCULACION', tipo_plan_id = NULL
                WHERE campo_regla_id IN (1, 2, 3);

                IF EXISTS (
                    SELECT negocio_codigo, proceso, tipo_plan_id, codigo
                    FROM campo_regla
                    GROUP BY negocio_codigo, proceso, tipo_plan_id, codigo
                    HAVING COUNT(*) > 1
                )
                    THROW 51002, 'El catálogo contiene campos duplicados dentro del mismo scope.', 1;
                """);

            migrationBuilder.DropIndex(
                name: "UX_campo_regla_scope_codigo",
                table: "campo_regla");

            migrationBuilder.AlterColumn<int>(
                name: "etapa",
                table: "ejecucion_regla",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "UX_campo_regla_scope_codigo",
                table: "campo_regla",
                columns: new[] { "negocio_codigo", "proceso", "tipo_plan_id", "codigo" },
                unique: true,
                filter: "[negocio_codigo] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_campo_regla_scope_codigo",
                table: "campo_regla");

            migrationBuilder.AlterColumn<int>(
                name: "etapa",
                table: "ejecucion_regla",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
            migrationBuilder.CreateIndex(
                name: "UX_campo_regla_scope_codigo",
                table: "campo_regla",
                columns: new[] { "negocio_codigo", "proceso", "tipo_plan_id", "codigo" },
                unique: true,
                filter: "[proceso] IS NOT NULL AND [tipo_plan_id] IS NOT NULL");
        }
    }
}
