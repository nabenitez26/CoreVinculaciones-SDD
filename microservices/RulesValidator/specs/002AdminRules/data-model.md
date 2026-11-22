# Modelo de datos - Feature 002

## 1. Objetivo

Feature 002 reutiliza el modelo persistente creado para Feature 001.

El objetivo es administrar:

```text
flujos
reglas
versiones
```

sin duplicar catálogos Core ni introducir inicialmente nuevas tablas.

---

## 2. Modelo reutilizado

```text
reglas.flujo_reglas
        1
        │
        N
reglas.regla
        1
        │
        N
reglas.version_regla
```

Catálogos relacionados:

```text
ref.negocio
mae.tipo_plan
cfg.parametro
cfg.dato_requerido

reglas.campo_regla
reglas.campo_regla_operador
```

Feature 002 consulta estos catálogos para validar las operaciones administrativas.

---

## 3. `reglas.flujo_reglas`

Feature 002 utiliza el flujo para determinar el alcance de la regla:

```text
negocio
proceso
tipoPlan
etapa
```

La creación inicial de una regla debe reutilizar un flujo existente con
coincidencia exacta.

No se crea automáticamente un flujo nuevo en la primera versión de Feature 002.

NULL forma parte de la coincidencia exacta: `negocioId`, `proceso`, `tipoPlanId` y `etapa` se comparan con sus valores tal cual, incluidos NULL (alcance global, igual que Feature 001). Solo flujos ACTIVOS son elegibles: 0 coincidencias → `FLUJO_NO_EXISTE`; más de 1 → `FLUJO_AMBIGUO` (409).

Ejemplo:

```text
CAJA
VINCULACION
TP-01
4
        ↓
CAJA-VINCULACION-TP01-E4
```

La gestión explícita de flujos se considera una capacidad administrativa
independiente.

---

## 4. `reglas.regla`

Representa la identidad estable de la regla.

Campos relevantes:

| Columna | Uso Feature 002 |
|---|---|
| regla_id | Identificador técnico |
| flujo_reglas_id | Alcance |
| codigo | Identificador funcional |
| nombre | Nombre visible |
| descripcion | Descripción |
| tipo_regla | Tipo de regla |
| prioridad | Valor técnico 0 en reglas nuevas; no se expone ni se edita (C015) |
| activo | Estado de identidad |
| fecha_creacion | Auditoría |
| usuario_creacion | Auditoría |

El código de una regla es estable.

Ejemplo:

```text
RV-002
```

no cambia al crear:

```text
v2
v3
v4
```

---

## 5. `reglas.version_regla`

Representa una versión funcional de la regla.

Campos:

```text
version_regla_id
regla_id
version
version_esquema
estado
severidad
definicion_json
codigo_error
mensaje_error
fecha_inicio_vigencia
fecha_fin_vigencia
fecha_creacion
usuario_creacion
fecha_aprobacion
usuario_aprobacion
```

UK:

```text
(regla_id, version)
```

---

## 6. Versionamiento

Ejemplo:

```text
regla
RV-002
    │
    ├── version 1
    │      estado = INACTIVA
    │
    ├── version 2
    │      estado = ACTIVA
    │
    └── version 3
           estado = BORRADOR
```

Una versión nueva utiliza:

```text
MAX(version) + 1
```

La determinación del siguiente número debe realizarse de manera segura frente
a concurrencia.

No confiar únicamente en leer `MAX(version)` y posteriormente insertar sin
protección transaccional.

---

## 7. Estados

Estados soportados:

```text
BORRADOR
EN_REVISION
APROBADA
ACTIVA
INACTIVA
```

Máquina de estados:

```text
BORRADOR
   ↓
EN_REVISION
   ↓
APROBADA
   ↓
ACTIVA
   ↓
INACTIVA
```

Transiciones permitidas (una operación explícita por transición):

```text
BORRADOR → EN_REVISION   enviar-revision
EN_REVISION → APROBADA   aprobar
APROBADA → ACTIVA        activar
ACTIVA → INACTIVA        inactivar
```

No se permiten retrocesos ni transiciones que salten estados. Una versión
INACTIVA no vuelve a activarse; los cambios se hacen creando otra versión.
No existe un endpoint genérico de cambio de estado.

El estado actual se persiste en:

```text
reglas.version_regla.estado
```

---

## 8. Inmutabilidad

Las versiones:

```text
ACTIVA
INACTIVA
```

son inmutables funcionalmente.

Las versiones:

```text
EN_REVISION
APROBADA
```

no se modifican después de entrar en esos estados.

La edición funcional queda limitada a:

```text
BORRADOR
```

---

## 9. Activación

Antes de activar una versión debe validarse:

```text
estado actual = APROBADA
regla activa
definición válida
vigencia válida
```

Debe evitarse una configuración activa incompatible con otra versión de la
misma regla.

La convivencia de versiones ACTIVA según vigencia está cerrada en `clarifications.md` (C006).

Implementación (T113, T139):

- La activación se ejecuta en una transacción serializable.
- Las versiones ACTIVA con vigencia solapada con la objetivo pasan a INACTIVA.
  Todas las solapadas se desactivan, no solo una.
