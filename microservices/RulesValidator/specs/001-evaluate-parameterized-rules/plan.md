# Plan técnico - Feature 001

## 1. Objetivo técnico

Implementar un servicio de evaluación de reglas parametrizables que seleccione
reglas según negocio, proceso, tipo de plan y etapa, utilizando un modelo
neutral propio y Microsoft RulesEngine encapsulado en Infrastructure.

La Feature 001 contempla:

- evaluación parametrizable;
- catálogo dinámico de campos y operadores;
- versionamiento y vigencia;
- parámetros provenientes del Core;
- consolidación de resultados;
- auditoría;
- integración con fuentes externas para Etapa 5.

---

## 2. Stack

- ASP.NET Core Web API
- .NET LTS aprobado
- Entity Framework Core
- Azure SQL
- Microsoft RulesEngine
- OpenAPI 3.0
- OpenTelemetry
- Azure Blob Storage
- Azure Identity
- xUnit

---

## 3. Proyectos

```text
RulesValidator.Api
RulesValidator.Application
RulesValidator.Domain
RulesValidator.Infrastructure
```

Dependencias:

```text
Api
 ↓
Application
 ↓
Domain

Infrastructure
 ↓
Application
 ↓
Domain
```

Application y Domain no dependen del SDK de Azure Blob Storage ni de
Microsoft RulesEngine.

---

## 4. Componentes existentes

### Domain / Application

Se mantienen:

- `Regla`
- `VersionRegla`
- `FlujoReglas`
- `DefinicionRegla`
- `ResultadoEvaluacionRegla`
- `EvaluarReglasCommand`
- `EvaluarReglasHandler`

Application define:

- `IRepositorioReglas`
- `IRepositorioEjecucionesReglas`
- `IRepositorioCatalogoCampos`
- `IRepositorioParametros`
- `IMotorReglas`
- `IValidadorDefinicionRegla`
- `IReloj`

No se crea `IRuleEvaluationService`.

`EvaluarReglasHandler` continúa siendo el orquestador del caso de uso.

### Infrastructure

Se mantienen:

- `RepositorioSqlReglas`
- `RepositorioSqlEjecucionesReglas`
- `RepositorioSqlCatalogoCampos`
- `RepositorioSqlParametros`
- `MicrosoftRuleEngineAdapter`
- `ExpressionBuilder`
- validación técnica de JSON Schema;
- mapeos EF Core de entidades Core mediante `ExcludeFromMigrations()`.

---

# 5. Estado actual implementado

## Etapa 4

La Etapa 4 se encuentra implementada y validada funcionalmente.

Reglas configuradas:

```text
RV-001
RV-002
RV-003
RV-101
RV-102
RV-103
```

Se validaron correctamente:

```text
CAJA + VINCULACION + TP-01 + Etapa 4
CAJA + VINCULACION + TP-02 + Etapa 4
```

### Caso A

```text
TP-01
Vejez
Mesada 2.100.000
Femenino
66 años
CC
```

Resultado:

```text
CUMPLE
```

### Caso B

```text
TP-01
Vejez
Mesada 1.900.000
Femenino
55 años
CC
```

Resultado:

```text
RV-102 = NO_CUMPLE
resultadoGeneral = NO_CUMPLE
```

### Caso C - Etapa 4

```text
TP-02
Vejez
Mesada 4.200.000
Masculino
68 años
CC
```

Resultado:

```text
CUMPLE
```

---

# 6. Integración actual con el Core

RulesValidator utiliza directamente los maestros existentes:

```text
ref.negocio
ref.tipo_dato

mae.tipo_vinculacion
mae.tipo_contrato
mae.tipo_plan

cfg.dato_requerido
cfg.parametro
```

Estas entidades se mapean en EF Core con:

```text
ExcludeFromMigrations()
```

RulesValidator no crea ni modifica estas tablas.

Mappings actualmente utilizados:

```text
DR-001 → pension.tipo
DR-002 → pension.mesada
DR-017 → persona.tipoDocumento
```

`reglas.campo_regla.tipo_valor` define el tipo utilizado durante la evaluación
y no se deriva obligatoriamente de `cfg.dato_requerido.tipo_dato_id`.

---

# 7. Parámetros Core

