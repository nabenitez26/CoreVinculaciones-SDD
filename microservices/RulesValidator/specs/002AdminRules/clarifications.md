# Clarificaciones - Feature 002

Este documento contiene decisiones pendientes que no deben ser resueltas
unilateralmente durante la implementación.

---

## C001 - Roles administrativos

### Pregunta

¿Qué perfiles pueden:

- consultar;
- editar BORRADOR;
- enviar a revisión;
- aprobar;
- activar;
- inactivar?

### Estado

PENDIENTE.

### Restricción

No hardcodear usuarios ni roles hasta cerrar esta decisión.

---

## C002 - Creación automática de flujos

### Pregunta

Si al crear una regla no existe un flujo con:

```text
negocio
proceso
tipoPlan
etapa
```

¿Feature 002 debe crearlo automáticamente?

### Propuesta inicial

No.

La creación de regla requiere un flujo existente.

### Razón

Evita que una operación de creación de regla cambie implícitamente la estructura
de selección del motor.

### Estado

PROPUESTA.

---

## C003 - Retorno desde EN_REVISION

### Pregunta

¿Una versión EN_REVISION puede devolverse a BORRADOR?

### Propuesta inicial

No en primera versión.

Si se requieren cambios, crear una nueva versión o incorporar posteriormente
una transición explícita de devolución con auditoría.

### Estado

CERRADA.

Decisión: EN_REVISION no puede volver a BORRADOR. Cualquier cambio requiere una nueva versión.

---

## C004 - Retorno desde APROBADA

### Pregunta

¿Una versión APROBADA puede regresar a BORRADOR o EN_REVISION?

### Propuesta

No.

### Estado

CERRADA.

Decisión: APROBADA no puede volver a EN_REVISION ni a BORRADOR. Máquina de estados definitiva:

BORRADOR -> EN_REVISION -> APROBADA -> ACTIVA -> INACTIVA.

---

## C005 - Activación de una nueva versión

### Decisión

Cuando una nueva versión reemplaza una versión ACTIVA cuya vigencia se
superpone, la versión anterior debe pasar a INACTIVA dentro de la misma
transacción que activa la nueva versión.

Una versión ACTIVA no se modifica funcionalmente. El cambio siempre queda
representado mediante una nueva versión.

### Estado

CERRADA.

---

## C006 - Múltiples versiones ACTIVA

### Decisión

Una regla puede tener más de una versión con estado ACTIVA siempre que sus
períodos de vigencia no se superpongan.

Ejemplo válido:

```text
v1 ACTIVA
2026-01-01 → 2026-12-31

v2 ACTIVA
2027-01-01 → NULL
```

### Ventaja

Permite programar configuración futura.

### Riesgo

Aumenta la complejidad administrativa y de activación.

### Estado

CERRADA.

Decisión: se permiten múltiples versiones ACTIVA solamente cuando sus vigencias no se superponen. Invariante: para una regla y una fechaEvaluacion existe como máximo una versión ACTIVA aplicable.

---

## C007 - Vigencias futuras

### Propuesta

Permitir:

```text
fecha_inicio_vigencia > fecha actual
```

para versiones APROBADAS o ACTIVA programadas, sujeto a la decisión C006.

### Estado

DESBLOQUEADA: la dependencia C006 está cerrada. Pendiente de confirmación explícita de la propuesta de vigencias futuras.

---

## Decisión POC - Usuario declarado en el body (temporal)

### Estado

CERRADA para la POC. Temporal.

### Decisión

No se implementa por ahora `IUsuarioActual` ni autenticación obligatoria en operaciones administrativas. Las escrituras reciben `usuario` en el body (`"usuario": "USR-011"` como ejemplo).

Operaciones: crear regla, crear versión, actualizar BORRADOR, enviar a revisión, aprobar, activar, inactivar. GET y `validar-definicion` no lo reciben.

Validación: obligatorio, string, trim, no vacío, máximo 100. Inválido o ausente → 400 `SOLICITUD_INVALIDA`. No se devuelve 401 ni 403 en esta versión.

Persistencia: `usuario_creacion` (regla y versión), `usuario_aprobacion` + `fecha_aprobacion` (aprobar). Las demás transiciones validan pero no persisten (no existe columna). Sin cambios de modelo ni migraciones.

### Declaración de seguridad

`usuario` es información declarada por el consumidor. NO es identidad autenticada. No se usa para autorización, no otorga permisos y no es prueba de identidad.

### Futuro

`body.usuario` se reemplazará por Bearer Token → identidad corporativa → ClaimsPrincipal → `IUsuarioActual` → auditoría. T104 y T131 quedan diferidas para integración productiva de identidad. No bloquean la POC. T069 (autorización por rol) también queda diferida.

Los códigos 401 `USUARIO_NO_AUTENTICADO` y 403 `OPERACION_NO_AUTORIZADA` se retiran del alcance actual de la POC y se conservan para la arquitectura productiva futura.

---

## Decisión T138 - Códigos de conflicto de activación

### Estado

