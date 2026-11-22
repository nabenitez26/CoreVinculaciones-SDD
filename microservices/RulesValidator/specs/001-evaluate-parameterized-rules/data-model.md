# Modelo de datos - Feature 001

## 1. Principios

- Azure SQL es la fuente persistente.
- Las tablas propiedad de RulesValidator utilizan el esquema `reglas`.
- Los nombres físicos utilizan `snake_case`.
- Gobierno y auditoría se mantienen en columnas relacionales.
- `definicion_json` contiene únicamente `RuleDefinition` neutral.
- `ISJSON` valida sintaxis JSON, no conformidad con JSON Schema.
- Los maestros existentes del Core no se duplican.
- Las entidades Core consumidas por RulesValidator se mapean en EF Core mediante `ExcludeFromMigrations()`.
- RulesValidator no persiste el contexto completo de evaluación ni PII.
- Microsoft RulesEngine permanece encapsulado en Infrastructure.
- Los campos y operadores utilizables por RuleDefinition se administran mediante allow-list persistente.

---

## 2. Límites de responsabilidad

### Core

El Core continúa siendo propietario de:

```text
ref.negocio
ref.tipo_dato
ref.fuente_dato
ref.forma_captura

mae.tipo_vinculacion
mae.tipo_contrato
mae.tipo_plan

cfg.dato_requerido
cfg.dato_requerido_exigibilidad
cfg.parametro

trx.dato_requerido_valor
trx.dato_valor_fuente_externa
trx.consulta_fuente_externa
trx.vinculacion
```

### RulesValidator

RulesValidator mantiene exclusivamente:

```text
reglas.campo_regla
reglas.campo_regla_operador
reglas.flujo_reglas
reglas.regla
reglas.version_regla
reglas.ejecucion_regla
reglas.fuente_externa_mapeo
```

RulesValidator no crea tablas propias para:

```text
negocio
tipo_plan
tipo_contrato
tipo_vinculacion
dato_requerido
parametro
fuente_dato
```

---

## 3. Relaciones

```text
ref.negocio
    1
    |
    N
mae.tipo_vinculacion
    1
    |
    N
mae.tipo_contrato
    1
    |
    N
mae.tipo_plan


ref.negocio
    1
    |
    N
reglas.flujo_reglas

mae.tipo_plan
    1
    |
    N
reglas.flujo_reglas


reglas.flujo_reglas
    1
    |
    N
reglas.regla
    1
    |
    N
reglas.version_regla


cfg.dato_requerido
    1
    |
    N
reglas.campo_regla

reglas.campo_regla
    1
    |
    N
reglas.campo_regla_operador


ref.negocio
    1
    |
    N
reglas.ejecucion_regla

mae.tipo_plan
    1
    |
    N
reglas.ejecucion_regla
```

`ref.fuente_dato` y `cfg.parametro` son catálogos Core consultados por Infrastructure, pero no requieren una relación física desde `reglas.version_regla`.

---

# 4. `reglas.campo_regla`

Catálogo operativo de rutas autorizadas para RuleDefinition.

## Columnas

| Columna | Tipo | Nulo | Restricción |
|---|---|---:|---|
| campo_regla_id | bigint identity | No | PK |
| negocio_id | nvarchar(20) | Sí | FK `ref.negocio`; NULL = scope global |
| proceso | varchar(100) | Sí | NULL = cualquier proceso |
| tipo_plan_id | nvarchar(10) | Sí | FK `mae.tipo_plan`; NULL = cualquier plan |
| codigo | varchar(150) | No | Alias utilizado por RuleDefinition |
| dato_requerido_id | nvarchar(10) | Sí | FK `cfg.dato_requerido` |
| tipo_valor | varchar(20) | No | Tipo utilizado por RulesValidator |
| activo | bit | No | |
| fecha_creacion | datetime2 | No | |
| usuario_creacion | varchar(100) | No | |
| fecha_modificacion | datetime2 | Sí | |
| usuario_modificacion | varchar(100) | Sí | |

