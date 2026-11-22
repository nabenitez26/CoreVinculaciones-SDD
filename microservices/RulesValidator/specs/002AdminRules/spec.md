# Feature 002 - Administración de reglas

## 1. Objetivo

Permitir consultar, crear, versionar y administrar reglas de RulesValidator
mediante API, utilizando los catálogos y mecanismos de validación existentes.

Feature 002 extiende las capacidades administrativas del microservicio sin
modificar el comportamiento de evaluación definido en Feature 001.

La administración debe permitir construir reglas sin escribir expresiones C#,
Dynamic LINQ, SQL ni expresiones propias de Microsoft RulesEngine.

---

## 2. Alcance

Feature 002 incluye:

- consultar reglas por alcance;
- consultar el detalle de una regla;
- consultar historial de versiones;
- crear reglas;
- crear nuevas versiones;
- guardar versiones en BORRADOR;
- validar definiciones antes de persistirlas;
- enviar versiones a revisión;
- aprobar versiones;
- activar versiones;
- inactivar versiones;
- consultar negocios disponibles;
- consultar tipos de plan;
- consultar campos disponibles;
- consultar operadores permitidos;
- consultar parámetros Core disponibles.

Feature 002 no ejecuta reglas.

La evaluación continúa siendo responsabilidad de Feature 001 mediante:

```http
POST /api/v1/evaluar-reglas
```

---

## 3. Principios

- Una versión ACTIVA es inmutable.
- Los cambios funcionales se realizan mediante una nueva versión.
- No se eliminan físicamente versiones históricas.
- RuleDefinition continúa siendo el formato neutral del motor.
- La API administrativa no expone expresiones ejecutables.
- Los catálogos del Core continúan siendo fuente de verdad.
- RulesValidator no duplica maestros del Core.
- La UI futura consume los catálogos expuestos por la API.
- La UI no mantiene campos, operadores, parámetros o planes hardcodeados.
- Microsoft RulesEngine continúa encapsulado en Infrastructure.
- La administración no conoce la sintaxis del motor subyacente.

---

## 4. Alcance funcional de una regla

Una regla pertenece a un flujo determinado por:

```text
negocio
proceso
tipoPlan
etapa
```

Ejemplo:

```text
CAJA
VINCULACION
TP-01
Etapa 4
```

El alcance no forma parte de RuleDefinition.

El alcance determina qué reglas son candidatas durante una evaluación.

---

## 5. Creación de una regla

Para crear una regla se informa:

```text
codigo
nombre
descripcion
negocioId
proceso
tipoPlanId
etapa
severidad
codigoError
mensajeError
fechaInicioVigencia
definicion
```

Ejemplo conceptual:

```json
{
  "codigo": "RV-107",
  "nombre": "Mesada máxima especial",
  "descripcion": "Valida el máximo permitido para la mesada.",
  "alcance": {
    "negocioId": "CAJA",
    "proceso": "VINCULACION",
    "tipoPlanId": "TP-01",
    "etapa": 4
  },
  "severidad": "BLOQUEANTE",
  "codigoError": "ERR-RV-107",
  "mensajeError": "La mesada supera el máximo permitido.",
  "fechaInicioVigencia": "2026-12-01T00:00:00Z",
  "definicion": {
    "tipo": "CONDICION",
    "campo": "pension.mesada",
    "operador": "MENOR_IGUAL",
    "valor": {
      "tipo": "PARAMETRO_MULTIPLICADO",
      "parametro": "SMMLV",
      "factor": 2
    }
  }
}
```

La creación debe ser atómica.

Se crean:

```text
regla
+
version 1
```

La versión inicial se crea en:

```text
BORRADOR
```

No se activa automáticamente.

La API no recibe ni expone prioridad. La columna `reglas.regla.prioridad` se conserva por compatibilidad con Feature 001; las reglas nuevas usan prioridad técnica = 0 (C015, cerrada).

La coincidencia de alcance es exacta: `negocioId`, `proceso`, `tipoPlanId` y `etapa` se comparan con sus valores tal cual, incluidos los NULL. NULL representa alcance global, igual que en Feature 001. Ejemplo: `negocioId=null`, `proceso=VINCULACION`, `tipoPlanId=null`, `etapa=null` busca un flujo con esos valores exactos.

---

## 6. Flujo de reglas

La administración debe reutilizar un flujo existente cuando coincida exactamente:

```text
negocioId
proceso
tipoPlanId
etapa
```

