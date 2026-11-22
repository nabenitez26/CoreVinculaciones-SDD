# Acceptance Tests - Feature 002

## 1. Objetivo

Validar que la API administrativa permita consultar, crear, versionar y
gestionar reglas sin modificar el comportamiento del motor de evaluación
definido en Feature 001.

Los escenarios descritos en este documento son criterios de aceptación
funcionales.

---

## AT-001 - Consultar reglas por alcance

### Dado

Existen reglas configuradas para:

CAJA / VINCULACION / TP-01 / Etapa 4

### Cuando

Se consulta:

GET /api/v1/reglas?negocioId=CAJA&proceso=VINCULACION&tipoPlanId=TP-01&etapa=4

### Entonces

La respuesta es HTTP 200.

Incluye las reglas correspondientes al alcance solicitado.

Cada elemento contiene al menos:

- reglaId;
- codigo;
- nombre;
- alcance;
- versiones activas.

No devuelve reglas pertenecientes exclusivamente a TP-02.

---

## AT-002 - Consultar regla por reglaId

### Dado

Existe una regla administrativa con un reglaId válido.

### Cuando

Se consulta:

GET /api/v1/reglas/{reglaId}

### Entonces

La respuesta es HTTP 200.

Incluye:

- reglaId;
- código;
- nombre;
- descripción;
- alcance;
- historial de versiones.

---

## AT-003 - reglaId inexistente

### Cuando

Se consulta un reglaId que no existe.

### Entonces

La respuesta es HTTP 404 con ProblemDetails.

Código esperado:

REGLA_NO_EXISTE

---

## AT-004 - Crear una regla

### Dado

Existe el flujo:

CAJA / VINCULACION / TP-01 / Etapa 4

Y el campo:

pension.mesada

está activo y permite:

MENOR_IGUAL

Y existe el parámetro:

SMMLV

### Cuando

Se envía:

POST /api/v1/reglas

con:

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

### Entonces

La respuesta es HTTP 201.

Se crea:

- una nueva regla;
- versión 1;
- estado BORRADOR.

La regla no participa todavía en Feature 001.

---

## AT-005 - No crear regla con código duplicado en el mismo flujo

### Dado

Ya existe RV-107 en el flujo seleccionado.

### Cuando

Se intenta crear nuevamente RV-107 en el mismo flujo.

### Entonces

La respuesta es HTTP 409.

Código esperado:

REGLA_YA_EXISTE

No se crea una segunda regla.

No se crea una versión adicional.

---

## AT-006 - Mismo código en otro flujo

### Dado

RV-001 puede existir en más de un flujo.

### Cuando

Se consultan sus instancias administrativas.

### Entonces

Cada instancia posee un reglaId diferente.

El sistema no utiliza solamente codigoRegla para identificar una regla.

---

## AT-007 - Crear regla con flujo inexistente

### Cuando

Se intenta crear una regla para una combinación sin flujo existente.

### Entonces

La respuesta es HTTP 404.

Código esperado:

FLUJO_NO_EXISTE

Feature 002 no crea automáticamente el flujo.

---

## AT-008 - Validar definición sin persistir

### Cuando

Se envía:

POST /api/v1/reglas/validar-definicion

con una definición válida.

### Entonces

La respuesta es HTTP 200:

{
  "valida": true,
  "errores": []
}

No se crea:

- regla;
- versión;
- ejecución.

---

## AT-009 - Rechazar campo no permitido

### Cuando

La definición utiliza:

campo = campo.no.autorizado

### Entonces

La validación retorna:

valida = false

con código:

CAMPO_NO_PERMITIDO

La definición no se persiste.

---

## AT-010 - Rechazar operador no permitido

### Dado

El campo seleccionado no tiene autorizado MAYOR.

### Cuando

La definición utiliza:

operador = MAYOR

### Entonces

La validación falla con:

OPERADOR_NO_PERMITIDO

---

## AT-011 - Validar PARAMETRO_MULTIPLICADO

### Dado

Existe:

SMMLV

### Cuando

Se valida:

{
  "tipo": "PARAMETRO_MULTIPLICADO",
  "parametro": "SMMLV",
  "factor": 1.5
}

sobre un campo NUMERO compatible.

### Entonces

La definición es válida.

No se almacena el valor monetario actual de SMMLV dentro de RuleDefinition.

---

## AT-012 - Rechazar parámetro inexistente

### Cuando

Se configura:

parametro = PARAMETRO_INEXISTENTE

