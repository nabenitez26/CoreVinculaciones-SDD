# Modelo de datos - Feature 001

## 1. Principios

- Azure SQL es la fuente persistente.
- Nombres físicos en `snake_case`.
- Gobierno y auditoría en columnas relacionales.
- `definicion_json` contiene únicamente RuleDefinition neutral.
- `ISJSON` valida sintaxis, no conformidad con JSON Schema.

## 2. Relaciones

```text
negocio 1---N flujo_reglas 1---N regla 1---N version_regla
negocio 1---N ejecucion_regla
```

## 3. negocio

| Columna | Tipo | Nulo | Restricción |
|---|---|---:|---|
| negocio_id | bigint identity | No | PK |
| codigo | varchar(50) | No | UK |
| nombre | varchar(150) | No | |
| descripcion | varchar(500) | Sí | |
| activo | bit | No | |
| fecha_creacion | datetime2 | No | |
| usuario_creacion | varchar(100) | No | |
| fecha_modificacion | datetime2 | Sí | |
| usuario_modificacion | varchar(100) | Sí | |

Debe existir `codigo = GLOBAL`.

## 4. flujo_reglas

| Columna | Tipo | Nulo | Restricción |
|---|---|---:|---|
| flujo_reglas_id | bigint identity | No | PK |
| negocio_id | bigint | No | FK negocio |
| codigo | varchar(100) | No | UK |
| nombre | varchar(200) | No | |
| proceso | varchar(100) | No | |
| etapa | int | No | CHECK etapa > 0 |
| prioridad | int | No | CHECK prioridad >= 0 |
| activo | bit | No | |
| fecha_creacion | datetime2 | No | |
| usuario_creacion | varchar(100) | No | |
| fecha_modificacion | datetime2 | Sí | |
| usuario_modificacion | varchar(100) | Sí | |

Índice: `(negocio_id, proceso, etapa, activo, prioridad)`.

La etapa se mantiene relacionalmente en `flujo_reglas`.

## 5. regla

| Columna | Tipo | Nulo | Restricción |
|---|---|---:|---|
| regla_id | bigint identity | No | PK |
| flujo_reglas_id | bigint | No | FK flujo_reglas |
| codigo | varchar(50) | No | UK compuesto |
| nombre | varchar(200) | No | |
| descripcion | varchar(1000) | Sí | |
| tipo_regla | varchar(30) | No | CHECK |
| prioridad | int | No | CHECK prioridad >= 0 |
| activo | bit | No | |
| fecha_creacion | datetime2 | No | |
| usuario_creacion | varchar(100) | No | |

UK: `(flujo_reglas_id, codigo)`.

`tipo_regla IN ('PARAMETRIZABLE','ESPECIALIZADA')`.

## 6. version_regla

| Columna | Tipo | Nulo | Restricción |
|---|---|---:|---|
| version_regla_id | bigint identity | No | PK |
| regla_id | bigint | No | FK regla |
| version | int | No | UK compuesto |
| version_esquema | varchar(20) | No | |
| estado | varchar(30) | No | CHECK |
| severidad | varchar(30) | No | CHECK |
| definicion_json | nvarchar(max) | No | ISJSON = 1 |
| codigo_error | varchar(100) | Sí | |
| mensaje_error | nvarchar(1000) | Sí | |
| fecha_inicio_vigencia | datetime2 | No | |
| fecha_fin_vigencia | datetime2 | Sí | CHECK |
| fecha_creacion | datetime2 | No | |
| usuario_creacion | varchar(100) | No | |
| fecha_aprobacion | datetime2 | Sí | |
| usuario_aprobacion | varchar(100) | Sí | |

UK: `(regla_id, version)`.

Estados: BORRADOR, EN_REVISION, APROBADA, ACTIVA, INACTIVA.

Severidades: INFORMATIVA, ADVERTENCIA, BLOQUEANTE.

`codigo_error`, `mensaje_error`, `severidad`, vigencias y aprobación son la única fuente de verdad relacional. No se duplican dentro de `definicion_json`.

## 7. ejecucion_regla

| Columna | Tipo | Nulo | Restricción |
|---|---|---:|---|
| ejecucion_regla_id | bigint identity | No | PK |
| id_evaluacion | uniqueidentifier | No | UK |
| negocio_id | bigint | No | FK negocio |
| proceso | varchar(100) | No | |
| etapa | int | No | |
| flujos_json | nvarchar(max) | No | ISJSON = 1 |
| resultado_general | varchar(30) | No | CHECK |
| resultados_json | nvarchar(max) | No | ISJSON = 1 |
| fecha_evaluacion | datetime2 | No | |
| fecha_ejecucion | datetime2 | No | |
| duracion_ms | bigint | No | CHECK >= 0 |
| id_correlacion | varchar(100) | No | |
| id_traza | varchar(100) | Sí | |

`resultado_general IN ('CUMPLE','NO_CUMPLE','INDETERMINADO')`.

No se persiste el contexto completo ni PII.

## 8. Vigencia

```text
estado = ACTIVA
AND fecha_inicio_vigencia <= fechaEvaluacion
AND (fecha_fin_vigencia IS NULL OR fechaEvaluacion <= fecha_fin_vigencia)
```
