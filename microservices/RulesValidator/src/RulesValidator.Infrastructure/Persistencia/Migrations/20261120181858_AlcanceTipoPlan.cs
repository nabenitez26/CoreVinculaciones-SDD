using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RulesValidator.Infrastructure.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class AlcanceTipoPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_flujo_reglas_negocio_proceso_etapa_activo_prioridad",
                table: "flujo_reglas");

            migrationBuilder.DropCheckConstraint(
                name: "CK_flujo_reglas_etapa",
                table: "flujo_reglas");

            migrationBuilder.AlterColumn<int>(
                name: "etapa",
                table: "flujo_reglas",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "tipo_plan_id",
                table: "flujo_reglas",
                type: "varchar(10)",
                unicode: false,
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tipo_plan_id",
                table: "ejecucion_regla",
                type: "varchar(10)",
                unicode: false,
                maxLength: 10,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_flujo_reglas_negocio_proceso_tipo_etapa_activo_prioridad",
                table: "flujo_reglas",
                columns: new[] { "negocio_id", "proceso", "tipo_plan_id", "etapa", "activo", "prioridad" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_flujo_reglas_etapa",
                table: "flujo_reglas",
                sql: "[etapa] IS NULL OR [etapa] > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_flujo_reglas_negocio_proceso_tipo_etapa_activo_prioridad",
                table: "flujo_reglas");

            migrationBuilder.DropCheckConstraint(
                name: "CK_flujo_reglas_etapa",
                table: "flujo_reglas");

            migrationBuilder.DropColumn(
                name: "tipo_plan_id",
                table: "flujo_reglas");

            migrationBuilder.DropColumn(
                name: "tipo_plan_id",
                table: "ejecucion_regla");

            migrationBuilder.AlterColumn<int>(
                name: "etapa",
                table: "flujo_reglas",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_flujo_reglas_negocio_proceso_etapa_activo_prioridad",
                table: "flujo_reglas",
                columns: new[] { "negocio_id", "proceso", "etapa", "activo", "prioridad" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_flujo_reglas_etapa",
                table: "flujo_reglas",
                sql: "[etapa] > 0");
        }
    }
}