Tipos:

```text
TEXTO
NUMERO
BOOLEANO
```

`codigo` es una ruta plana y puede contener puntos:

```text
pension.tipo
pension.mesada
persona.sexo
persona.edad
persona.tipoDocumento
persona.numeroDocumento
fuente.pension.respuesta
```

## Tipo de dato Core vs tipo del motor

`cfg.dato_requerido.tipo_dato_id` describe el dato según el modelo Core.

`reglas.campo_regla.tipo_valor` describe cómo RulesValidator debe interpretar el valor para ejecutar una regla.

Por tanto, `tipo_valor` no debe derivarse automáticamente de `cfg.dato_requerido.tipo_dato_id`.

Ejemplo:

```text
cfg.dato_requerido
DR-002
tipo_dato_id = TEXTO
```

puede mapear a:

```text
reglas.campo_regla
dato_requerido_id = DR-002
codigo = pension.mesada
tipo_valor = NUMERO
```

RulesValidator normaliza ese valor como número para las comparaciones del motor sin modificar la configuración del Core.

## Mappings conocidos

```text
DR-001
TIPO_DE_PENSION_RECONOCIDA
        ↓
pension.tipo
tipo_valor = TEXTO


DR-002
MESADA_PENSIONAL
        ↓
pension.mesada
tipo_valor = NUMERO


DR-017
        ↓
persona.tipoDocumento
tipo_valor = TEXTO
```

Los campos:

```text
persona.sexo
persona.edad
```

pueden permanecer con `dato_requerido_id = NULL` mientras no exista un mapping Core confirmado.

---

# 5. `reglas.campo_regla_operador`

Define la allow-list de operadores por campo.

| Columna | Tipo | Nulo | Restricción |
|---|---|---:|---|
| campo_regla_id | bigint | No | PK/FK |
| operador | varchar(30) | No | PK/CHECK |

Operadores:

```text
IGUAL
DIFERENTE
MAYOR
MAYOR_IGUAL
MENOR
MENOR_IGUAL
EN
NO_EN
```

Ejemplos:

```text
pension.tipo
    EN
    DIFERENTE

pension.mesada
    MENOR_IGUAL
    MAYOR
    MAYOR_IGUAL

persona.sexo
    IGUAL

persona.edad
    MAYOR_IGUAL

persona.tipoDocumento
    EN

fuente.pension.respuesta
    IGUAL
```

---

# 6. `reglas.flujo_reglas`

Representa un conjunto de reglas seleccionable según contexto.

| Columna | Tipo | Nulo | Restricción |
|---|---|---:|---|
| flujo_reglas_id | bigint identity | No | PK |
| negocio_id | nvarchar(20) | Sí | FK `ref.negocio`; NULL = global |
| codigo | varchar(100) | No | UK |
| nombre | varchar(200) | No | |
| proceso | varchar(100) | No | |
| tipo_plan_id | nvarchar(10) | Sí | FK `mae.tipo_plan` |
| etapa | int | Sí | CHECK etapa > 0 cuando tenga valor |
| prioridad | int | No | CHECK >= 0 |
| activo | bit | No | |
| fecha_creacion | datetime2 | No | |
| usuario_creacion | varchar(100) | No | |
| fecha_modificacion | datetime2 | Sí | |
| usuario_modificacion | varchar(100) | Sí | |

Índice:

```text
(negocio_id, proceso, tipo_plan_id, etapa, activo, prioridad)
```

Ejemplos:

```text
CAJA-VINCULACION-TP01-E4
CAJA-VINCULACION-TP02-E4

CAJA-VINCULACION-TP01-E5
CAJA-VINCULACION-TP02-E5
```

---

# 7. `reglas.regla`

Representa la identidad funcional de una regla dentro de un flujo.