- La versión objetivo pasa a ACTIVA en la misma transacción. Si falla cualquier
  cambio, se revierte todo.
- Las versiones ACTIVA con vigencias no solapadas coexisten.
- Invariante: para una regla y una fechaEvaluacion existe como máximo una versión
  ACTIVA aplicable.
- `CONFLICTO_VERSION_ACTIVA` se mantiene como código reservado para un conflicto
  funcional de activación/vigencia (`clarifications.md`, T138). Hoy no se emite,
  porque todo solapamiento se resuelve como reemplazo válido.
- Los conflictos técnicos por concurrencia se reportan como `CONFLICTO_CONCURRENCIA`.

---

## 10. Definición

`definicion_json` continúa almacenando exclusivamente RuleDefinition.

Feature 002 recibe una estructura JSON tipada y Application controla su
serialización.

No se expone una operación administrativa cuyo propósito sea guardar
directamente un string arbitrario en `definicion_json`.

---

## 11. `reglas.campo_regla`

Feature 002 utiliza esta tabla para construir y validar reglas.

Permite consultar:

```text
codigo
tipo_valor
dato_requerido_id
scope
activo
```

Ejemplo:

```text
codigo = pension.mesada
dato_requerido_id = DR-002
tipo_valor = NUMERO
```

No se administra `campo_regla` inicialmente desde Feature 002.

---

## 12. `reglas.campo_regla_operador`

Feature 002 la utiliza para determinar operadores válidos por campo.

Ejemplo:

```text
pension.mesada
├── MAYOR
├── MAYOR_IGUAL
├── MENOR
└── MENOR_IGUAL
```

No se administran operadores desde Feature 002.

---

## 13. Parámetros Core

`cfg.parametro` continúa siendo propiedad del Core.

Feature 002 consulta parámetros para construir RuleDefinition.

Ejemplo:

```text
SMMLV
1750000
2026
```

Feature 002 no modifica dichos valores.

---

## 14. Jerarquía Core

La relación:

```text
ref.negocio
    ↓
mae.tipo_vinculacion
    ↓
mae.tipo_contrato
    ↓
mae.tipo_plan
```

se utiliza para validar que un `tipoPlanId` pertenezca al negocio seleccionado.

No se confía únicamente en la existencia individual de ambas FK.

---

## 15. Transacciones

Deben ser transaccionales:

### Crear regla

```text
validaciones
+
insert regla
+
insert version 1 BORRADOR
```

### Crear versión

```text
determinar siguiente versión
+
insert version BORRADOR
```

### Activar

```text
validaciones de estado
+
validaciones de conflicto
+
cambio de estado
```

No dejar estados parciales ante errores.

---

## 16. Concurrencia

Feature 002 debe proteger:

```text
dos creaciones simultáneas del mismo código;
dos nuevas versiones simultáneas;
doble aprobación;
doble activación;
activación concurrente de versiones incompatibles.
```

Los UK actuales constituyen la última defensa de integridad:

```text
flujo_reglas_id + codigo
regla_id + version
```

Application debe transformar conflictos de concurrencia conocidos en errores
controlados.

Antes de Incremento 2 debe compararse el modelo EF, la migración inicial y la BD real para `UK_regla_flujo_codigo` y `UK_version_regla_regla_version` (ver tasks.md, T-EF-01). No se generan migraciones automáticamente.

Edición concurrente de BORRADOR: last-write-wins (C008 cerrada para Feature 002 v1). No hay rowversion ni ETag, y no se genera migración por este motivo.

---

## 17. Auditoría existente

Se reutilizan:

```text
fecha_creacion
usuario_creacion
fecha_aprobacion
usuario_aprobacion
```

Feature 002 no introduce inicialmente otra tabla de auditoría.

Si posteriormente se requiere conservar cada transición completa:

```text
BORRADOR → EN_REVISION
EN_REVISION → APROBADA
APROBADA → ACTIVA
ACTIVA → INACTIVA
```

se evaluará una extensión independiente del modelo.

---

## 18. Migraciones

La primera implementación de Feature 002 no requiere cambios estructurales.

No generar migraciones mientras el alcance pueda implementarse con:

```text
flujo_reglas
regla
version_regla
campo_regla
campo_regla_operador
```

Cualquier migración futura debe justificarse contra una necesidad funcional
específica.

---

## 19. Fuente de verdad

| Información | Fuente |
|---|---|
| Negocios | `ref.negocio` |
| Planes | `mae.tipo_plan` |
| Datos funcionales | `cfg.dato_requerido` |
| Parámetros | `cfg.parametro` |
| Campos utilizables | `reglas.campo_regla` |
| Operadores | `reglas.campo_regla_operador` |
| Alcance | `reglas.flujo_reglas` |
| Identidad de regla | `reglas.regla` |
| Versión | `reglas.version_regla` |
| Estado | `reglas.version_regla.estado` |
| Definición | `reglas.version_regla.definicion_json` |
| Vigencia | `reglas.version_regla` |
| Severidad | `reglas.version_regla.severidad` |