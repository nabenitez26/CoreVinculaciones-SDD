# Tareas - Feature 001

## Fundación y arquitectura

- [x] T001 Crear `RulesValidator.sln`.
- [x] T002 Crear proyectos Api, Application, Domain e Infrastructure.
- [x] T003 Configurar referencias de Clean Architecture.
- [x] T004 Crear proyectos Domain.Tests, Application.Tests, IntegrationTests y ArchitectureTests.
- [x] T005 Implementar Architecture Tests: Domain y Application no referencian Infrastructure, Api, EF Core ni Microsoft RulesEngine; únicamente Infrastructure referencia Microsoft RulesEngine.

## Dominio y Application

- [x] T006 Crear `Regla`, `VersionRegla`, `FlujoReglas`, `DefinicionRegla` y `ResultadoEvaluacionRegla`.
- [x] T007 Crear estados CUMPLE, NO_CUMPLE, NO_APLICA e INDETERMINADO.
- [x] T008 Crear severidades INFORMATIVA, ADVERTENCIA y BLOQUEANTE.
- [x] T009 Crear `IRepositorioReglas`, `IRepositorioEjecucionesReglas`, `IMotorReglas`, `IValidadorDefinicionRegla` e `IReloj`.
- [x] T010 Crear `EvaluarReglasCommand` y `EvaluarReglasHandler`; no crear un servicio de evaluación duplicado.
- [x] T011 Implementar precedencia de resultadoGeneral definida en spec.md.
- [x] T012 Generar `idEvaluacion` GUID e interpretar `fechaEvaluacion` UTC opcional.

## Modelo neutral y seguridad

- [x] T013 Incorporar y validar `rule-definition-1.0.schema.json`.
- [x] T014 Implementar catálogo allow-list de campos y operadores configurable desde Azure SQL.
- [x] T015 Validar tipos, propiedades desconocidas, profundidad máxima 5, máximo 50 condiciones y máximo 50 valores EN/NO_EN.
- [x] T016 Garantizar que definiciones rechazadas no lleguen al Adapter.

## Azure SQL

- [x] T017 Crear `RulesDbContext` y mappings de negocio, flujo_reglas, regla, version_regla, ejecucion_regla y catálogo de campos.
- [x] T018 Configurar PK, FK, UK, índices y CHECK de data-model.md, incluido el catálogo dinámico.
- [x] T019 Crear las migraciones inicial y del catálogo dinámico.
- [x] T020 Implementar `RepositorioSqlReglas`.
- [x] T021 Consultar reglas GLOBAL por proceso y reglas específicas por negocio, proceso, etapa y `tipo_plan_id`.
- [x] T022 Filtrar flujo/regla activos, versión ACTIVA y vigencia temporal inclusiva; usar fecha recibida desde Application.
- [x] T023 Implementar `RepositorioSqlEjecucionesReglas` y persistir ejecución después de consolidar.

## Adapter

- [x] T024 Instalar Microsoft RulesEngine solo en Infrastructure.
- [x] T025 Implementar `ExpressionBuilder` para operadores permitidos, incluyendo EN/NO_EN sin métodos arbitrarios.
- [x] T026 Implementar `MicrosoftRuleEngineAdapter` y mapeo de resultados neutral.
- [x] T027 Configurar explícitamente CustomTypes y AutoRegisterInputType; no exponer Reflection, IO, procesos, red ni entorno.
- [x] T028 Implementar manejo de datos ausentes como INDETERMINADO / DATOS_INSUFICIENTES.

## API y errores

- [x] T029 Crear `POST /api/v1/evaluaciones-reglas`.
- [x] T030 Crear DTOs alineados con rules-api.yaml, usando `mesadaEnSmlmv`.
- [x] T031 Implementar validación de request y ProblemDetails con `errors`.
- [x] T032 Implementar 400, 404, 422 y 500 controlado.

## Observabilidad

- [x] T033 Configurar OpenTelemetry.
- [x] T034 Implementar middleware X-Correlation-ID y generación si falta.
- [x] T035 Medir `duracionMs` y propagar idEvaluacion, idCorrelacion e idTraza.
- [x] T036 Crear métricas definidas en plan.md.
- [x] T037 Implementar logging estructurado y protección de PII/contexto.

## Pruebas

- [ ] T038 Implementar pruebas RV-001, incluidas dato ausente, severidad, código y mensaje.
- [ ] T039 Implementar pruebas RV-101, límites y dato ausente.
- [ ] T040 Implementar pruebas RV-002, NO_APLICA y datos ausentes.
- [ ] T041 Probar consolidación de resultadoGeneral.
- [ ] T042 Probar combinación GLOBAL + negocio específico.
- [ ] T043 Probar ACTIVA, INACTIVA, APROBADA, futura, vencida, vigencia abierta y límites inclusivos.
- [ ] T044 Probar campos/operadores/tipos no permitidos, propiedades extra, profundidad y cantidad.
- [ ] T045 Probar que definición rechazada no invoca Adapter.
- [ ] T046 Probar mappings de ExpressionBuilder y MicrosoftRuleEngineAdapter.
- [ ] T047 Probar contratos 400, 404, 422 y 500 sin filtración de información.
- [ ] T048 Probar Correlation ID recibido/generado, duración y ausencia de PII.
- [ ] T049 Probar persistencia de ejecucion_regla y resultados_json.
- [ ] T050 Ejecutar suite completa, Architecture Tests y convergencia contra Spec, OpenAPI y modelo.
