using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RulesValidator.Infrastructure.Persistencia.Migrations
{
    [DbContext(typeof(RulesDbContext))]
    [Migration("20261121090000_AlinearModeloCore")]
    public partial class AlinearModeloCore : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'ref.negocio', N'U') IS NULL
                    THROW 51010, 'No existe ref.negocio; no se puede eliminar reglas.negocio.', 1;
                IF OBJECT_ID(N'mae.tipo_plan', N'U') IS NULL
                    THROW 51011, 'No existe mae.tipo_plan; no se puede enlazar el flujo de reglas.', 1;
                IF OBJECT_ID(N'mae.tipo_contrato', N'U') IS NULL
                    THROW 51018, 'No existe mae.tipo_contrato; no se puede validar la pertenencia del plan.', 1;
                IF OBJECT_ID(N'mae.tipo_vinculacion', N'U') IS NULL
                    THROW 51019, 'No existe mae.tipo_vinculacion; no se puede validar la pertenencia del plan.', 1;
                IF OBJECT_ID(N'cfg.dato_requerido', N'U') IS NULL
                    THROW 51012, 'No existe cfg.dato_requerido; no se puede crear el mapping de campos.', 1;
                IF OBJECT_ID(N'ref.tipo_dato', N'U') IS NULL
                    THROW 51013, 'No existe ref.tipo_dato; no se puede derivar el tipo de campo.', 1;

                IF EXISTS (
                    SELECT 1
                    FROM reglas.flujo_reglas AS f
                    INNER JOIN reglas.negocio AS n ON n.negocio_id = f.negocio_id
                    WHERE n.codigo <> 'GLOBAL'
                      AND NOT EXISTS (SELECT 1 FROM ref.negocio AS r WHERE r.negocio_id = n.codigo)
                )
                    THROW 51014, 'Hay flujos cuyo reglas.negocio.codigo no coincide con ref.negocio.negocio_id.', 1;

                IF EXISTS (
                    SELECT 1
                    FROM reglas.ejecucion_regla AS e
                    INNER JOIN reglas.negocio AS n ON n.negocio_id = e.negocio_id
                    WHERE n.codigo <> 'GLOBAL'
                      AND NOT EXISTS (SELECT 1 FROM ref.negocio AS r WHERE r.negocio_id = n.codigo)
                )
                    THROW 51015, 'Hay ejecuciones cuyo reglas.negocio.codigo no coincide con ref.negocio.negocio_id.', 1;

                IF EXISTS (
                    SELECT 1
                    FROM reglas.campo_regla AS c
                    WHERE c.negocio_codigo <> 'GLOBAL'
                      AND NOT EXISTS (SELECT 1 FROM ref.negocio AS r WHERE r.negocio_id = c.negocio_codigo)
                )
                    THROW 51016, 'Hay campos cuyo negocio_codigo no coincide con ref.negocio.negocio_id.', 1;

                IF EXISTS (
                    SELECT 1 FROM reglas.flujo_reglas AS f
                    WHERE f.tipo_plan_id IS NOT NULL
                      AND NOT EXISTS (SELECT 1 FROM mae.tipo_plan AS p WHERE p.tipo_plan_id = f.tipo_plan_id)
                ) OR EXISTS (
                    SELECT 1 FROM reglas.ejecucion_regla AS e
                    WHERE e.tipo_plan_id IS NOT NULL
                      AND NOT EXISTS (SELECT 1 FROM mae.tipo_plan AS p WHERE p.tipo_plan_id = e.tipo_plan_id)
                ) OR EXISTS (
                    SELECT 1 FROM reglas.campo_regla AS c
                    WHERE c.tipo_plan_id IS NOT NULL
                      AND NOT EXISTS (SELECT 1 FROM mae.tipo_plan AS p WHERE p.tipo_plan_id = c.tipo_plan_id)
                )
                    THROW 51017, 'Hay tipo_plan_id sin correspondencia en mae.tipo_plan.', 1;

                IF EXISTS (
                    SELECT 1
                    FROM reglas.flujo_reglas AS f
                    INNER JOIN reglas.negocio AS n ON n.negocio_id = f.negocio_id
                    INNER JOIN mae.tipo_plan AS p ON p.tipo_plan_id = f.tipo_plan_id
                    INNER JOIN mae.tipo_contrato AS c ON c.tipo_contrato_id = p.tipo_contrato_id
                    INNER JOIN mae.tipo_vinculacion AS v ON v.tipo_vinculacion_id = c.tipo_vinculacion_id
                    WHERE f.tipo_plan_id IS NOT NULL
                      AND (n.codigo = 'GLOBAL' OR v.negocio_id <> n.codigo)
                )
                    THROW 51020, 'Hay flujos cuyo tipo_plan_id no pertenece al negocio configurado.', 1;

                DROP INDEX IX_flujo_reglas_negocio_proceso_tipo_etapa_activo_prioridad ON reglas.flujo_reglas;
                DROP INDEX IX_ejecucion_regla_negocio_id ON reglas.ejecucion_regla;
                DROP INDEX UX_campo_regla_scope_codigo ON reglas.campo_regla;
                DROP INDEX IX_campo_regla_scope_activo ON reglas.campo_regla;

                ALTER TABLE reglas.flujo_reglas DROP CONSTRAINT FK_flujo_reglas_negocio;
                ALTER TABLE reglas.ejecucion_regla DROP CONSTRAINT FK_ejecucion_regla_negocio;

                ALTER TABLE reglas.flujo_reglas ADD negocio_id_core nvarchar(20) NULL;
                UPDATE f
                SET negocio_id_core = CASE WHEN n.codigo = 'GLOBAL' THEN NULL ELSE r.negocio_id END
                FROM reglas.flujo_reglas AS f
                INNER JOIN reglas.negocio AS n ON n.negocio_id = f.negocio_id
                LEFT JOIN ref.negocio AS r ON r.negocio_id = n.codigo;

                ALTER TABLE reglas.ejecucion_regla ADD negocio_id_core nvarchar(20) NULL;
                UPDATE e
                SET negocio_id_core = CASE WHEN n.codigo = 'GLOBAL' THEN NULL ELSE r.negocio_id END
                FROM reglas.ejecucion_regla AS e
                INNER JOIN reglas.negocio AS n ON n.negocio_id = e.negocio_id
                LEFT JOIN ref.negocio AS r ON r.negocio_id = n.codigo;

                ALTER TABLE reglas.campo_regla ADD negocio_id_core nvarchar(20) NULL;
                UPDATE c
                SET negocio_id_core = CASE WHEN c.negocio_codigo = 'GLOBAL' THEN NULL ELSE r.negocio_id END
                FROM reglas.campo_regla AS c
                LEFT JOIN ref.negocio AS r ON r.negocio_id = c.negocio_codigo;

                ALTER TABLE reglas.flujo_reglas DROP COLUMN negocio_id;
                EXEC sys.sp_rename N'reglas.flujo_reglas.negocio_id_core', N'negocio_id', N'COLUMN';
                ALTER TABLE reglas.ejecucion_regla DROP COLUMN negocio_id;
                EXEC sys.sp_rename N'reglas.ejecucion_regla.negocio_id_core', N'negocio_id', N'COLUMN';
                ALTER TABLE reglas.campo_regla DROP COLUMN negocio_codigo;
                EXEC sys.sp_rename N'reglas.campo_regla.negocio_id_core', N'negocio_id', N'COLUMN';

                ALTER TABLE reglas.flujo_reglas ALTER COLUMN tipo_plan_id nvarchar(10) NULL;
                ALTER TABLE reglas.ejecucion_regla ALTER COLUMN tipo_plan_id nvarchar(10) NULL;
                ALTER TABLE reglas.campo_regla ALTER COLUMN tipo_plan_id nvarchar(10) NULL;
                ALTER TABLE reglas.campo_regla ADD dato_requerido_id nvarchar(10) NULL;

                ALTER TABLE reglas.flujo_reglas WITH CHECK ADD CONSTRAINT CK_flujo_reglas_tipo_plan_negocio
                    CHECK (tipo_plan_id IS NULL OR negocio_id IS NOT NULL);

                DROP TABLE reglas.negocio;

                ALTER TABLE reglas.campo_regla WITH CHECK ADD CONSTRAINT FK_campo_negocio
                    FOREIGN KEY (negocio_id) REFERENCES ref.negocio (negocio_id);
                ALTER TABLE reglas.campo_regla WITH CHECK ADD CONSTRAINT FK_campo_tipo_plan
                    FOREIGN KEY (tipo_plan_id) REFERENCES mae.tipo_plan (tipo_plan_id);
                ALTER TABLE reglas.campo_regla WITH CHECK ADD CONSTRAINT FK_campo_dato_requerido
                    FOREIGN KEY (dato_requerido_id) REFERENCES cfg.dato_requerido (dato_requerido_id);
                ALTER TABLE reglas.flujo_reglas WITH CHECK ADD CONSTRAINT FK_flujo_negocio
                    FOREIGN KEY (negocio_id) REFERENCES ref.negocio (negocio_id);
                ALTER TABLE reglas.flujo_reglas WITH CHECK ADD CONSTRAINT FK_flujo_tipo_plan
                    FOREIGN KEY (tipo_plan_id) REFERENCES mae.tipo_plan (tipo_plan_id);
                ALTER TABLE reglas.ejecucion_regla WITH CHECK ADD CONSTRAINT FK_ejecucion_negocio
                    FOREIGN KEY (negocio_id) REFERENCES ref.negocio (negocio_id);
                ALTER TABLE reglas.ejecucion_regla WITH CHECK ADD CONSTRAINT FK_ejecucion_tipo_plan
                    FOREIGN KEY (tipo_plan_id) REFERENCES mae.tipo_plan (tipo_plan_id);

                CREATE INDEX IX_flujo_reglas_negocio_proceso_tipo_etapa_activo_prioridad
                    ON reglas.flujo_reglas (negocio_id, proceso, tipo_plan_id, etapa, activo, prioridad);
                CREATE INDEX IX_ejecucion_regla_negocio_id ON reglas.ejecucion_regla (negocio_id);
                CREATE UNIQUE INDEX UX_campo_regla_scope_codigo
                    ON reglas.campo_regla (negocio_id, proceso, tipo_plan_id, codigo);
                CREATE INDEX IX_campo_regla_scope_activo
                    ON reglas.campo_regla (activo, negocio_id, proceso, tipo_plan_id);
                """);

            migrationBuilder.Sql("""
                CREATE TRIGGER reglas.TR_flujo_reglas_validar_negocio_plan
                ON reglas.flujo_reglas
                AFTER INSERT, UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;

                    IF EXISTS (
                        SELECT 1
                        FROM inserted AS i
                        LEFT JOIN mae.tipo_plan AS p ON p.tipo_plan_id = i.tipo_plan_id
                        LEFT JOIN mae.tipo_contrato AS c ON c.tipo_contrato_id = p.tipo_contrato_id
                        LEFT JOIN mae.tipo_vinculacion AS v ON v.tipo_vinculacion_id = c.tipo_vinculacion_id
                        WHERE i.tipo_plan_id IS NOT NULL
                          AND (i.negocio_id IS NULL OR v.negocio_id IS NULL OR v.negocio_id <> i.negocio_id)
                    )
                        THROW 51021, 'El tipo de plan del flujo no pertenece al negocio indicado.', 1;
                END;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER reglas.TR_flujo_reglas_validar_negocio_plan;

                ALTER TABLE reglas.ejecucion_regla DROP CONSTRAINT FK_ejecucion_tipo_plan;
                ALTER TABLE reglas.ejecucion_regla DROP CONSTRAINT FK_ejecucion_negocio;
                ALTER TABLE reglas.flujo_reglas DROP CONSTRAINT FK_flujo_tipo_plan;
                ALTER TABLE reglas.flujo_reglas DROP CONSTRAINT FK_flujo_negocio;
                ALTER TABLE reglas.campo_regla DROP CONSTRAINT FK_campo_dato_requerido;
                ALTER TABLE reglas.campo_regla DROP CONSTRAINT FK_campo_tipo_plan;
                ALTER TABLE reglas.campo_regla DROP CONSTRAINT FK_campo_negocio;

                DROP INDEX IX_flujo_reglas_negocio_proceso_tipo_etapa_activo_prioridad ON reglas.flujo_reglas;
                DROP INDEX IX_ejecucion_regla_negocio_id ON reglas.ejecucion_regla;
                DROP INDEX UX_campo_regla_scope_codigo ON reglas.campo_regla;
                DROP INDEX IX_campo_regla_scope_activo ON reglas.campo_regla;

                CREATE TABLE reglas.negocio (
                    negocio_id bigint IDENTITY(1,1) NOT NULL,
                    codigo varchar(50) NOT NULL,
                    nombre varchar(150) NOT NULL,
                    descripcion varchar(500) NULL,
                    activo bit NOT NULL,
                    fecha_creacion datetime2 NOT NULL,
                    usuario_creacion varchar(100) NOT NULL,
                    fecha_modificacion datetime2 NULL,
                    usuario_modificacion varchar(100) NULL,
                    CONSTRAINT PK_negocio PRIMARY KEY (negocio_id),
                    CONSTRAINT UK_negocio_codigo UNIQUE (codigo)
                );

                INSERT INTO reglas.negocio (codigo, nombre, descripcion, activo, fecha_creacion, usuario_creacion)
                SELECT 'GLOBAL', 'Global', NULL, 1, SYSUTCDATETIME(), 'MIGRACION'
                UNION
                SELECT r.negocio_id, r.nombre, NULL, 1, SYSUTCDATETIME(), 'MIGRACION'
                FROM ref.negocio AS r
                     WHERE EXISTS (SELECT 1 FROM reglas.flujo_reglas WHERE negocio_id = r.negocio_id)
                         OR EXISTS (SELECT 1 FROM reglas.ejecucion_regla WHERE negocio_id = r.negocio_id)
                         OR EXISTS (SELECT 1 FROM reglas.campo_regla WHERE negocio_id = r.negocio_id);

                     ALTER TABLE reglas.flujo_reglas ADD negocio_id_old bigint NULL;
                UPDATE f SET negocio_id_old = n.negocio_id
                     FROM reglas.flujo_reglas AS f
                INNER JOIN reglas.negocio AS n ON n.codigo = ISNULL(f.negocio_id, 'GLOBAL');
                     ALTER TABLE reglas.ejecucion_regla ADD negocio_id_old bigint NULL;
                UPDATE e SET negocio_id_old = n.negocio_id
                     FROM reglas.ejecucion_regla AS e
                INNER JOIN reglas.negocio AS n ON n.codigo = ISNULL(e.negocio_id, 'GLOBAL');
                     ALTER TABLE reglas.campo_regla ADD negocio_codigo_old varchar(50) NULL;
                UPDATE c SET negocio_codigo_old = ISNULL(c.negocio_id, 'GLOBAL')
                     FROM reglas.campo_regla AS c;

                     ALTER TABLE reglas.flujo_reglas DROP COLUMN negocio_id;
                     EXEC sys.sp_rename N'reglas.flujo_reglas.negocio_id_old', N'negocio_id', N'COLUMN';
                     ALTER TABLE reglas.ejecucion_regla DROP COLUMN negocio_id;
                     EXEC sys.sp_rename N'reglas.ejecucion_regla.negocio_id_old', N'negocio_id', N'COLUMN';
                     ALTER TABLE reglas.campo_regla DROP COLUMN negocio_id;
                     EXEC sys.sp_rename N'reglas.campo_regla.negocio_codigo_old', N'negocio_codigo', N'COLUMN';

                     ALTER TABLE reglas.flujo_reglas ALTER COLUMN negocio_id bigint NOT NULL;
                     ALTER TABLE reglas.ejecucion_regla ALTER COLUMN negocio_id bigint NOT NULL;
                     ALTER TABLE reglas.campo_regla ALTER COLUMN negocio_codigo varchar(50) NOT NULL;
                     ALTER TABLE reglas.flujo_reglas ALTER COLUMN tipo_plan_id varchar(10) NULL;
                     ALTER TABLE reglas.ejecucion_regla ALTER COLUMN tipo_plan_id varchar(10) NULL;
                     ALTER TABLE reglas.campo_regla ALTER COLUMN tipo_plan_id varchar(10) NULL;

                    ALTER TABLE reglas.flujo_reglas DROP CONSTRAINT CK_flujo_reglas_tipo_plan_negocio;

                     ALTER TABLE reglas.campo_regla DROP COLUMN dato_requerido_id;

                ALTER TABLE reglas.flujo_reglas WITH CHECK ADD CONSTRAINT FK_flujo_reglas_negocio
                    FOREIGN KEY (negocio_id) REFERENCES reglas.negocio (negocio_id);
                ALTER TABLE reglas.ejecucion_regla WITH CHECK ADD CONSTRAINT FK_ejecucion_regla_negocio
                    FOREIGN KEY (negocio_id) REFERENCES reglas.negocio (negocio_id);
                CREATE INDEX IX_flujo_reglas_negocio_proceso_tipo_etapa_activo_prioridad
                    ON reglas.flujo_reglas (negocio_id, proceso, tipo_plan_id, etapa, activo, prioridad);
                CREATE INDEX IX_ejecucion_regla_negocio_id ON reglas.ejecucion_regla (negocio_id);
                CREATE UNIQUE INDEX UX_campo_regla_scope_codigo
                    ON reglas.campo_regla (negocio_codigo, proceso, tipo_plan_id, codigo)
                    WHERE negocio_codigo IS NOT NULL;
                CREATE INDEX IX_campo_regla_scope_activo
                    ON reglas.campo_regla (activo, negocio_codigo, proceso, tipo_plan_id);
                """);
        }
    }
}