CERRADA.

Decisión: se mantiene `CONFLICTO_VERSION_ACTIVA` en `spec.md` §27.

- `CONFLICTO_VERSION_ACTIVA` = conflicto funcional de activación/vigencia. Aplica
  solo si la nueva configuración no puede quedar válidamente activa.
- Un reemplazo válido no devuelve este código: la ACTIVA anterior pasa a INACTIVA y
  la APROBADA nueva pasa a ACTIVA en una sola transacción.
- Versiones ACTIVA con vigencias no solapadas siguen permitidas.
- `CONFLICTO_CONCURRENCIA` = conflicto técnico por operaciones concurrentes.
- Ninguno de los dos códigos se retira.

---

## C008 - Concurrencia

### Pregunta

¿Qué mecanismo se utilizará para edición concurrente?

Opciones:

```text
rowversion
ETag
control optimista por estado/fecha
```

### Estado

CERRADA para Feature 002 v1.

Decisión: la edición concurrente de una versión BORRADOR usa last-write-wins. No se agregan rowversion, ETag ni If-Match, y no se genera migración por concurrencia de edición. Se mantienen la protección transaccional y los constraints para: creación concurrente de regla, asignación concurrente de número de versión, aprobación, activación, reemplazo de versión ACTIVA y cumplimiento de vigencias no superpuestas.

### Condición

No introducir una columna estructural hasta confirmar la necesidad.

---

## C009 - Administración de `campo_regla`

### Pregunta

¿Feature 002 permite crear aliases nuevos?

### Propuesta

No en primera versión.

Feature 002 selecciona exclusivamente:

```text
campo_regla activo
```

### Estado

PROPUESTA.

---

## C010 - Administración de operadores

### Pregunta

¿La administración puede modificar los operadores permitidos de un campo?

### Propuesta

No en primera versión.

### Estado

PROPUESTA.

---

## C011 - Administración de parámetros

### Pregunta

¿Feature 002 modifica `cfg.parametro`?

### Decisión

No.

`cfg.parametro` pertenece al Core.

Feature 002 es únicamente consumidor.

### Estado

CERRADA.

---

## C012 - Fuentes externas

### Pregunta

¿Feature 002 administra `fuente_externa_mapeo`?

### Propuesta

No en primera versión.

El alcance inicial se limita a reglas y versiones.

### Estado

PROPUESTA.

---

## C013 - Eliminación de reglas

### Pregunta

¿Puede eliminarse una regla?

### Propuesta

No existe DELETE físico.

La inactivación funcional se realiza mediante los mecanismos de estado
permitidos.

### Estado

PROPUESTA.

---

## C014 - Código de regla

### Pregunta

¿Puede cambiar `RV-002` por otro código después de creada?

### Propuesta

No.

El código forma parte de la identidad funcional.

### Estado

PROPUESTA.

---

## C015 - Prioridad

### Decisión

La prioridad no forma parte del contrato administrativo de Feature 002.

La ejecución funcional se organiza por proceso, flujo y etapa.

Feature 002:

- no solicita prioridad al consumidor;
- no permite editar prioridad;
- no la presenta como parámetro funcional de configuración.

La columna existente `reglas.regla.prioridad` se conserva por compatibilidad
con Feature 001 y no requiere una migración en esta Feature.

Al crear una regla se utiliza el valor técnico predeterminado definido por la
implementación.

### Estado

CERRADA.

```text
reglas.regla.prioridad
```

Por tanto no está versionada.

### Riesgo

Cambiar la prioridad modifica el comportamiento de ejecución sin crear una nueva
versión.

### Estado

CERRADA.

### Decisión final

Feature 002 no expone ni permite modificar prioridad. Las reglas nuevas usan prioridad técnica = 0. `reglas.regla.prioridad` se conserva por compatibilidad. No genera migración.

---

## C016 - Nombre y descripción

### Pregunta

¿Nombre y descripción pueden modificarse sin crear versión?

### Modelo actual

Existen en:

```text
reglas.regla
```

y no en:

```text
version_regla
```

### Propuesta

Considerarlos metadata de identidad y permitir modificación controlada
únicamente si se decide que no representan comportamiento funcional.

### Estado

PENDIENTE.

---

## C017 - Auditoría completa de transiciones

### Pregunta

¿Se requiere historial de cada transición?

Ejemplo:

```text
BORRADOR → EN_REVISION
EN_REVISION → APROBADA
APROBADA → ACTIVA
```

El modelo actual registra creación y aprobación, pero no representa cada
transición históricamente.

### Estado

PENDIENTE.

### Restricción

No crear tabla nueva hasta confirmar este requerimiento.

---

## C018 - Simulación antes de activar

### Pregunta

¿Debe poder evaluarse una versión BORRADOR/APROBADA contra un contexto de
prueba sin activarla?

### Valor potencial

Permitirá que la futura UI tenga:

```text
Probar regla
```

antes de publicación.

### Estado

FUERA DEL ALCANCE INICIAL.

Puede evaluarse como incremento posterior.