| Columna | Tipo | Nulo |
|---|---|---:|
| regla_id | bigint identity | No |
| flujo_reglas_id | bigint | No |
| codigo | varchar(50) | No |
| nombre | varchar(200) | No |
| descripcion | varchar(1000) | Sí |
| tipo_regla | varchar(30) | No |
| prioridad | int | No |
| activo | bit | No |
| fecha_creacion | datetime2 | No |
| usuario_creacion | varchar(100) | No |

UK:

```text
(flujo_reglas_id, codigo)
```

Tipos actualmente soportados:

```text
PARAMETRIZABLE
ESPECIALIZADA
```

Para Feature 001:

```text
RV-001  PARAMETRIZABLE
RV-002  PARAMETRIZABLE
RV-003  PARAMETRIZABLE
RV-101  PARAMETRIZABLE
RV-102  PARAMETRIZABLE
RV-103  PARAMETRIZABLE
RV-104  PARAMETRIZABLE
```

RV-104 no requiere un tipo especial únicamente por utilizar información proveniente de una fuente externa.

La consulta de la fuente sucede antes de ejecutar RuleDefinition.

---

# 8. `reglas.version_regla`

Contiene las versiones inmutables de las reglas.

| Columna | Tipo | Nulo |
|---|---|---:|
| version_regla_id | bigint identity | No |
| regla_id | bigint | No |
| version | int | No |
| version_esquema | varchar(20) | No |
| estado | varchar(30) | No |
| severidad | varchar(30) | No |
| definicion_json | nvarchar(max) | No |
| codigo_error | varchar(100) | Sí |
| mensaje_error | nvarchar(1000) | Sí |
| fecha_inicio_vigencia | datetime2 | No |
| fecha_fin_vigencia | datetime2 | Sí |
| fecha_creacion | datetime2 | No |
| usuario_creacion | varchar(100) | No |
| fecha_aprobacion | datetime2 | Sí |
| usuario_aprobacion | varchar(100) | Sí |

UK:

```text
(regla_id, version)
```

Estados:

```text
BORRADOR
EN_REVISION
APROBADA
ACTIVA
INACTIVA
```

Severidades:

```text
INFORMATIVA
ADVERTENCIA
BLOQUEANTE
```

La metadata:

```text
estado
severidad
codigo_error
mensaje_error
vigencia
aprobación
```

tiene como única fuente de verdad las columnas relacionales.

No se duplica en `definicion_json`.

---

# 9. RuleDefinition

`definicion_json` almacena exclusivamente RuleDefinition neutral.

Ejemplo literal:

```json
{
  "versionEsquema": "1.0",
  "tipo": "CONDICION",
  "campo": "pension.tipo",
  "operador": "EN",
  "valor": [
    "Vejez",
    "Invalidez",
    "Sobrevivientes"
  ]
}
```

Ejemplo con parámetro:

```json
{
  "versionEsquema": "1.0",
  "tipo": "CONDICION",
  "campo": "pension.mesada",
  "operador": "MENOR_IGUAL",
  "valor": {
    "tipo": "PARAMETRO_MULTIPLICADO",
    "parametro": "SMMLV",
    "factor": 1.5
  }
}
```

Ejemplo de grupo:

```json
{
  "versionEsquema": "1.0",
  "tipo": "GRUPO",
  "operadorLogico": "O",
  "condiciones": []
}
```

Los operadores lógicos neutrales son:

```text
Y
O
```

---

# 10. Parámetros del Core

`cfg.parametro` pertenece al Core.

RulesValidator no crea ni modifica esa tabla.

EF Core la mapea como entidad Core excluida de migraciones.

Campos requeridos para Feature 001:

```text
parametro_id
valor
vigencia
```

Ejemplo:

```text
parametro_id = SMMLV
valor = 1750000
vigencia = 2026
```

Las referencias `PARAMETRO_MULTIPLICADO` se resuelven por:

```text
parametro_id
+
fechaEvaluacion
        ↓
valor vigente
        ↓
decimal
        ↓
× factor
```

Ejemplo:

```text
SMMLV = 1.750.000
factor = 1.5
resultado = 2.625.000
```

El valor resuelto se entrega al `ExpressionBuilder`.

---

# 11. Fuente externa FD-PENS

`ref.fuente_dato` pertenece al Core.

La fuente utilizada por RV-104 es:

```text
fuente_dato_id = FD-PENS
nombre = Fuente externa de pensionados (simulada en la POC)
tipo = Fuente externa de datos
guarda_documento = 0
```

RulesValidator no almacena la fuente en tablas propias del Core. Su mapeo funcional
(nombre del blob, separador, columnas y campos del contexto) se guarda en
`reglas.fuente_externa_mapeo`, que referencia `ref.fuente_dato` mediante FK.

`ref.fuente_dato` se mapea como entidad Core mediante:

```text
ExcludeFromMigrations()
```

Esquema de `reglas.fuente_externa_mapeo`:

| Columna | Tipo | Nulo | Restricción |
|---|---|---:|---|
| fuente_dato_id | nvarchar(10) | No | PK, FK a `ref.fuente_dato` |
| nombre_blob | varchar(255) | No | CHECK: termina en `.csv` |
| separador | varchar(1) | No | CHECK: `LEN(separador) = 1` |
| columna_clave | varchar(100) | No | CHECK: distinta de `columna_valor` |
| columna_valor | varchar(100) | No | |
| campo_clave_contexto | varchar(150) | No | Código del campo del contexto; no requiere estar en `campo_regla` |
| campo_regla_id | bigint | No | FK a `reglas.campo_regla`; campo que recibe la respuesta (TEXTO) |
| activo | bit | No | Solo una fila activa por fuente |
| fecha_creacion | datetime2 | No | |
| usuario_creacion | varchar(100) | No | |
| fecha_modificacion | datetime2 | Sí | |
| usuario_modificacion | varchar(100) | Sí | |

El valor se lee en cada evaluación que referencia el campo de respuesta, sin caché entre evaluaciones.
Si no existe una fila activa para la fuente, la evaluación falla con error de configuración.

FD-PENS no reemplaza ni modifica DR-001.

```text
DR-001
    ↓
Tipo de pensión declarado
    ↓
RV-001 / RV-102


FD-PENS
    ↓
Confirmación externa
    ↓
RV-104
```

---

# 12. Fuente física FD-PENS

La fuente se encuentra en Azure Blob Storage.

Nombre lógico:

```text
fuente_externa_simulada
```

Ejemplo de nombre físico:

```text
fuente_externa_simulada.csv
```

Contenido:

```text
NUMERO_DOCUMENTO;RESPUESTA
52310057;Reconocida
52310103;Reconocida
52310122;Reconocida
```

La ubicación física pertenece a configuración de Infrastructure.

No se almacena en:

```text
reglas.regla
reglas.version_regla
reglas.campo_regla
```

La configuración física contempla:

```text
FuenteId
Storage/Blob URI o cuenta
ContainerName
BlobName
```

No se almacenan credenciales ni SAS tokens en RuleDefinition.

---

# 13. Contexto enriquecido para RV-104

Para Etapa 5 el consumidor proporciona:

```text
persona.numeroDocumento
```

Infrastructure consulta FD-PENS.

Cuando existe una respuesta válida:

```text
RESPUESTA = Reconocida
```

RulesValidator incorpora únicamente al contexto interno:

```text
fuente.pension.respuesta = Reconocida
```

Ese campo debe existir en `reglas.campo_regla`:

```text
codigo = fuente.pension.respuesta
tipo_valor = TEXTO
dato_requerido_id = NULL
```

No corresponde a DR-001.

Operador permitido:

```text
IGUAL
```

RV-104 utiliza:

```json
{
  "versionEsquema": "1.0",
  "tipo": "CONDICION",
  "campo": "fuente.pension.respuesta",
  "operador": "IGUAL",
  "valor": "Reconocida"
}
```

---

# 14. Ausencia o indisponibilidad de la fuente