Está implementada la resolución de parámetros mediante:

```text
cfg.parametro
```

Actualmente se utiliza:

```text
parametro_id = SMMLV
valor = 1750000
vigencia = 2026
```

RuleDefinition soporta:

```json
{
  "tipo": "PARAMETRO_MULTIPLICADO",
  "parametro": "SMMLV",
  "factor": 1.5
}
```

El flujo implementado es:

```text
RuleDefinition
      ↓
validación
      ↓
ResolverParametrosRegla
      ↓
RepositorioSqlParametros
      ↓
cfg.parametro
      ↓
valor decimal resuelto
      ↓
ExpressionBuilder
      ↓
Microsoft RulesEngine
```

Ejemplos:

```text
RV-002 → mesada <= 1.5 × SMMLV
RV-003 → mesada >  1.5 × SMMLV
RV-101 → mesada >= 1.0 × SMMLV
```

---

# 8. Flujo de evaluación actual

1. API valida el request.
2. Middleware obtiene o genera `idCorrelacion`.
3. Handler genera `idEvaluacion`.
4. Handler determina `fechaEvaluacion` mediante request o `IReloj`.
5. Se valida negocio y tipo de plan contra los maestros Core.
6. Se recuperan los flujos aplicables.
7. Se recupera el catálogo activo de campos y operadores.
8. Se seleccionan reglas activas.
9. Se seleccionan versiones activas y vigentes.
10. Se deserializa `definicion_json`.
11. Se valida JSON Schema.
12. Se valida allow-list de campos y operadores.
13. Se validan tipos y complejidad.
14. Se resuelven parámetros Core cuando corresponda.
15. Se detectan datos ausentes.
16. `ExpressionBuilder` genera una expresión segura.
17. `MicrosoftRuleEngineAdapter` ejecuta la regla.
18. Se normalizan resultados.
19. El Handler consolida `resultadoGeneral`.
20. Se persiste `reglas.ejecucion_regla`.
21. Se emiten trazas y métricas.
22. API responde.

---

# 9. Selección GLOBAL

Existen dos alcances.

## Global

```text
negocio_id = NULL
```

Los flujos globales se limitan por proceso según la política definida.

## Específico

```text
negocio_id = negocioSolicitado
```

Se filtran por:

```text
proceso
etapa
tipo_plan_id cuando corresponda
```

Para `VINCULACION`, `tipo_plan_id` participa en la selección.

Las reglas globales se procesan antes de las específicas.

Dentro de cada alcance se respeta:

```text
prioridad del flujo
prioridad de la regla
```

---

# 10. Consolidación

La precedencia del resultado es:

```text
1. NO_CUMPLE BLOQUEANTE
2. INDETERMINADO
3. CUMPLE
```

`NO_APLICA` no afecta la consolidación.

Una regla `ADVERTENCIA` o `INFORMATIVA` con `NO_CUMPLE` no cambia por sí sola
`resultadoGeneral` a `NO_CUMPLE`.

---

# 11. Seguridad de RuleDefinition

La BD no contiene expresiones ejecutables.

`IValidadorDefinicionRegla` valida:

- versión de esquema;
- estructura JSON;
- campos autorizados;
- operadores autorizados;
- tipos;
- referencias de parámetros;
- profundidad máxima;
- número máximo de condiciones;
- número máximo de valores en `EN` / `NO_EN`;
- rechazo de propiedades no conocidas.

Límites actuales:

```text
profundidad máxima = 5
máximo de condiciones = 50
máximo de valores EN/NO_EN = 50
```

`ExpressionBuilder` genera únicamente tokens permitidos.

`MicrosoftRuleEngineAdapter` no expone helpers de:

- Reflection;
- IO;
- procesos;
- red;
- variables de entorno.

---

# 12. Datos ausentes

Cuando una regla requiere un campo que no se encuentra en el contexto:

```text
resultado = INDETERMINADO
codigoError = DATOS_INSUFICIENTES
```

No se propaga `NullReferenceException`.

No se convierte automáticamente en:

```text
NO_CUMPLE
```

---

# 13. Etapa 5 - RV-104

## Objetivo

Implementar la confirmación externa de la pensión mediante:

```text
RV-104
```

