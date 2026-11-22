using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RulesValidator.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "reglas");

            migrationBuilder.CreateTable(
                name: "campo_regla",
                schema: "reglas",
                columns: table => new
                {
                    campo_regla_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    negocio_id = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    proceso = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    tipo_plan_id = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    codigo = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: false),
                    tipo_valor = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    dato_requerido_id = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    activo = table.Column<bool>(type: "bit", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    usuario_creacion = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    fecha_modificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    usuario_modificacion = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_campo_regla", x => x.campo_regla_id);
                    table.CheckConstraint("CK_campo_regla_tipo_valor", "[tipo_valor] IN ('TEXTO', 'NUMERO', 'BOOLEANO')");
                    table.ForeignKey(
                        name: "FK_campo_regla_dato_requerido",
                        column: x => x.dato_requerido_id,
                        principalSchema: "cfg",
                        principalTable: "dato_requerido",
                        principalColumn: "dato_requerido_id");
                    table.ForeignKey(
                        name: "FK_campo_regla_negocio",
                        column: x => x.negocio_id,
                        principalSchema: "ref",
                        principalTable: "negocio",
                        principalColumn: "negocio_id");
                    table.ForeignKey(
                        name: "FK_campo_regla_tipo_plan",
                        column: x => x.tipo_plan_id,
                        principalSchema: "mae",
                        principalTable: "tipo_plan",
                        principalColumn: "tipo_plan_id");
                });

            migrationBuilder.CreateTable(
                name: "ejecucion_regla",
                schema: "reglas",
                columns: table => new
                {
                    ejecucion_regla_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_evaluacion = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    negocio_id = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    proceso = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    tipo_plan_id = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    etapa = table.Column<int>(type: "int", nullable: true),
                    flujos_json = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    resultado_general = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    resultados_json = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fecha_evaluacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fecha_ejecucion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    duracion_ms = table.Column<long>(type: "bigint", nullable: false),
                    id_correlacion = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    id_traza = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ejecucion_regla", x => x.ejecucion_regla_id);
                    table.UniqueConstraint("UK_ejecucion_regla_id_evaluacion", x => x.id_evaluacion);
                    table.CheckConstraint("CK_ejecucion_regla_duracion_ms", "[duracion_ms] >= 0");
                    table.CheckConstraint("CK_ejecucion_regla_flujos_json", "ISJSON([flujos_json]) = 1");
                    table.CheckConstraint("CK_ejecucion_regla_resultado_general", "[resultado_general] IN ('CUMPLE', 'NO_CUMPLE', 'INDETERMINADO')");
                    table.CheckConstraint("CK_ejecucion_regla_resultados_json", "ISJSON([resultados_json]) = 1");
                    table.ForeignKey(
                        name: "FK_ejecucion_regla_negocio",
                        column: x => x.negocio_id,
                        principalSchema: "ref",
                        principalTable: "negocio",
                        principalColumn: "negocio_id");
                    table.ForeignKey(
                        name: "FK_ejecucion_regla_tipo_plan",
                        column: x => x.tipo_plan_id,
                        principalSchema: "mae",
                        principalTable: "tipo_plan",
                        principalColumn: "tipo_plan_id");
                });

            migrationBuilder.CreateTable(
                name: "flujo_reglas",
                schema: "reglas",
                columns: table => new
                {
                    flujo_reglas_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    negocio_id = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    codigo = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    nombre = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    proceso = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    etapa = table.Column<int>(type: "int", nullable: true),
                    tipo_plan_id = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    prioridad = table.Column<int>(type: "int", nullable: false),
                    activo = table.Column<bool>(type: "bit", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    usuario_creacion = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    fecha_modificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    usuario_modificacion = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flujo_reglas", x => x.flujo_reglas_id);
                    table.UniqueConstraint("UK_flujo_reglas_codigo", x => x.codigo);
                    table.CheckConstraint("CK_flujo_reglas_etapa", "[etapa] IS NULL OR [etapa] > 0");
                    table.CheckConstraint("CK_flujo_reglas_prioridad", "[prioridad] >= 0");
                    table.ForeignKey(
                        name: "FK_flujo_reglas_negocio",
                        column: x => x.negocio_id,
                        principalSchema: "ref",
                        principalTable: "negocio",
                        principalColumn: "negocio_id");
                    table.ForeignKey(
                        name: "FK_flujo_reglas_tipo_plan",
                        column: x => x.tipo_plan_id,
                        principalSchema: "mae",
                        principalTable: "tipo_plan",
                        principalColumn: "tipo_plan_id");
                });

            migrationBuilder.CreateTable(
                name: "campo_regla_operador",
                schema: "reglas",
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
                        principalSchema: "reglas",
                        principalTable: "campo_regla",
                        principalColumn: "campo_regla_id");
                });

            migrationBuilder.CreateTable(
                name: "regla",
                schema: "reglas",
                columns: table => new
                {
                    regla_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    flujo_reglas_id = table.Column<long>(type: "bigint", nullable: false),
                    codigo = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    nombre = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    descripcion = table.Column<string>(type: "varchar(1000)", unicode: false, maxLength: 1000, nullable: true),
                    tipo_regla = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    prioridad = table.Column<int>(type: "int", nullable: false),
                    activo = table.Column<bool>(type: "bit", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    usuario_creacion = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_regla", x => x.regla_id);
                    table.UniqueConstraint("UK_regla_flujo_codigo", x => new { x.flujo_reglas_id, x.codigo });
                    table.CheckConstraint("CK_regla_prioridad", "[prioridad] >= 0");
                    table.CheckConstraint("CK_regla_tipo_regla", "[tipo_regla] IN ('PARAMETRIZABLE', 'ESPECIALIZADA')");
                    table.ForeignKey(
                        name: "FK_regla_flujo_reglas",
                        column: x => x.flujo_reglas_id,
                        principalSchema: "reglas",
                        principalTable: "flujo_reglas",
                        principalColumn: "flujo_reglas_id");
                });

            migrationBuilder.CreateTable(
                name: "version_regla",
                schema: "reglas",
                columns: table => new
                {
                    version_regla_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    regla_id = table.Column<long>(type: "bigint", nullable: false),
                    version = table.Column<int>(type: "int", nullable: false),
                    version_esquema = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    estado = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    severidad = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    definicion_json = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    codigo_error = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    mensaje_error = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    fecha_inicio_vigencia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fecha_fin_vigencia = table.Column<DateTime>(type: "datetime2", nullable: true),
                    fecha_creacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    usuario_creacion = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    fecha_aprobacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    usuario_aprobacion = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_version_regla", x => x.version_regla_id);
                    table.UniqueConstraint("UK_version_regla_regla_version", x => new { x.regla_id, x.version });
                    table.CheckConstraint("CK_version_regla_definicion_json", "ISJSON([definicion_json]) = 1");
                    table.CheckConstraint("CK_version_regla_estado", "[estado] IN ('BORRADOR', 'EN_REVISION', 'APROBADA', 'ACTIVA', 'INACTIVA')");
                    table.CheckConstraint("CK_version_regla_severidad", "[severidad] IN ('INFORMATIVA', 'ADVERTENCIA', 'BLOQUEANTE')");
                    table.CheckConstraint("CK_version_regla_vigencia", "[fecha_fin_vigencia] IS NULL OR [fecha_fin_vigencia] >= [fecha_inicio_vigencia]");
                    table.ForeignKey(
                        name: "FK_version_regla_regla",
                        column: x => x.regla_id,
                        principalSchema: "reglas",
                        principalTable: "regla",
                        principalColumn: "regla_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_campo_regla_dato_requerido_id",
                schema: "reglas",
                table: "campo_regla",
                column: "dato_requerido_id");

            migrationBuilder.CreateIndex(
                name: "IX_campo_regla_scope_activo",
                schema: "reglas",
                table: "campo_regla",
                columns: new[] { "activo", "negocio_id", "proceso", "tipo_plan_id" });

            migrationBuilder.CreateIndex(
                name: "IX_campo_regla_tipo_plan_id",
                schema: "reglas",
                table: "campo_regla",
                column: "tipo_plan_id");

            migrationBuilder.CreateIndex(
                name: "UX_campo_regla_scope_codigo",
                schema: "reglas",
                table: "campo_regla",
                columns: new[] { "negocio_id", "proceso", "tipo_plan_id", "codigo" },
                unique: true,
                filter: "[negocio_id] IS NOT NULL AND [proceso] IS NOT NULL AND [tipo_plan_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ejecucion_regla_negocio_id",
                schema: "reglas",
                table: "ejecucion_regla",
                column: "negocio_id");

            migrationBuilder.CreateIndex(
                name: "IX_ejecucion_regla_tipo_plan_id",
                schema: "reglas",
                table: "ejecucion_regla",
                column: "tipo_plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_flujo_reglas_negocio_proceso_tipo_etapa_activo_prioridad",
                schema: "reglas",
                table: "flujo_reglas",
                columns: new[] { "negocio_id", "proceso", "tipo_plan_id", "etapa", "activo", "prioridad" });

            migrationBuilder.CreateIndex(
                name: "IX_flujo_reglas_tipo_plan_id",
                schema: "reglas",
                table: "flujo_reglas",
                column: "tipo_plan_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "campo_regla_operador",
                schema: "reglas");

            migrationBuilder.DropTable(
                name: "ejecucion_regla",
                schema: "reglas");

            migrationBuilder.DropTable(
                name: "version_regla",
                schema: "reglas");

            migrationBuilder.DropTable(
                name: "campo_regla",
                schema: "reglas");

            migrationBuilder.DropTable(
                name: "regla",
                schema: "reglas");

            migrationBuilder.DropTable(
                name: "flujo_reglas",
                schema: "reglas");
        }
    }
}
