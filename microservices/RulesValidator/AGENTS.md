# Instrucciones para agentes de desarrollo - RulesValidator

## Contexto obligatorio

Antes de modificar código, revisar en este orden:

1. `../../AGENTS.md`
2. `../../docs/global/ARCHITECTURE.md`
3. `../../docs/global/CODING_STANDARDS.md`
4. `../../docs/standards/microservice-template.md`
5. `constitution.md`
6. `RuleValidator_Context.md`
7. La especificación activa en `/specs`
8. `plan.md`
9. `data-model.md`
10. `tasks.md`
11. `rules-api.yaml`

Ante una diferencia entre documentación histórica y la especificación activa,
prevalece la definición más reciente aprobada para la Feature.

---

## Reglas de desarrollo

- Implementar únicamente el alcance definido en la especificación activa.
- No agregar funcionalidades que no estén definidas en la Spec.
- No reabrir decisiones arquitectónicas que ya estén cerradas salvo que exista una incompatibilidad demostrable.
- Utilizar Clean Architecture.
- Domain no puede depender de Application, Infrastructure ni Api.
- Application no puede depender directamente de Azure SQL.
- Application y Domain no pueden depender directamente de Microsoft RulesEngine.
- Application y Domain no pueden depender de SDKs de Azure Storage.
- Microsoft RulesEngine debe permanecer encapsulado mediante un Adapter en Infrastructure.
- No incluir lógica de negocio en endpoints o Controllers.
- No almacenar código ejecutable dentro de las reglas.
- No permitir expresiones C# arbitrarias provenientes de consumidores o configuración.
- No permitir Dynamic LINQ arbitrario proveniente de configuración.
- No construir SQL a partir de RuleDefinition.
- Las reglas activas publicadas son inmutables.
- Los errores HTTP deben utilizar ProblemDetails.
- No registrar información sensible completa en logs.
- Mantener los cambios limitados a los componentes necesarios para la tarea activa.
- No realizar refactorizaciones no relacionadas con la tarea.

---

## Estado actual de Feature 001

La Etapa 4 está implementada y validada manualmente.

No modificar la implementación existente de las siguientes reglas salvo que la
tarea activa lo requiera explícitamente:

```text
RV-001
RV-002
RV-003
RV-101
RV-102
RV-103
```

Se encuentran validados:

```text
CAJA + VINCULACION + TP-01 + Etapa 4
CAJA + VINCULACION + TP-02 + Etapa 4
```

También están implementados:

- selección de flujos;
- catálogo dinámico de campos;
- catálogo dinámico de operadores;
- RuleDefinition 1.0;
- grupos anidados `Y` / `O`;
- consolidación de resultados;
- persistencia de ejecuciones;
- resolución de parámetros Core;
- `PARAMETRO_MULTIPLICADO`;
- resolución de `SMMLV` desde `cfg.parametro`;
- mesada recibida en valor monetario mediante `pension.mesada`.

No sustituir:

```text
pension.mesada
```

por:

```text
pension.mesadaEnSmlmv
```

El consumidor envía la mesada en pesos.

---

## Persistencia

Utilizar Azure SQL.

RulesValidator es propietario únicamente de:

```text
reglas.campo_regla
reglas.campo_regla_operador
reglas.flujo_reglas
reglas.regla
reglas.version_regla
reglas.ejecucion_regla
reglas.fuente_externa_mapeo
```

No crear tablas duplicadas para:

```text
negocio
tipo_vinculacion
tipo_contrato
tipo_plan
dato_requerido
parametro
fuente_dato
```

Estos conceptos pertenecen al Core.

Las entidades Core utilizadas por RulesValidator deben mapearse mediante EF Core
con:

```text
ExcludeFromMigrations()
```

No generar migraciones que creen, modifiquen o eliminen tablas de los esquemas
Core.

---

## Datos requeridos y campos del motor

`cfg.dato_requerido` es propiedad del Core.

`reglas.campo_regla` es la capa de mapping y allow-list de RulesValidator.

Mappings confirmados:

```text
DR-001 → pension.tipo
DR-002 → pension.mesada
DR-017 → persona.tipoDocumento
```

No modificar `cfg.dato_requerido` para adaptarlo a las necesidades técnicas del
motor.

El tipo funcional del Core y el tipo utilizado por RulesValidator son conceptos
independientes.

Ejemplo:

```text
cfg.dato_requerido
DR-002
tipo_dato_id = TEXTO

reglas.campo_regla
codigo = pension.mesada
tipo_valor = NUMERO
```

`reglas.campo_regla.tipo_valor` define cómo RulesValidator interpreta el valor
durante la evaluación.

No sobrescribir automáticamente `tipo_valor` con
`cfg.dato_requerido.tipo_dato_id`.

---

## Parámetros Core

`cfg.parametro` pertenece al Core.