Fuente:

```text
FD-PENS
```

La fuente se encuentra catalogada en:

```text
ref.fuente_dato
```

El archivo físico estará almacenado en Azure Blob Storage.

Nombre lógico:

```text
fuente_externa_simulada
```

Formato inicial:

```text
NUMERO_DOCUMENTO;RESPUESTA
52310057;Reconocida
52310103;Reconocida
52310122;Reconocida
```

---

# 14. Diseño de integración FD-PENS

La integración debe respetar:

```text
Application
     ↓
abstracción de fuente externa

Infrastructure
     ↓
Azure Blob Storage
```

Application no depende de:

```text
Azure.Storage.Blobs
Azure.Identity
```

Se agregará una abstracción equivalente a:

```text
IFuentePensionados
```

La abstracción debe retornar un resultado explícito.

Estados:

```text
ENCONTRADO
SIN_RESPUESTA
NO_DISPONIBLE
INVALIDO
```

No utilizar únicamente `null` para representar todos los casos.

---

# 15. Implementación Azure Blob Storage

Infrastructure implementará el acceso a FD-PENS.

Dependencias:

```text
Azure.Storage.Blobs
Azure.Identity
```

Autenticación:

```text
DefaultAzureCredential
```

En Azure se utilizará Managed Identity.

No se almacenan:

- Access Keys en código;
- SAS tokens en código;
- credenciales dentro de RuleDefinition.

Configuración:

```text
FuentesExternas__Pensionados__FuenteId
FuentesExternas__Pensionados__ContainerName
FuentesExternas__Pensionados__BlobName
```

Nombre esperado:

```text
fuente_externa_simulada.csv
```

La extensión real debe provenir de configuración.

---

# 16. Resolución de la fuente

Para Etapa 5 el consumidor envía:

```text
persona.numeroDocumento
```

Flujo:

```text
Request
   ↓
persona.numeroDocumento
   ↓
Proveedor FD-PENS
   ↓
Azure Blob Storage
   ↓
fuente_externa_simulada
   ↓
buscar NUMERO_DOCUMENTO
   ↓
RESPUESTA
```

El número de documento se procesa como texto.

No se realizan conversiones numéricas que puedan eliminar ceros iniciales.

---

# 17. Enriquecimiento del contexto

Cuando FD-PENS retorna:

```text
RESPUESTA = Reconocida
```

Application crea un contexto enriquecido:

```text
fuente.pension.respuesta = Reconocida
```

El contexto original del consumidor no se modifica directamente.

El consumidor no puede proporcionar:

```text
fuente.pension.respuesta
```

como sustitución de la consulta externa.

---

# 18. Evaluación RV-104

RV-104 continúa siendo:

```text
PARAMETRIZABLE
```

RuleDefinition:

```json
{
  "versionEsquema": "1.0",
  "tipo": "CONDICION",
  "campo": "fuente.pension.respuesta",
  "operador": "IGUAL",
  "valor": "Reconocida"
}
```

Flujo:

```text
FD-PENS
   ↓
enriquecimiento
   ↓
RuleDefinition
   ↓
ExpressionBuilder
   ↓
Microsoft RulesEngine
```

La lógica de acceso a Azure no debe incorporarse dentro de:

```text
RuleDefinition
ExpressionBuilder
MicrosoftRuleEngineAdapter
```

---

# 19. Comportamiento ante fuente externa

## Encontrado

```text
RESPUESTA = Reconocida
```

Resultado:

```text
RV-104 = CUMPLE
```

## Sin respuesta

Documento no encontrado o respuesta vacía:

```text
RV-104 = INDETERMINADO
severidad = ADVERTENCIA
```

## Fuente no disponible

Azure Blob Storage no disponible o blob inaccesible:

```text
RV-104 = INDETERMINADO
severidad = ADVERTENCIA
```

## Fuente inválida

Archivo mal formado, encabezados inválidos o registros ambiguos:

```text
estado consulta = INVALIDO
```

No se evalúa utilizando información ambigua.

---

# 20. Responsabilidad después de RV-104

RulesValidator no:

- crea Caso TC-03;
- modifica `trx.paso_tramite`;
- actualiza la Etapa 5;
- activa vinculaciones;
- rechaza solicitudes.

