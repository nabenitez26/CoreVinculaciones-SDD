# Plan técnico - Feature 002

## 1. Objetivo

Implementar capacidades administrativas sobre el modelo de reglas existente sin
modificar el motor de evaluación de Feature 001.

---

## 2. Stack

Se reutiliza:

- ASP.NET Core
- .NET
- Entity Framework Core
- Azure SQL
- OpenAPI
- OpenTelemetry

Feature 002 no requiere nuevas dependencias técnicas inicialmente.

---

## 3. Arquitectura

Se mantienen:

```text
RulesValidator.Api
RulesValidator.Application
RulesValidator.Domain
RulesValidator.Infrastructure
```

Feature 002 sigue Clean Architecture.

---

## 4. Reutilización de Feature 001

Reutilizar:

```text
RuleDefinition
IValidadorDefinicionRegla
IRepositorioCatalogoCampos
IRepositorioParametros
RulesDbContext
entidades persistentes
ProblemDetails
Correlation ID
OpenTelemetry
```

No duplicar:

- validador RuleDefinition;
- resolución de campos;
- resolución de operadores;
- resolución de parámetros;
- lógica de jerarquía Core cuando ya exista.

---

## 5. Casos de uso

Application incorporará casos de uso equivalentes a:

```text
ConsultarReglas
ConsultarRegla
ConsultarVersionesRegla

CrearRegla
CrearVersionRegla
ActualizarBorradorRegla

ValidarDefinicionRegla

EnviarVersionRevision
AprobarVersionRegla
ActivarVersionRegla
InactivarVersionRegla

ConsultarNegocios
ConsultarTiposPlan
ConsultarCamposRegla
ConsultarParametros
```

Los nombres de esta sección son conceptuales. En código se aplican los sufijos de `docs/global/CODING_STANDARDS.md` (Command, Query, Handler, Dto, Repository según corresponda).

---

## 6. Repositorio administrativo

Evaluar extender los repositorios existentes antes de crear nuevas
abstracciones.

La API administrativa necesita operaciones explícitas para:

```text
crear regla
crear versión
actualizar borrador
cambiar estado
consultar historial
```

No exponer `DbContext` directamente a Application.

---

## 7. Crear regla

Flujo:

```text
request
 ↓
validar alcance
 ↓
encontrar flujo existente
 ↓
validar código único
 ↓
validar definición
 ↓
transacción
 ↓
crear regla
 ↓
crear versión 1 BORRADOR
 ↓
response
```

No activar automáticamente.

---

## 8. Crear versión

Flujo:

```text
regla existente
 ↓
validar estado
 ↓
determinar siguiente versión
 ↓
copiar valores de base cuando corresponda
 ↓
aplicar cambios recibidos
 ↓
validar definición
 ↓
crear BORRADOR
```

La versión anterior permanece intacta.

---

## 9. Actualizar borrador

Solo:

```text
BORRADOR
```

puede modificarse.

La operación debe rechazar:

```text
EN_REVISION
APROBADA
ACTIVA
INACTIVA
```

con error:

```text
VERSION_INMUTABLE
```

---

## 10. Validación previa

El endpoint administrativo de validación debe construir el mismo objeto
`DefinicionRegla` que posteriormente se persistiría.

Flujo:

```text
DTO
 ↓
mapping
 ↓
DefinicionRegla
 ↓
IValidadorDefinicionRegla
 ↓
resultado
```

No invocar Microsoft RulesEngine para determinar si una definición es
estructuralmente válida salvo que Feature 001 ya requiera esa validación como
parte explícita del mecanismo existente.

---

## 11. Estados

Implementar transiciones mediante comandos explícitos.

Evitar endpoint genérico:

```http
PUT /estado
```

que permita cualquier transición.

Las acciones de dominio son:

```text
enviar a revisión
aprobar
activar
inactivar
```

---

## 12. Activación

La activación debe ejecutarse dentro de una transacción.

Validar:

```text
versión existente
estado = APROBADA
regla activa
definición válida
vigencia válida 

```