RulesValidator únicamente consulta la configuración.

Actualmente se utiliza:

```text
SMMLV
```

RuleDefinition soporta referencias cerradas:

```json
{
  "tipo": "PARAMETRO_MULTIPLICADO",
  "parametro": "SMMLV",
  "factor": 1.5
}
```

La resolución debe ejecutarse:

```text
RuleDefinition
    ↓
validación
    ↓
resolución del parámetro
    ↓
valor numérico
    ↓
ExpressionBuilder
    ↓
Microsoft RulesEngine
```

No hardcodear valores de SMMLV.

No permitir que una referencia de parámetro contenga expresiones ejecutables.

---

## Motor de reglas

Las reglas parametrizables se evalúan mediante Microsoft RulesEngine.

Microsoft RulesEngine recibe únicamente expresiones construidas internamente
por RulesValidator.

`ExpressionBuilder` debe generar exclusivamente tokens autorizados.

No entregar directamente al motor:

- expresiones configuradas externamente;
- código C#;
- funciones arbitrarias;
- acceso a Reflection;
- acceso a IO;
- acceso a red;
- variables de entorno.

Una integración externa no convierte automáticamente una regla en
`ESPECIALIZADA`.

Una regla puede continuar siendo `PARAMETRIZABLE` cuando Infrastructure obtiene
previamente un dato externo y Application enriquece el contexto antes de
evaluar RuleDefinition.

---

## Reglas especializadas

Utilizar `ESPECIALIZADA` únicamente cuando la lógica de evaluación no pueda
representarse adecuadamente mediante RuleDefinition y el modelo parametrizable
aprobado.

No utilizar `ESPECIALIZADA` únicamente porque:

- el dato provenga de Azure;
- exista una consulta externa;
- el dato se obtenga de un archivo;
- se requiera acceder a otro sistema.

Primero evaluar si la dependencia externa puede resolverse antes de la
evaluación y exponer su resultado como un campo autorizado del contexto.

---

## Etapa 5 y RV-104

RV-104 forma parte de la Etapa 5.

RV-104 continúa siendo:

```text
PARAMETRIZABLE
```

La fuente utilizada es:

```text
FD-PENS
```

definida por el Core en:

```text
ref.fuente_dato
```

La fuente física se encuentra en Azure Blob Storage.

Nombre lógico:

```text
fuente_externa_simulada
```

El consumidor proporciona:

```text
persona.numeroDocumento
```

El consumidor no proporciona:

```text
fuente.pension.respuesta
```

El flujo correcto es:

```text
persona.numeroDocumento
    ↓
proveedor FD-PENS
    ↓
Azure Blob Storage
    ↓
fuente_externa_simulada
    ↓
RESPUESTA
    ↓
contexto interno enriquecido
    ↓
fuente.pension.respuesta
    ↓
RV-104
```

RV-104 evalúa:

```text
fuente.pension.respuesta IGUAL "Reconocida"
```

No asociar FD-PENS con DR-001.

DR-001 continúa representando:

```text
TIPO_DE_PENSION_RECONOCIDA
```

y es utilizado en Etapa 4 por RV-001 y RV-102.

---

## Fuentes externas

El SDK de Azure debe existir únicamente en Infrastructure.

Reparto de configuración de la fuente externa:

```text
Configuración de infraestructura (appsettings / variables de entorno / Key Vault):
  FuenteId, BlobServiceUri, ContainerName

reglas.fuente_externa_mapeo (propiedad de RulesValidator):
  nombre del blob, separador, columnas de clave y valor,
  campo de clave del contexto, campo de respuesta (FK a reglas.campo_regla)
```

La ubicación física (URI y contenedor) nunca proviene de la base de datos.

Application debe depender de una abstracción y no de:

```text
Azure.Storage.Blobs
Azure.Identity
BlobClient
BlobServiceClient
```

La consulta de FD-PENS debe devolver un resultado explícito capaz de distinguir:

```text
ENCONTRADO
SIN_RESPUESTA
NO_DISPONIBLE
INVALIDO
```

No utilizar un único `null` para representar todos los escenarios.

Para FD-PENS:

- obtener ubicación desde configuración;
- no hardcodear URI;
- no hardcodear container;
- no hardcodear el nombre físico del blob en lógica de dominio;
- no incluir credenciales en RuleDefinition;
- no incluir SAS tokens en RuleDefinition;
- no permitir que RuleDefinition seleccione URI;
- utilizar `DefaultAzureCredential`;
- soportar Managed Identity en Azure;
- propagar `CancellationToken`.

Si la fuente utiliza el formato delimitado de la POC:

```text
NUMERO_DOCUMENTO;RESPUESTA
```

tratar `NUMERO_DOCUMENTO` como texto.

No convertirlo a tipos numéricos.

Preservar ceros iniciales.

---

## Contexto enriquecido

