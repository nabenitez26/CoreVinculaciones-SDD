using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RulesValidator.Infrastructure.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class ScopeCatalogoCampos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "UK_campo_regla_codigo",
                table: "campo_regla");

            migrationBuilder.DropIndex(
                name: "IX_campo_regla_activo_codigo",
                table: "campo_regla");

            migrationBuilder.AlterColumn<int>(
                name: "etapa",
                table: "ejecucion_regla",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "negocio_codigo",
                table: "campo_regla",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "proceso",
                table: "campo_regla",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tipo_plan_id",
                table: "campo_regla",
                type: "varchar(10)",
                unicode: false,
                maxLength: 10,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_campo_regla_scope_activo",
                table: "campo_regla",
                columns: new[] { "activo", "negocio_codigo", "proceso", "tipo_plan_id" });

            migrationBuilder.CreateIndex(
                name: "UX_campo_regla_scope_codigo",
                table: "campo_regla",
                columns: new[] { "negocio_codigo", "proceso", "tipo_plan_id", "codigo" },
                unique: true,
                filter: "[proceso] IS NOT NULL AND [tipo_plan_id] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_campo_regla_scope_activo",
                table: "campo_regla");

            migrationBuilder.DropIndex(
                name: "UX_campo_regla_scope_codigo",
                table: "campo_regla");

            migrationBuilder.DropColumn(
                name: "negocio_codigo",
                table: "campo_regla");

            migrationBuilder.DropColumn(
                name: "proceso",
                table: "campo_regla");

            migrationBuilder.DropColumn(
                name: "tipo_plan_id",
                table: "campo_regla");

            migrationBuilder.AlterColumn<int>(
                name: "etapa",
                table: "ejecucion_regla",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "UK_campo_regla_codigo",
                table: "campo_regla",
                column: "codigo");

            migrationBuilder.CreateIndex(
                name: "IX_campo_regla_activo_codigo",
                table: "campo_regla",
                columns: new[] { "activo", "codigo" });
        }
    }
}