### Entonces

La definición es rechazada.

Código esperado:

PARAMETRO_NO_EXISTE

---

# Versionamiento

## AT-013 - Crear nueva versión

### Dado

Existe:

RV-107 v1

### Cuando

Se ejecuta:

POST /api/v1/reglas/{reglaId}/versiones

### Entonces

Se crea:

RV-107 v2

con:

estado = BORRADOR

RV-107 v1 permanece sin modificaciones.

---

## AT-014 - Actualizar BORRADOR

### Dado

RV-107 v2 se encuentra en BORRADOR.

### Cuando

Se actualizan:

- severidad;
- mensaje;
- vigencia;
- definición.

### Entonces

La operación retorna HTTP 200.

La versión actualizada continúa en:

BORRADOR

---

## AT-015 - No modificar EN_REVISION

### Dado

RV-107 v2 está:

EN_REVISION

### Cuando

Se intenta actualizar mediante PUT.

### Entonces

La respuesta es HTTP 409.

Código esperado:

VERSION_INMUTABLE

---

## AT-016 - No modificar APROBADA

### Dado

La versión está APROBADA.

### Cuando

Se intenta modificar.

### Entonces

HTTP 409.

Código:

VERSION_INMUTABLE

---

## AT-017 - No modificar ACTIVA

### Dado

La versión está ACTIVA.

### Cuando

Se intenta modificar.

### Entonces

HTTP 409.

Código:

VERSION_INMUTABLE

La configuración activa permanece intacta.

---

## AT-018 - No modificar INACTIVA

### Dado

La versión está INACTIVA.

### Cuando

Se intenta modificar.

### Entonces

HTTP 409.

Código:

VERSION_INMUTABLE

---

# Ciclo de vida

## AT-019 - BORRADOR a EN_REVISION

### Dado

Una versión BORRADOR tiene una definición válida.

### Cuando

Se llama:

POST /versiones/{version}/enviar-revision

### Entonces

El estado cambia a:

EN_REVISION

---

## AT-020 - BORRADOR inválido no pasa a revisión

### Dado

La versión contiene una definición inválida.

### Cuando

Se intenta enviar a revisión.

### Entonces

La transición es rechazada.

La versión continúa:

BORRADOR

---

## AT-021 - EN_REVISION a APROBADA

### Dado

La versión está:

EN_REVISION

### Cuando

Se aprueba.

### Entonces

Estado:

APROBADA

Se registran:

fechaAprobacion
usuarioAprobacion

---

## AT-022 - APROBADA a ACTIVA

### Dado

La versión:

- está APROBADA;
- tiene definición válida;
- tiene vigencia válida.

### Cuando

Se activa.

### Entonces

La versión pasa a:

ACTIVA

---

## AT-023 - ACTIVA a INACTIVA

### Dado

La versión está ACTIVA.

### Cuando

Se ejecuta la operación de inactivación.

### Entonces

La versión pasa a:

INACTIVA

y deja de participar en las evaluaciones de Feature 001.

---

## AT-024 - Rechazar transición inválida

### Dado

Una versión está BORRADOR.

### Cuando

Se intenta activar directamente.

### Entonces

La operación retorna HTTP 409.

Código:

TRANSICION_ESTADO_INVALIDA

El estado continúa siendo:

BORRADOR

---

# Vigencias

## AT-025 - Varias ACTIVA sin solapamiento

### Dado

Existe:

v1 ACTIVA
2026-01-01 -> 2026-12-31

### Cuando

Se activa:

v2
2027-01-01 -> NULL

### Entonces

La operación es válida.

Pueden coexistir:

v1 ACTIVA
v2 ACTIVA

porque sus vigencias no se superponen.

---

## AT-026 - Una sola versión aplicable por fecha

### Dado

Existen:

v1 ACTIVA
2026-01-01 -> 2026-12-31

v2 ACTIVA
2027-01-01 -> NULL

### Cuando

Feature 001 evalúa:

fechaEvaluacion = 2026-06-01

### Entonces

Solo v1 es aplicable.

### Cuando

fechaEvaluacion = 2027-06-01

### Entonces

Solo v2 es aplicable.

---

## AT-027 - Reemplazo de versión ACTIVA

### Dado

Existe una versión ACTIVA cuya vigencia es reemplazada por una nueva versión
APROBADA.

### Cuando

Se activa la nueva versión mediante la operación de reemplazo definida.

### Entonces

Dentro de una única transacción:

versión anterior:
ACTIVA -> INACTIVA

nueva versión:
APROBADA -> ACTIVA

No existe un estado intermedio persistido donde ambas versiones sean
aplicables para la misma fecha.

---

## AT-028 - Rollback ante fallo de activación

### Dado

Se intenta reemplazar una versión ACTIVA.

### Y

ocurre un error antes de completar la transacción.

### Entonces

Se realiza rollback.

La versión anterior permanece ACTIVA.

La nueva versión permanece APROBADA.

---

## AT-029 - Vigencia inválida

### Cuando

fechaFinVigencia < fechaInicioVigencia

### Entonces

La operación es rechazada con:

VIGENCIA_INVALIDA

---

# Catálogos

## AT-030 - Consultar negocios

### Cuando

Se consulta:

GET /api/v1/catalogos/negocios

### Entonces

La respuesta se construye desde:

ref.negocio

No utiliza una lista hardcodeada.

---

## AT-031 - Consultar planes por negocio

### Cuando

Se consulta:

GET /api/v1/catalogos/tipos-plan?negocioId=CAJA

### Entonces

Se devuelven únicamente planes pertenecientes a CAJA según:

mae.tipo_plan
→ mae.tipo_contrato
→ mae.tipo_vinculacion
→ ref.negocio

---

## AT-032 - Consultar campos

### Cuando

Se consulta el catálogo para:

CAJA / VINCULACION / TP-01

### Entonces

Se devuelven los campos aplicables.

Para cada campo se informa:

- codigo;
- nombre funcional cuando exista;
- datoRequeridoId;
- tipoValor;
- operadores.

---

## AT-033 - Mesada se expone correctamente

### Entonces

El catálogo contiene:

codigo = pension.mesada
datoRequeridoId = DR-002
tipoValor = NUMERO

El tipo técnico no debe reemplazarse por TEXTO aunque DR-002 esté definido
como TEXTO en el Core.

---

## AT-034 - Consultar parámetros

### Cuando

Se consulta el catálogo de parámetros para 2026.

### Entonces

Puede devolverse:

SMMLV
valor = 1750000
vigencia = 2026

La API no permite modificar el parámetro.

---

# Seguridad

## AT-035 - Rechazar expresiones C#

### Cuando

Un consumidor intenta enviar una expresión C# en lugar de RuleDefinition.

### Entonces

La solicitud es rechazada.

No llega a Microsoft RulesEngine.

---

## AT-036 - Rechazar Dynamic LINQ arbitrario

### Cuando

Se intenta almacenar sintaxis Dynamic LINQ.

### Entonces

La definición es rechazada.

---

## AT-037 - Rechazar propiedades desconocidas

### Cuando

RuleDefinition contiene propiedades no definidas en el esquema.

### Entonces

La definición es rechazada.

---

## AT-038 - No exponer Microsoft RulesEngine

### Entonces

Ningún request ni response administrativo expone:

- Workflow;
- Rule;
- Expression;
- tipos internos de Microsoft RulesEngine.

---

# Integración Feature 001 / Feature 002

## AT-039 - BORRADOR no afecta evaluación

### Dado

RV-002 tiene:

v1 ACTIVA
v2 BORRADOR

### Cuando

Feature 001 evalúa RV-002.

### Entonces

Solo utiliza v1.

---

## AT-040 - APROBADA no afecta evaluación

### Dado

La versión nueva está APROBADA pero todavía no está ACTIVA.

### Cuando

Feature 001 evalúa.

### Entonces

La versión APROBADA no participa.

---

## AT-041 - Nueva configuración sin despliegue

### Dado

Se crea, revisa, aprueba y activa una nueva versión válida.

### Cuando

Se realiza posteriormente una evaluación mediante Feature 001.

### Entonces

RulesValidator utiliza la versión aplicable según fechaEvaluacion.

No se requiere:

- recompilar;
- modificar código;
- desplegar nuevamente el microservicio.

Este escenario constituye un criterio principal de aceptación de la POC.

---

# Concurrencia

## AT-042 - Creación concurrente de la misma regla

### Cuando

Dos operaciones intentan crear simultáneamente el mismo codigo en el mismo
flujo.

### Entonces

Solo una operación puede persistirse.

La otra retorna un conflicto controlado.

No quedan registros duplicados.

---

## AT-043 - Creación concurrente de versión

### Cuando

Dos solicitudes intentan crear simultáneamente la siguiente versión de una
regla.

### Entonces