El Core/orquestador interpreta:

```text
RV-104
INDETERMINADO
ADVERTENCIA
```

y aplica:

```text
Etapa 5 = EN ESPERA
Vinculación continúa
Caso TC-03
```

---

# 21. Observabilidad

Actualmente se registran:

- `idEvaluacion`;
- `idCorrelacion`;
- `idTraza`;
- negocio;
- proceso;
- etapa;
- flujos;
- cantidad de reglas;
- versiones;
- resultado;
- duración.

Métricas:

```text
rulesvalidator.evaluations.total
rulesvalidator.evaluation.duration
rulesvalidator.rules.evaluated
rulesvalidator.rules.non_compliant
rulesvalidator.rules.indeterminate
rulesvalidator.definition.rejected
```

Para fuentes externas se podrán agregar:

```text
rulesvalidator.external_source.requests
rulesvalidator.external_source.duration
rulesvalidator.external_source.unavailable
```

No utilizar:

```text
numeroDocumento
idEvaluacion
idCorrelacion
```

como dimensiones de métricas.

No registrar PII en logs.

---

# 22. Errores

HTTP:

```text
400 → request inválido
404 → configuración aplicable inexistente
422 → contexto semánticamente inválido
500 → error técnico o definición ACTIVA inválida
```

Todos utilizan:

```text
application/problem+json
```

e incluyen:

```text
idTraza
idCorrelacion
```

Una fuente externa no disponible durante RV-104 no produce por sí sola HTTP 500:

```text
RV-104 = INDETERMINADO
```

si la situación corresponde a un escenario funcional esperado de indisponibilidad.

Errores técnicos no recuperables o configuración inválida sí se procesan mediante
el mecanismo estándar de errores técnicos.

---

# 23. Persistencia

La evaluación se registra en:

```text
reglas.ejecucion_regla
```

No se persiste:

- contexto completo;
- número de documento;
- contenido del blob;
- información sensible de la fuente;
- credenciales.

Las tablas:

```text
ref.fuente_dato
cfg.parametro
cfg.dato_requerido
```

continúan bajo ownership del Core.

---

# 24. Pruebas ya realizadas

Se completaron pruebas manuales de Etapa 4:

- TP-01 exitoso;
- TP-01 con incumplimiento RV-102;
- TP-02 exitoso;
- valores monetarios de mesada;
- resolución de SMMLV;
- condiciones simples;
- grupos `Y/O`;
- campos dinámicos;
- resultados `CUMPLE`;
- resultados `NO_CUMPLE`;
- resultados `INDETERMINADO`.

Estas tareas no deben reabrirse durante la implementación de Etapa 5 salvo que
se detecte una regresión directa.

---

# 25. Pruebas pendientes Etapa 5

Validar manualmente:

## Escenario 1

Documento existente:

```text
52310057
```

Fuente:

```text
Reconocida
```

Esperado:

```text
RV-104 = CUMPLE
resultadoGeneral = CUMPLE
```

## Escenario 2

Documento inexistente:

```text
SIN_RESPUESTA
```

Esperado:

```text
RV-104 = INDETERMINADO
severidad = ADVERTENCIA
resultadoGeneral = INDETERMINADO
```

## Escenario 3

Blob no disponible:

```text
RV-104 = INDETERMINADO
severidad = ADVERTENCIA
```

## Escenario 4

Archivo inválido:

```text
consulta = INVALIDO
```

Debe producir tratamiento controlado sin utilizar datos ambiguos.

---

# 26. Restricciones de implementación

Durante Etapa 5:

- no modificar Etapa 4;
- no modificar RV-001 a RV-103;
- generar únicamente la migración de `reglas.fuente_externa_mapeo`;
- no agregar otras tablas al esquema `reglas`;
- no modificar tablas del Core;
- no asociar DR-001 con FD-PENS;
- no hardcodear `fuente_externa_simulada` en lógica de dominio;
- no introducir Azure SDK en Application o Domain;
- no crear el Caso TC-03 desde RulesValidator;
- no almacenar PII en auditoría;
- no ejecutar ni modificar proyectos de pruebas durante la implementación;
- sí se permite compilar proyectos productivos para validar consistencia.