Feature 002 no debe crear silenciosamente un flujo diferente por cada regla.

Si el flujo requerido no existe, la primera versión de Feature 002 debe rechazar
la operación con un error funcional controlado.

La administración explícita de flujos queda fuera del alcance inicial.

En la coincidencia de flujo, NULL es un valor más y no funciona como comodín: `negocioId`, `proceso`, `tipoPlanId` y `etapa` deben coincidir exactamente con `reglas.flujo_reglas`, incluidos los NULL.

Solo un flujo ACTIVO puede utilizarse para crear una regla. Reglas de resolución: 0 flujos activos coincidentes → 404 `FLUJO_NO_EXISTE`; 1 flujo activo coincidente → continuar; más de 1 flujo activo coincidente → 409 `FLUJO_AMBIGUO`. No se selecciona ninguno arbitrariamente. Los flujos inactivos no son elegibles. Feature 002 no activa ni reactiva flujos automáticamente.

---

## 7. Versionamiento

Una regla mantiene identidad estable mediante:

```text
regla.codigo
```

Los cambios se representan mediante:

```text
version_regla
```

Ejemplo:

```text
RV-002

v1  INACTIVA
v2  ACTIVA
v3  BORRADOR
```

Una versión ACTIVA no puede modificarse.

Una modificación sobre una regla con versión ACTIVA debe producir una nueva
versión en BORRADOR.

---

## 8. Ciclo de vida

Estados:

```text
BORRADOR
EN_REVISION
APROBADA
ACTIVA
INACTIVA
```

Transiciones permitidas:

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

No se permiten saltos arbitrarios.

No se permite:

```text
ACTIVA → BORRADOR
INACTIVA → ACTIVA
APROBADA → BORRADOR
APROBADA → EN_REVISION
EN_REVISION → BORRADOR
```

Máquina de estados definitiva (C003 y C004 cerradas): BORRADOR → EN_REVISION → APROBADA → ACTIVA → INACTIVA.

Cuando se requiera modificar una versión ya publicada, se crea una versión nueva.

---

## 9. BORRADOR

Una versión BORRADOR:

- puede modificarse;
- puede validar su RuleDefinition;
- no participa en evaluaciones;
- no puede convertirse directamente en ACTIVA.

La edición concurrente de BORRADOR usa last-write-wins en Feature 002 v1 (C008 cerrada): no hay rowversion, ETag ni If-Match. La revalidación del estado BORRADOR ocurre dentro de la transacción.

Mientras permanezca en BORRADOR pueden modificarse:

```text
severidad
codigoError
mensajeError
fechaInicioVigencia
fechaFinVigencia
definicion
```

---

## 10. EN_REVISION

Una versión EN_REVISION:

- ya no se modifica funcionalmente;
- puede ser evaluada para aprobación;
- no participa en evaluaciones productivas.

La transición desde BORRADOR requiere una definición válida.

---

## 11. APROBADA

Una versión APROBADA:

- pasó revisión;
- no se modifica;
- todavía no participa en evaluaciones;
- queda disponible para activación.

La identidad del aprobador y la fecha de aprobación deben quedar registradas.

---

## 12. ACTIVA

Una versión ACTIVA:

- es inmutable;
- puede ser utilizada por Feature 001;
- debe respetar su vigencia;
- puede pasar únicamente a INACTIVA.

La activación debe verificar que no se produzca una configuración incompatible
con otra versión activa de la misma regla.

---

## 13. INACTIVA

Una versión INACTIVA:

- conserva su historial;
- no participa en evaluaciones;
- no vuelve a activarse;
- no puede modificarse.

Los cambios posteriores se realizan creando otra versión.

---

## 14. Validación de RuleDefinition

Feature 002 reutiliza el mismo validador utilizado por Feature 001.

La definición debe superar:

```text
JSON Schema
    ↓
campos permitidos
    ↓
operadores permitidos
    ↓
tipos
    ↓
parámetros
    ↓
límites de complejidad
```

No se crea un segundo mecanismo de validación administrativo.

---

## 15. Campos

Los campos disponibles provienen de:

```text
reglas.campo_regla
```

La administración no permite utilizar un campo que no se encuentre activo en
el catálogo aplicable al alcance seleccionado.

Ejemplo:

```text
Mesada pensional

datoRequeridoId = DR-002
codigo = pension.mesada
tipo = NUMERO
```

La futura UI mostrará el nombre funcional.

RuleDefinition utiliza:

```text
pension.mesada
```

---

## 16. Operadores