No pueden persistirse dos filas con el mismo:

regla_id + version

El conflicto se reporta de forma controlada.

---

## AT-044 - Activación concurrente

### Cuando

Dos versiones incompatibles intentan activarse simultáneamente.

### Entonces

No puede quedar una configuración donde ambas sean aplicables a la misma
fechaEvaluacion.

---

# Auditoría y observabilidad

## AT-045 - Auditoría de creación

### Entonces

La creación registra:

- fecha_creacion;
- usuario_creacion.

---

## AT-046 - Auditoría de aprobación

### Entonces

La aprobación registra:

- fecha_aprobacion;
- usuario_aprobacion.

---

## AT-047 - Correlation ID

### Dado

El consumidor envía:

X-Correlation-ID

### Entonces

La operación administrativa conserva el mismo identificador en la
trazabilidad.

### Dado

No lo envía.

### Entonces

RulesValidator genera uno.

---

## AT-048 - No registrar definición completa innecesariamente

### Entonces

Los logs administrativos no incluyen `definicion_json` completa.

La telemetría registra solamente metadata necesaria para diagnóstico.

---

# Modelo y persistencia

## AT-049 - No modificar maestros Core

Feature 002 no realiza INSERT, UPDATE ni DELETE sobre:

- ref.negocio;
- mae.tipo_plan;
- mae.tipo_contrato;
- mae.tipo_vinculacion;
- cfg.dato_requerido;
- cfg.parametro.

---

## AT-050 - Sin nuevas tablas

La primera implementación de Feature 002 utiliza el modelo existente.

No requiere una migración nueva mientras no aparezca una necesidad funcional
adicional aprobada.

---

## AT-051 - Escritura sin usuario o con usuario inválido (POC)

### Dado

La solicitud de escritura (crear regla, crear versión, actualizar BORRADOR, enviar a revisión, aprobar, activar o inactivar) no incluye `usuario`, lo envía vacío, solo con espacios o con más de 100 caracteres.

### Cuando

Se ejecuta la operación.

### Entonces

La respuesta es HTTP 400 con código:

SOLICITUD_INVALIDA

La operación se detiene antes de acceder al repositorio. No se persiste ningún registro ni se usa un usuario de respaldo.

---

## AT-052 - El campo usuario no otorga permisos (POC)

### Dado

Una solicitud de escritura incluye `usuario` con un valor cualquiera (por ejemplo, un usuario sin permisos administrativos).

### Cuando

Se ejecuta la operación.

### Entonces

La operación se procesa según las reglas de negocio (no se devuelve 401 ni 403). El valor de `usuario` se registra solo como dato de auditoría.

No se modifica la lógica de autorización: `usuario` no se usa para decisiones de seguridad. Los roles concretos y la autorización por operación quedan pendientes de integración corporativa (T069).

---

## AT-053 - Usuario persistido en auditoría (POC)

### Dado

Se envía una solicitud válida con `usuario = "USR-011"` (ejemplo).

### Cuando

Se crea una regla, se crea una versión o se aprueba una versión.

### Entonces

- `reglas.regla.usuario_creacion` = `USR-011` al crear la regla.
- `reglas.version_regla.usuario_creacion` = `USR-011` al crear la versión.
- `reglas.version_regla.usuario_aprobacion` = `USR-011` y `fecha_aprobacion` (UTC) al aprobar.

En enviar a revisión, activar, inactivar y actualizar BORRADOR, `usuario` se valida pero no se persiste.

---

# Gate de aceptación Feature 002

Feature 002 puede considerarse funcionalmente aceptada cuando:

- creación de reglas funciona;
- versión 1 queda en BORRADOR;
- nueva versión funciona;
- solamente BORRADOR puede editarse;
- RuleDefinition se valida con el mecanismo de Feature 001;
- las transiciones de estado se respetan;
- la aprobación queda auditada;
- las vigencias activas no se superponen;
- el reemplazo de versión es transaccional;
- los catálogos provienen de las fuentes reales;
- no se pueden introducir expresiones ejecutables;
- Feature 001 utiliza una nueva versión activa sin recompilar ni desplegar;
- no se modifican maestros Core;
- no se requieren tablas nuevas para el alcance inicial;
- las escrituras administrativas exigen `usuario` válido en el body (AT-051, 400) y el usuario se persiste en auditoría (AT-053). `usuario` no se usa para autorización (AT-052). Identidad autenticada y autorización por rol quedan diferidas a la integración corporativa (T131, T069).