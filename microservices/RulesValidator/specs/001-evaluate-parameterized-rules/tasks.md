# Tareas - Feature 001

## Fundación y arquitectura

- [x] T001 Crear `RulesValidator.sln`.
- [x] T002 Crear proyectos Api, Application, Domain e Infrastructure.
- [x] T003 Configurar referencias de Clean Architecture.
- [x] T004 Crear proyectos Domain.Tests, Application.Tests, IntegrationTests y ArchitectureTests.
- [x] T005 Implementar Architecture Tests: Domain y Application no referencian Infrastructure, Api, EF Core ni Microsoft RulesEngine; únicamente Infrastructure referencia Microsoft RulesEngine.

## Dominio y Application

- [ ] T006 Crear `Regla`, `VersionRegla`, `FlujoReglas`, `DefinicionRegla` y `ResultadoEvaluacionRegla`.
- [ ] T007 Crear estados CUMPLE, NO_CUMPLE, NO_APLICA e INDETERMINADO.
- [ ] T008 Crear severidades INFORMATIVA, ADVERTENCIA y BLOQUEANTE.
- [ ] T009 Crear `IRepositorioReglas`, `IRepositorioEjecucionesReglas`, `IMotorReglas`, `IValidadorDefinicionRegla` e `IReloj`.
- [ ] T010 Crear `EvaluarReglasCommand` y `EvaluarReglasHandler`; no crear un servicio de evaluación duplicado.
- [ ] T011 Implementar precedencia de resultadoGeneral definida en spec.md.
- [ ] T012 Generar `idEvaluacion` GUID e interpretar `fechaEvaluacion` UTC opcional.

## Modelo neutral y seguridad

- [ ] T013 Incorporar y validar `rule-definition-1.0.schema.json`.
- [ ] T014 Implementar catálogo allow-list de campos y operadores.
- [ ] T015 Validar tipos, propiedades desconocidas, profundidad máxima 5, máximo 50 condiciones y máximo 50 valores EN/NO_EN.
- [ ] T016 Garantizar que definiciones rechazadas no lleguen al Adapter.

## Azure SQL

- [ ] T017 Crear `RulesDbContext` y mappings de negocio, flujo_reglas, regla, version_regla y ejecucion_regla.
- [ ] T018 Configurar PK, FK, UK, índices y CHECK de data-model.md.
- [ ] T019 Crear migración inicial.
- [ ] T020 Implementar `RepositorioSqlReglas`.
- [ ] T021 Consultar reglas GLOBAL y específicas por negocio, proceso y etapa.
- [ ] T022 Filtrar flujo/regla activos, versión ACTIVA y vigencia temporal inclusiva; usar fecha recibida desde Application.
- [ ] T023 Implementar `RepositorioSqlEjecucionesReglas` y persistir ejecución después de consolidar.

## Adapter

- [ ] T024 Instalar Microsoft RulesEngine solo en Infrastructure.
- [ ] T025 Implementar `ExpressionBuilder` para operadores permitidos, incluyendo EN/NO_EN sin métodos arbitrarios.
- [ ] T026 Implementar `MicrosoftRuleEngineAdapter` y mapeo de resultados neutral.
- [ ] T027 Configurar explícitamente CustomTypes y AutoRegisterInputType; no exponer Reflection, IO, procesos, red ni entorno.
- [ ] T028 Implementar manejo de datos ausentes como INDETERMINADO / DATOS_INSUFICIENTES.

## API y errores

- [ ] T029 Crear `POST /api/v1/evaluaciones-reglas`.
- [ ] T030 Crear DTOs alineados con rules-api.yaml, usando `mesadaEnSmlmv`.
- [ ] T031 Implementar validación de request y ProblemDetails con `errors`.
- [ ] T032 Implementar 400, 404, 422 y 500 controlado.

## Observabilidad

- [ ] T033 Configurar OpenTelemetry.
- [ ] T034 Implementar middleware X-Correlation-ID y generación si falta.
- [ ] T035 Medir `duracionMs` y propagar idEvaluacion, idCorrelacion e idTraza.
- [ ] T036 Crear métricas definidas en plan.md.
- [ ] T037 Implementar logging estructurado y protección de PII/contexto.

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