El contexto enviado por el consumidor no debe modificarse directamente de forma
insegura.

Cuando una fuente externa produce un dato válido, crear una copia o
representación enriquecida para la evaluación.

Ejemplo:

```text
fuente.pension.respuesta = Reconocida
```

Los campos enriquecidos deben estar autorizados en `reglas.campo_regla`.

Para RV-104:

```text
campo = fuente.pension.respuesta
tipo = TEXTO
operador = IGUAL
```

---

## Resultados de fuente externa

Una fuente externa indisponible no equivale a una regla incumplida.

Para RV-104:

```text
Reconocida
→ CUMPLE
```

Documento no encontrado o sin respuesta:

```text
→ INDETERMINADO
```

Fuente no disponible:

```text
→ INDETERMINADO
```

RV-104 utiliza severidad:

```text
ADVERTENCIA
```

No convertir indisponibilidad de fuente en:

```text
NO_CUMPLE
```

RulesValidator no debe crear:

```text
TC-03
```

ni modificar:

```text
trx.paso_tramite
Etapa 5
Vinculación
Solicitud
```

Estas acciones pertenecen al Core/orquestador.

---

## Tipos de validación

El sistema distingue:

1. reglas parametrizables;
2. reglas especializadas cuando realmente sean necesarias;
3. restricciones de integridad.

No convertir restricciones de integridad de base de datos en reglas
parametrizables.

No convertir integraciones externas automáticamente en reglas especializadas.

---

## Seguridad y privacidad

No registrar:

- número de documento;
- contexto completo;
- contenido completo de fuentes externas;
- credenciales;
- tokens;
- cadenas de conexión;
- SAS tokens.

`persona.numeroDocumento` se considera dato sensible para efectos de logging y
auditoría de RulesValidator.

No persistirlo en:

```text
reglas.ejecucion_regla
```

No almacenar copias permanentes de:

```text
fuente_externa_simulada
```

desde RulesValidator.

---

## Observabilidad

Mantener:

- idEvaluacion;
- idCorrelacion;
- idTraza;
- negocio;
- proceso;
- tipo de plan;
- etapa;
- cantidad de reglas;
- versiones;
- resultado;
- duración.

Las métricas de fuentes externas pueden utilizar:

```text
fuenteId
estadoConsulta
```

como dimensiones de baja cardinalidad.

No utilizar como dimensiones:

- número de documento;
- idEvaluacion;
- idCorrelacion;
- URI completo del blob.

---

## Convenciones

- Seguir `../../docs/global/CODING_STANDARDS.md`.
- Mantener nombres de negocio en español.
- Documentación funcional en español.
- Mensajes funcionales en español.
- Conservar identificadores oficiales como `RV-001`.
- Conservar aliases aprobados.
- No renombrar contratos o aliases existentes por preferencia personal.
- No mezclar español e inglés arbitrariamente en conceptos de dominio.

---

## Política de migraciones

No generar migraciones salvo que la tarea activa requiera explícitamente un
cambio estructural del modelo propiedad de RulesValidator.

La implementación de Etapa 5 / RV-104:

```text
Requiere únicamente la tabla aprobada `reglas.fuente_externa_mapeo`,
propiedad de RulesValidator, con su migración.
NO modifica tablas existentes ni tablas Core.
```

Nunca generar migraciones para alterar tablas Core.

---

## Política de ejecución de pruebas

Durante la implementación incremental, el agente NO debe ejecutar
automáticamente proyectos de pruebas.

No ejecutar de forma automática:

```text
dotnet test
tests unitarios
tests de integración
Architecture Tests
Contract Tests
Security Tests
cobertura de código
suites completas de regresión
```

Los proyectos de pruebas se mantienen en la solución, pero durante el alcance
actual no deben modificarse ni ejecutarse salvo instrucción explícita.

Durante tareas de implementación se permite ejecutar únicamente:

```text
dotnet restore
dotnet build
```

y solo cuando sea necesario validar compilación.

Las pruebas se ejecutarán únicamente cuando:

1. una tarea activa solicite explícitamente ejecutar pruebas;
2. el usuario solicite explícitamente ejecutar pruebas;
3. se alcance el gate final de validación de la Feature.

El agente no debe ejecutar pruebas como paso adicional para:

```text
verificar
asegurar
validar
confirmar
```

una implementación cuando la tarea activa no lo exige.

---

## Cierre de cada tarea

Al terminar cada tarea:

1. indicar archivos modificados;
2. resumir los cambios realizados;
3. indicar si `dotnet build` fue ejecutado;
4. indicar el resultado de compilación;
5. indicar qué pruebas corresponderían a la tarea;
6. no ejecutar dichas pruebas;
7. indicar si se generaron migraciones;
8. indicar cualquier supuesto pendiente;
9. detenerse.

No continuar automáticamente con la siguiente tarea de `tasks.md`.