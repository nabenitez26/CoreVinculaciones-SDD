using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RulesValidator.Infrastructure.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class AgregarFuenteExternaMapeo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "fuente_externa_mapeo",
                schema: "reglas",
                columns: table => new
                {
                    fuente_dato_id = table.Column<string>(type: "nvarchar(20)", maxLength: 10, nullable: false),
                    nombre_blob = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    separador = table.Column<string>(type: "varchar(1)", unicode: false, maxLength: 1, nullable: false),
                    columna_clave = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    columna_valor = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    campo_clave_contexto = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: false),
                    campo_regla_id = table.Column<long>(type: "bigint", nullable: false),
                    activo = table.Column<bool>(type: "bit", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    usuario_creacion = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    fecha_modificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    usuario_modificacion = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fuente_externa_mapeo", x => x.fuente_dato_id);
                    table.CheckConstraint("CK_fuente_externa_mapeo_columnas", "[columna_clave] <> [columna_valor]");
                    table.CheckConstraint("CK_fuente_externa_mapeo_nombre_blob", "LOWER(RIGHT([nombre_blob], 4)) = '.csv'");
                    table.CheckConstraint("CK_fuente_externa_mapeo_separador", "LEN([separador]) = 1");
                    table.ForeignKey(
                        name: "FK_fuente_externa_mapeo_campo_regla",
                        column: x => x.campo_regla_id,
                        principalSchema: "reglas",
                        principalTable: "campo_regla",
                        principalColumn: "campo_regla_id");
                    table.ForeignKey(
                        name: "FK_fuente_externa_mapeo_fuente_dato",
                        column: x => x.fuente_dato_id,
                        principalSchema: "ref",
                        principalTable: "fuente_dato",
                        principalColumn: "fuente_dato_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_fuente_externa_mapeo_campo_regla_id",
                schema: "reglas",
                table: "fuente_externa_mapeo",
                column: "campo_regla_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fuente_externa_mapeo",
                schema: "reglas");
        }
    }
}