Los operadores disponibles dependen del campo seleccionado y provienen de:

```text
reglas.campo_regla_operador
```

Ejemplo para un campo NUMERO:

```text
MAYOR
MAYOR_IGUAL
MENOR
MENOR_IGUAL
```

La futura UI puede representar:

```text
MENOR_IGUAL
```

como:

```text
Menor o igual que
```

pero el contrato utiliza el código neutral.

---

## 17. Valores literales

Una condición puede comparar contra un valor literal compatible con el tipo
del campo.

Ejemplo:

```json
{
  "tipo": "CONDICION",
  "campo": "persona.sexo",
  "operador": "IGUAL",
  "valor": "Femenino"
}
```

---

## 18. Parámetros Core

Las reglas numéricas pueden utilizar parámetros definidos en:

```text
cfg.parametro
```

Ejemplo:

```text
SMMLV
```

RuleDefinition utiliza:

```json
{
  "tipo": "PARAMETRO_MULTIPLICADO",
  "parametro": "SMMLV",
  "factor": 1.5
}
```

Feature 002 no modifica `cfg.parametro`.

Solamente consulta los parámetros disponibles.

---

## 19. Reglas compuestas

Feature 002 soporta condiciones y grupos definidos por RuleDefinition 1.0.

Ejemplo:

```text
O

├── pension.tipo != "Vejez"

├── Y
│   ├── persona.sexo = "Femenino"
│   └── persona.edad >= 57

└── Y
    ├── persona.sexo = "Masculino"
    └── persona.edad >= 62
```

La API no recibe expresiones booleanas libres.

Recibe estructuras tipadas de RuleDefinition.

---

## 20. Validación previa

Debe existir una operación para validar una definición sin guardarla.

Entrada:

```text
alcance
+
definicion
```

Salida conceptual:

```json
{
  "valida": true,
  "errores": []
}
```

Una validación exitosa no crea ni modifica registros.

---

## 21. Consultas administrativas

La API debe permitir consultar reglas filtrando por:

```text
negocioId
proceso
tipoPlanId
etapa
estado
```

Un parámetro de consulta omitido significa "no aplicar ese filtro". No equivale a alcance global: por ejemplo, `GET /api/v1/reglas` sin `negocioId` no filtra por negocio. En esta versión no existe un filtro para seleccionar únicamente reglas de alcance global.

Diferencia deliberada (T128): en `GET /api/v1/catalogos/campos` los parámetros representan el alcance de resolución. Omitidos o NULL resuelven el catálogo global aplicable para ese parámetro, no una búsqueda abierta sobre todos los campos. La regla de "filtro omitido = sin filtro" aplica solo a `GET /api/v1/reglas`.

Una consulta debe mostrar como mínimo:

```text
codigo
nombre
descripcion
alcance
version activa
estado
vigencia
severidad
```

---

## 22. Historial

Debe poder consultarse el historial de una regla:

```text
RV-002

v1 INACTIVA
v2 ACTIVA
v3 BORRADOR
```

El historial no se reconstruye desde logs.

La fuente persistente continúa siendo:

```text
reglas.version_regla
```

---

## 23. Catálogos para clientes administrativos

La API debe exponer catálogos de lectura para:

```text
negocios
tipos de plan
campos
operadores
parámetros
```

Estos endpoints existen para que una futura UI pueda construir reglas sin
listas hardcodeadas.

---

## 24. Auditoría

Las operaciones administrativas deben permitir identificar:

```text
usuario
fecha
regla
version
operacion
```

Los campos existentes:

```text
usuario_creacion
fecha_creacion
usuario_aprobacion
fecha_aprobacion
```

deben aprovecharse cuando correspondan.

La necesidad de una auditoría administrativa adicional se evalúa aparte y no
implica crear una tabla nueva en esta Feature sin una necesidad demostrada.

---

## 25. Seguridad

La API nunca acepta:

- expresiones C#;
- Dynamic LINQ;
- SQL;
- scripts;
- Workflow de Microsoft RulesEngine;
- Rule de Microsoft RulesEngine;
- URI ejecutables;
- helpers arbitrarios.

Toda definición continúa pasando por el mecanismo seguro de Feature 001.

### Identidad y autorización de operaciones de escritura

Las operaciones de escritura son: crear regla, crear versión, actualizar BORRADOR, enviar a revisión, aprobar, activar e inactivar.

**Cambio aprobado para la POC (temporal):** no hay `IUsuarioActual` ni autenticación obligatoria. El responsable de la operación se recibe en el body como `usuario`.