Las demás versiones no se modifican. La política (C005 y C006) está cerrada en `clarifications.md`.

---

## 13. Catálogo de campos

El endpoint consulta:

```text
reglas.campo_regla
+
cfg.dato_requerido
+
reglas.campo_regla_operador
```

Respuesta conceptual:

```json
{
  "codigo": "pension.mesada",
  "nombre": "Mesada pensional",
  "datoRequeridoId": "DR-002",
  "tipoValor": "NUMERO",
  "operadores": [
    "MAYOR",
    "MAYOR_IGUAL",
    "MENOR",
    "MENOR_IGUAL"
  ]
}
```

No exponer información innecesaria del Core.

---

## 14. Catálogo de planes

Consultar:

```text
mae.tipo_plan
    ↓
mae.tipo_contrato
    ↓
mae.tipo_vinculacion
    ↓
ref.negocio
```

Permitir filtro:

```text
negocioId
```

La API no mantiene una lista propia de planes.

---

## 15. Catálogo de parámetros

Consultar parámetros vigentes desde:

```text
cfg.parametro
```

Permitir una fecha de referencia cuando sea necesaria.

No modificar parámetros.

---

## 16. Seguridad

Nunca aceptar:

```text
expression
csharpExpression
dynamicLinq
sql
workflow
rulesEngineRule
```

La entrada utiliza únicamente RuleDefinition neutral.

No agregar endpoints genéricos para ejecutar JSON arbitrario.

---

## 17. Autorización

Feature 002 administra configuración sensible del motor.

La implementación debe integrarse con el mecanismo de autenticación y
autorización estándar de la plataforma.

Separar conceptualmente permisos para:

```text
consultar
editar borrador
enviar a revisión
aprobar
activar/inactivar
```

El modelo exacto de roles queda pendiente en `clarifications.md`.

No hardcodear usuarios individuales.

### Identidad en la POC (temporal)

No existe `IUsuarioActual` ni autenticación obligatoria en esta versión. Las escrituras reciben `usuario` en el body (obligatorio, trim, máximo 100; inválido → 400 `SOLICITUD_INVALIDA`). `usuario` es información declarada para auditoría, no identidad autenticada, y no se usa para autorización. Detalle en `spec.md` §25 y `clarifications.md` (decisión POC). En la integración productiva se reemplaza por Bearer Token → ClaimsPrincipal → `IUsuarioActual` (T104, T131).

---

## 18. Observabilidad

Registrar:

```text
idCorrelacion
operacion
codigoRegla
version
estadoOrigen
estadoDestino
resultado
duracion
```

No registrar:

```text
definicion_json completa
credenciales
PII
```

Métricas candidatas:

```text
rulesvalidator.admin.operations.total
rulesvalidator.admin.operation.duration
rulesvalidator.admin.validation.rejected
rulesvalidator.admin.state_transition.rejected
```

Evitar código de regla y usuario como dimensión de alta cardinalidad si la
plataforma de observabilidad no lo permite.

---

## 19. Errores

Usar ProblemDetails.

Códigos funcionales:

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
CONFLICTO_CONCURRENCIA
```

---

## 20. Migraciones

No generar migraciones inicialmente.

El modelo existente cubre el alcance inicial.

Si durante implementación aparece una necesidad estructural:

1. documentarla;
2. contrastarla contra `data-model.md`;
3. detenerse;
4. no generar automáticamente la migración.

---

## 21. Política de pruebas

Mantener la política definida en `AGENTS.md`.

Durante implementación incremental:

```text
dotnet restore
dotnet build
```

cuando corresponda.

No ejecutar automáticamente suites de pruebas salvo instrucción explícita.

---

## 22. Orden de implementación

```text
1. Contrato OpenAPI
2. DTOs
3. Consultas administrativas
4. Catálogos
5. Validación sin persistencia
6. Crear regla
7. Crear versión
8. Actualizar borrador
9. Enviar a revisión
10. Aprobar
11. Activar
12. Inactivar
13. Observabilidad
14. Compilación
15. Validación manual
```