La ausencia de respuesta de FD-PENS no se representa inventando un valor funcional.

Estados conceptuales del proveedor:

```text
ENCONTRADO
SIN_RESPUESTA
NO_DISPONIBLE
INVALIDO
```

Cuando no existe una respuesta evaluable:

```text
RV-104 = INDETERMINADO
```

La versión de RV-104 utiliza:

```text
severidad = ADVERTENCIA
```

RulesValidator no crea directamente:

```text
Caso TC-03
```

ni actualiza:

```text
Etapa 5 = EN ESPERA
```

Esas acciones pertenecen al Core/orquestador.

---

# 15. `reglas.ejecucion_regla`

Registra la trazabilidad de la evaluación.

| Columna | Tipo | Nulo |
|---|---|---:|
| ejecucion_regla_id | bigint identity | No |
| id_evaluacion | uniqueidentifier | No |
| negocio_id | nvarchar(20) | Sí |
| proceso | varchar(100) | No |
| tipo_plan_id | nvarchar(10) | Sí |
| etapa | int | Sí |
| flujos_json | nvarchar(max) | No |
| resultado_general | varchar(30) | No |
| resultados_json | nvarchar(max) | No |
| fecha_evaluacion | datetime2 | No |
| fecha_ejecucion | datetime2 | No |
| duracion_ms | bigint | No |
| id_correlacion | varchar(100) | No |
| id_traza | varchar(100) | Sí |

Resultado general:

```text
CUMPLE
NO_CUMPLE
INDETERMINADO
```

No se persiste:

- contexto completo;
- número de documento;
- PII;
- contenido del blob;
- credenciales;
- respuesta completa de fuentes externas.

---

# 16. Vigencia

Una versión es evaluable cuando:

```text
regla.activo = 1

AND version_regla.estado = ACTIVA

AND version_regla.fecha_inicio_vigencia <= fechaEvaluacion

AND (
    version_regla.fecha_fin_vigencia IS NULL
    OR fechaEvaluacion <= version_regla.fecha_fin_vigencia
)
```

Los límites son inclusivos.

---

# 17. Fuente de verdad

| Información | Fuente de verdad |
|---|---|
| Negocios | `ref.negocio` |
| Tipos de vinculación | `mae.tipo_vinculacion` |
| Tipos de contrato | `mae.tipo_contrato` |
| Tipos de plan | `mae.tipo_plan` |
| Datos funcionales | `cfg.dato_requerido` |
| Exigibilidad | `cfg.dato_requerido_exigibilidad` |
| Parámetros como SMMLV | `cfg.parametro` |
| Fuentes externas | `ref.fuente_dato` |
| Valores del Core | `trx.dato_requerido_valor` |
| Flujos de reglas | `reglas.flujo_reglas` |
| Campos del motor | `reglas.campo_regla` |
| Operadores | `reglas.campo_regla_operador` |
| Reglas | `reglas.regla` |
| Versiones | `reglas.version_regla` |
| Auditoría del motor | `reglas.ejecucion_regla` |

---

# 18. Privacidad y seguridad

RulesValidator no debe almacenar ni registrar:

```text
persona.numeroDocumento
```

en logs o auditoría del motor.

La integración FD-PENS debe evitar almacenar permanentemente:

- archivo descargado;
- filas consultadas;
- números de documento;
- credenciales;
- tokens SAS.

Azure Blob Storage se consume desde Infrastructure.

La configuración física de la fuente no forma parte de RuleDefinition.

---

# 19. Tablas propiedad de RulesValidator

El modelo final de Feature 001 mantiene exactamente siete tablas:

```text
reglas.campo_regla
reglas.campo_regla_operador
reglas.flujo_reglas
reglas.regla
reglas.version_regla
reglas.ejecucion_regla
reglas.fuente_externa_mapeo
```

La tabla `reglas.fuente_externa_mapeo` fue aprobada para Etapa 5 / RV-104 (ver sección 11).