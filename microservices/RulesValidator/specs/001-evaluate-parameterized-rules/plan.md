# Plan técnico - Feature 001

## 1. Stack

- ASP.NET Core Web API
- .NET LTS aprobado
- Entity Framework Core
- Azure SQL
- Microsoft RulesEngine encapsulado
- xUnit
- OpenAPI 3.0
- OpenTelemetry

## 2. Proyectos

- RulesValidator.Api
- RulesValidator.Application
- RulesValidator.Domain
- RulesValidator.Infrastructure

## 3. Componentes de dominio y aplicación

Nombres de negocio en español:

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
- `IMotorReglas`
- `IValidadorDefinicionRegla`
- `IReloj`

No se crea `IRuleEvaluationService`; `EvaluarReglasHandler` orquesta el caso de uso.

Infrastructure implementa:

- `RepositorioSqlReglas`
- `RepositorioSqlEjecucionesReglas`
- `MicrosoftRuleEngineAdapter`
- `ExpressionBuilder`
- validación técnica de JSON Schema cuando requiera librería externa.

## 4. Flujo

1. API valida request.
2. Middleware obtiene o genera `idCorrelacion`.
3. Handler genera `idEvaluacion` GUID.
4. Handler determina `fechaEvaluacion` UTC mediante request o `IReloj`.
5. Repository recupera flujos GLOBAL y del negocio solicitado.
6. Filtra flujo, regla y versión por estado y vigencia.
7. Deserializa `definicion_json` a `DefinicionRegla`.
8. Valida JSON Schema, campos, operadores, tipos y complejidad.
9. Adapter genera expresión segura.
10. Microsoft RulesEngine evalúa.
11. Adapter normaliza resultados.
12. Handler consolida resultadoGeneral.
13. Persiste `ejecucion_regla`.
14. Emite trazas y métricas.
15. API responde.

## 5. Selección GLOBAL

Se realizan dos consultas lógicas, que pueden resolverse en una sola consulta SQL:

- `negocio.codigo = GLOBAL`;
- `negocio.codigo = negocioSolicitado`.

Ambas deben coincidir en proceso, etapa y vigencia. Se ordenan por prioridad del flujo y prioridad de la regla.

## 6. Consolidación

- NO_CUMPLE BLOQUEANTE tiene mayor precedencia.
- Luego INDETERMINADO.
- Finalmente CUMPLE.
- NO_APLICA se ignora en la agregación.

## 7. Seguridad

La BD no almacena expresiones ejecutables.

`IValidadorDefinicionRegla` valida:

- esquema 1.0;
- campos autorizados;
- operadores autorizados;
- tipos de valores;
- profundidad máxima 5;
- máximo 50 condiciones por regla;
- máximo 50 valores para EN/NO_EN;
- rechazo de propiedades desconocidas.

`ExpressionBuilder` genera exclusivamente tokens permitidos.

`MicrosoftRuleEngineAdapter` configura explícitamente:

- `CustomTypes` restringidos;
- `AutoRegisterInputType` de acuerdo con la política aprobada;
- sin helpers para Reflection, IO, procesos, red o variables de entorno.

## 8. Datos ausentes

El Adapter o la capa Application detecta propiedades ausentes requeridas por una regla y produce `INDETERMINADO`, `DATOS_INSUFICIENTES` y mensaje funcional. No propaga NullReferenceException.

## 9. Observabilidad

Middleware y Application registran:

- idEvaluacion;
- idCorrelacion;
- idTraza;
- negocio;
- proceso;
- etapa;
- flujos;
- número de reglas;
- versiones;
- resultado;
- duración.

Métricas:

- rulesvalidator.evaluations.total
- rulesvalidator.evaluation.duration
- rulesvalidator.rules.evaluated
- rulesvalidator.rules.non_compliant
- rulesvalidator.rules.indeterminate
- rulesvalidator.definition.rejected

No se usan identificadores de alta cardinalidad como dimensiones de métricas.

## 10. Errores

- 400: request inválido.
- 404: no existe configuración aplicable.
- 422: request sintácticamente válido pero no procesable según contrato.
- 500: error técnico o definición ACTIVA inválida.

Todos usan `application/problem+json` e incluyen idTraza e idCorrelacion. La respuesta 500 no filtra detalles internos.

## 11. Pruebas

- unitarias funcionales y consolidación;
- integración SQL y vigencia;
- seguridad de definiciones;
- Adapter y ExpressionBuilder;
- contratos HTTP;
- persistencia;
- observabilidad;
- Architecture Tests.
