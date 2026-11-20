using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RulesValidator.Infrastructure.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class CatalogoDinamicoCampos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "campo_regla",
                columns: table => new
                {
                    campo_regla_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    codigo = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: false),
                    tipo_valor = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    activo = table.Column<bool>(type: "bit", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    usuario_creacion = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    fecha_modificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    usuario_modificacion = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_campo_regla", x => x.campo_regla_id);
                    table.UniqueConstraint("UK_campo_regla_codigo", x => x.codigo);
                    table.CheckConstraint("CK_campo_regla_tipo_valor", "[tipo_valor] IN ('TEXTO', 'NUMERO', 'BOOLEANO')");
                });

            migrationBuilder.CreateTable(
                name: "campo_regla_operador",
                columns: table => new
                {
                    campo_regla_id = table.Column<long>(type: "bigint", nullable: false),
                    operador = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_campo_regla_operador", x => new { x.campo_regla_id, x.operador });
                    table.CheckConstraint("CK_campo_regla_operador_codigo", "[operador] IN ('IGUAL', 'DIFERENTE', 'MAYOR', 'MAYOR_IGUAL', 'MENOR', 'MENOR_IGUAL', 'EN', 'NO_EN')");
                    table.ForeignKey(
                        name: "FK_campo_regla_operador_campo_regla",
                        column: x => x.campo_regla_id,
                        principalTable: "campo_regla",
                        principalColumn: "campo_regla_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_campo_regla_activo_codigo",
                table: "campo_regla",
                columns: new[] { "activo", "codigo" });

            migrationBuilder.InsertData(
                table: "campo_regla",
                columns: new[] { "campo_regla_id", "codigo", "tipo_valor", "activo", "fecha_creacion", "usuario_creacion" },
                values: new object[,]
                {
                    { 1L, "pension.tipo", "TEXTO", true, new DateTime(2026, 11, 20, 0, 0, 0, DateTimeKind.Utc), "SISTEMA" },
                    { 2L, "pension.mesadaEnSmlmv", "NUMERO", true, new DateTime(2026, 11, 20, 0, 0, 0, DateTimeKind.Utc), "SISTEMA" },
                    { 3L, "plan.codigo", "TEXTO", true, new DateTime(2026, 11, 20, 0, 0, 0, DateTimeKind.Utc), "SISTEMA" }
                });

            migrationBuilder.InsertData(
                table: "campo_regla_operador",
                columns: new[] { "campo_regla_id", "operador" },
                values: new object[,]
                {
                    { 1L, "IGUAL" },
                    { 1L, "DIFERENTE" },
                    { 1L, "EN" },
                    { 1L, "NO_EN" },
                    { 2L, "IGUAL" },
                    { 2L, "DIFERENTE" },
                    { 2L, "MAYOR" },
                    { 2L, "MAYOR_IGUAL" },
                    { 2L, "MENOR" },
                    { 2L, "MENOR_IGUAL" },
                    { 2L, "EN" },
                    { 2L, "NO_EN" },
                    { 3L, "IGUAL" },
                    { 3L, "DIFERENTE" },
                    { 3L, "EN" },
                    { 3L, "NO_EN" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "campo_regla_operador");

            migrationBuilder.DropTable(
                name: "campo_regla");
        }
    }
}