- Campo `usuario` obligatorio en: crear regla, crear versión, actualizar BORRADOR, enviar a revisión, aprobar, activar e inactivar.
- Tipo string, con trim aplicado, no vacío, máximo 100 caracteres. Si falta o es inválido: HTTP 400 `SOLICITUD_INVALIDA`.
- No se devuelve 401 ni 403 en la versión POC. `USUARIO_NO_AUTENTICADO` y `OPERACION_NO_AUTORIZADA` quedan fuera del alcance actual, pero se conservan como conceptos de la arquitectura productiva futura.
- Persistencia: `usuario` se guarda en `reglas.regla.usuario_creacion` (crear regla), `reglas.version_regla.usuario_creacion` (crear versión) y `reglas.version_regla.usuario_aprobacion` (aprobar, junto con `fecha_aprobacion` en UTC). En enviar-revisión, activar, inactivar y actualizar BORRADOR se valida pero no se persiste: el modelo actual no tiene columna de auditoría para esas operaciones. No se modificó el modelo SQL ni se generaron migraciones.
- **Declaración de seguridad:** el campo `usuario` es información declarada por el consumidor y NO representa una identidad autenticada. No se usa para autorización, no otorga permisos y no se considera prueba de identidad ni se utiliza para decisiones de seguridad.
- Precedencia actualizada: 1) validación estructural HTTP, incluido `usuario` (400 `SOLICITUD_INVALIDA`); 2) caso de uso.
- Consultas GET y `validar-definicion` no reciben ni requieren `usuario`.
- El RuleDefinition neutral persistible se construye en Application (versionEsquema "1.0" solo en la raíz). La API solo transporta el DTO administrativo (T134, cerrada).
- No hay valores de respaldo ni usuarios hardcodeados (`SYSTEM`, `ADMIN`, `USR-011`, `anonymous`, etc.). `USR-011` solo aparece como ejemplo del contrato.
- **Futuro (T131, T104, T069):** en la integración productiva, `body.usuario` se reemplazará por: Bearer Token → identidad corporativa → ClaimsPrincipal → `IUsuarioActual` → auditoría. El contrato futuro podrá eliminar `usuario` del body. Los roles administrativos y la autorización por operación (401/403) quedan pendientes de integración corporativa (C001).

---

## 26. Vigencias activas

Una regla puede mantener varias versiones ACTIVA únicamente cuando sus
períodos efectivos no se superponen.

La activación de una versión debe validar las vigencias de todas las versiones
ACTIVA de la misma regla.

Si una nueva versión reemplaza una versión actualmente activa dentro de un
período superpuesto, la versión reemplazada pasa a INACTIVA dentro de la misma
transacción.

No puede existir más de una versión aplicable para una misma regla y una misma
`fechaEvaluacion`.

C006 está cerrada (`clarifications.md`): varias versiones ACTIVA solo si sus vigencias no se superponen. La activación valida el solapamiento dentro de la misma transacción.

---

## 27. Errores funcionales esperados

Ejemplos:

```text
REGLA_YA_EXISTE
REGLA_NO_EXISTE
FLUJO_NO_EXISTE
VERSION_NO_EXISTE
VERSION_INMUTABLE
TRANSICION_ESTADO_INVALIDA
DEFINICION_REGLA_INVALIDA
CAMPO_NO_PERMITIDO
OPERADOR_NO_PERMITIDO
PARAMETRO_NO_EXISTE
VIGENCIA_INVALIDA
CONFLICTO_VERSION_ACTIVA
FLUJO_AMBIGUO
```

Códigos 401 (`USUARIO_NO_AUTENTICADO`) y 403 (`OPERACION_NO_AUTORIZADA`) fuera del alcance de la POC; reservados para la integración productiva futura.
Usuario faltante o inválido en escrituras: 400 `SOLICITUD_INVALIDA`.

Los errores HTTP utilizan ProblemDetails.

---

## 28. Fuera de alcance

- evaluar reglas;
- editar Feature 001;
- editar versiones ACTIVA;
- eliminar físicamente reglas/versiones;
- modificar `cfg.dato_requerido`;
- modificar `cfg.parametro`;
- modificar maestros Core;
- crear o modificar tipos de plan;
- administrar fuentes externas;
- subir archivos a Azure Blob Storage;
- modificar `fuente_externa_mapeo`;
- administrar plantillas;
- administrar exigibilidades;
- construir la UI;
- rollback destructivo.

