# Modelo de datos - Feature 001

## 1. Principios

- Azure SQL es la fuente persistente.
- Nombres físicos en `snake_case`.
- Gobierno y auditoría en columnas relacionales.
- `definicion_json` contiene únicamente RuleDefinition neutral.
- `ISJSON` valida sintaxis, no conformidad con JSON Schema.

## 2. Relaciones

```text
ref.negocio 1---N mae.tipo_vinculacion 1---N mae.tipo_contrato 1---N mae.tipo_plan
ref.negocio 1---N reglas.flujo_reglas
mae.tipo_plan 1---N reglas.flujo_reglas
reglas.flujo_reglas 1---N reglas.regla 1---N reglas.version_regla
ref.negocio 1---N reglas.ejecucion_regla
cfg.dato_requerido 1---N reglas.campo_regla
reglas.campo_regla 1---N reglas.campo_regla_operador
```

RulesValidator mantiene seis tablas en el esquema `reglas`: `flujo_reglas`, `regla`, `version_regla`, `campo_regla`, `campo_regla_operador` y `ejecucion_regla`. No duplica `ref.negocio` ni `mae.tipo_plan`; `reglas.flujo_reglas` referencia sus claves existentes. Tampoco crea tablas propias de tipo de plan o tipo de vinculación.

`trx.dato_requerido_valor` y `trx.vinculacion` permanecen bajo propiedad del Core. El Rule Validator recibe los valores de evaluación en el contexto y no los copia ni persiste en sus tablas.

## 3. reglas.campo_regla

Catálogo operativo de rutas JSON autorizadas. Lo administra un proceso externo a Feature 001; se consulta en cada evaluación y no se mantiene una allow-list de campos en el código.

| Columna | Tipo | Nulo | Restricción |
|---|---|---:|---|
| campo_regla_id | bigint identity | No | PK |
| negocio_id | nvarchar(20) | Sí | FK `ref.negocio`; NULL significa scope global |
| proceso | varchar(100) | Sí | NULL significa cualquier proceso |
| tipo_plan_id | nvarchar(10) | Sí | FK `mae.tipo_plan`; NULL significa cualquier tipo de plan |
| codigo | varchar(150) | No | Ruta JSON plana; puede contener puntos |
| dato_requerido_id | nvarchar(10) | Sí | Mapping a `cfg.dato_requerido`; NULL conserva compatibilidad con aliases existentes |
| tipo_valor | varchar(20) | No | Tipo técnico fallback para campos aún no mapeados; al mapear, se deriva de `cfg.dato_requerido.tipo_dato_id` y `ref.tipo_dato` |
| activo | bit | No | |
| fecha_creacion | datetime2 | No | |
| usuario_creacion | varchar(100) | No | |
| fecha_modificacion | datetime2 | Sí | |
| usuario_modificacion | varchar(100) | Sí | |

UK: `(negocio_id, proceso, tipo_plan_id, codigo)`. `codigo` es el alias que usa RuleDefinition y se resuelve como clave JSON literal; no duplica el nombre, fuente, captura ni exigibilidad del dato definido en Core.

`cfg.dato_requerido` conserva la definición funcional de los datos. `reglas.campo_regla` solo relaciona el alias de RuleDefinition con ese dato, habilita o deshabilita su uso y aporta las restricciones propias del motor. El mapping es nullable durante la migración para conservar el catálogo existente; los campos que ya tengan mapping derivan el tipo desde Core.

El alias `pension.mesada` se mapea a `dato_requerido_id = 'DR-002'`.

`cfg.parametro` pertenece al Core y no se crea ni modifica desde RulesValidator. EF Core la mapea como entidad excluida de migrations con `parametro_id` (`nvarchar(40)`), `valor` (`nvarchar(max)`) y `vigencia` (`nvarchar(max)`, NULL o año `yyyy`). Las referencias de RuleDefinition resuelven `parametro_id` contra `fechaEvaluacion`; `valor` debe parsear como decimal y una vigencia informada debe coincidir con el año de evaluación.

## 4. reglas.campo_regla_operador

| Columna | Tipo | Nulo | Restricción |
|---|---|---:|---|
| campo_regla_id | bigint | No | PK compuesta; FK `campo_regla` |
| operador | varchar(30) | No | PK compuesta; CHECK operador neutral permitido |

Las reglas de catálogo describen el allow-list del motor, no duplican `cfg.dato_requerido`. El servicio carga el conjunto activo una vez por evaluación.

## 5. reglas.flujo_reglas

| Columna | Tipo | Nulo | Restricción |
|---|---|---:|---|
| flujo_reglas_id | bigint identity | No | PK |
| negocio_id | nvarchar(20) | Sí | FK `ref.negocio`; NULL identifica flujo global |
| codigo | varchar(100) | No | UK |
| nombre | varchar(200) | No | |
| proceso | varchar(100) | No | |
| tipo_plan_id | nvarchar(10) | Sí | FK `mae.tipo_plan`; NULL para otros procesos y flujos globales |
| etapa | int | Sí | NULL para flujos globales de proceso; si tiene valor, CHECK etapa > 0 |
| prioridad | int | No | CHECK prioridad >= 0 |
| activo | bit | No | |
| fecha_creacion | datetime2 | No | |
| usuario_creacion | varchar(100) | No | |
| fecha_modificacion | datetime2 | Sí | |
| usuario_modificacion | varchar(100) | Sí | |

Índice: `(negocio_id, proceso, tipo_plan_id, etapa, activo, prioridad)`.

Los flujos con `negocio_id = NULL` se limitan únicamente por `proceso`; usan `etapa = NULL` y `tipo_plan_id = NULL`. Los flujos específicos se seleccionan por `negocio_id`, proceso y etapa; para `VINCULACION`, además por `tipo_plan_id`.

## 6. reglas.regla

| Columna | Tipo | Nulo | Restricción |
|---|---|---:|---|
| regla_id | bigint identity | No | PK |
| flujo_reglas_id | bigint | No | FK `reglas.flujo_reglas` |
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

## 7. reglas.version_regla

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

## 8. reglas.ejecucion_regla

| Columna | Tipo | Nulo | Restricción |
|---|---|---:|---|
| ejecucion_regla_id | bigint identity | No | PK |
| id_evaluacion | uniqueidentifier | No | UK |
| negocio_id | nvarchar(20) | Sí | FK `ref.negocio`; NULL para auditorías globales históricas |
| proceso | varchar(100) | No | |
| tipo_plan_id | nvarchar(10) | Sí | FK `mae.tipo_plan` |
| etapa | int | Sí | NULL cuando el proceso no usa etapas |
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

## 9. Vigencia

```text
estado = ACTIVA
AND fecha_inicio_vigencia <= fechaEvaluacion
AND (fecha_fin_vigencia IS NULL OR fechaEvaluacion <= fecha_fin_vigencia)
```
