-- Carga inicial del mapeo funcional de FD-PENS en reglas.fuente_externa_mapeo.
-- Requiere que ref.fuente_dato contenga FD-PENS (Core) y que reglas.campo_regla contenga
-- el campo global 'fuente.pension.respuesta' (TEXTO). Ejecutar manualmente tras la migración
-- AgregarFuenteExternaMapeo. No ejecutar desde la aplicación.

IF NOT EXISTS (SELECT 1 FROM reglas.fuente_externa_mapeo WHERE fuente_dato_id = N'FD-PENS')
BEGIN
    INSERT INTO reglas.fuente_externa_mapeo
        (fuente_dato_id, nombre_blob, separador, columna_clave, columna_valor,
         campo_clave_contexto, campo_regla_id, activo, fecha_creacion, usuario_creacion)
    SELECT
        N'FD-PENS',
        'fuente_externa_simulada.csv',
        ';',
        'NUMERO_DOCUMENTO',
        'RESPUESTA',
        'persona.numeroDocumento',
        cr.campo_regla_id,
        1,
        SYSUTCDATETIME(),
        'carga_inicial'
    FROM reglas.campo_regla AS cr
    WHERE cr.codigo = 'fuente.pension.respuesta'
      AND cr.negocio_id IS NULL
      AND cr.proceso IS NULL
      AND cr.tipo_plan_id IS NULL;
END
