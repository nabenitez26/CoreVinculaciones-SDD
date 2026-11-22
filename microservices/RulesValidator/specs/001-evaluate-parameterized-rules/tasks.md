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

- [x] T017 Crear `RulesDbContext` con las seis tablas del esquema `reglas` y referenciar catálogos Core sin duplicarlos.
- [x] T018 Configurar PK, FK, UK, índices y CHECK definidos en `data-model.md`.
- [x] T019 Crear baseline de migración del modelo RulesValidator.
- [x] T020 Implementar `RepositorioSqlReglas`.
- [x] T021 Consultar flujos globales y específicos.
- [x] T022 Filtrar flujo/regla activos y versión ACTIVA vigente.
- [x] T023 Implementar `RepositorioSqlEjecucionesReglas` y persistir la ejecución consolidada.

## Adapter

- [x] T024 Instalar Microsoft RulesEngine únicamente en Infrastructure.
- [x] T025 Implementar `ExpressionBuilder` para operadores permitidos.
- [x] T026 Implementar `MicrosoftRuleEngineAdapter` y normalización de resultados.
- [x] T027 Restringir CustomTypes, AutoRegisterInputType y capacidades accesibles desde expresiones.
- [x] T028 Implementar datos ausentes como INDETERMINADO / DATOS_INSUFICIENTES.

## API y errores

- [x] T029 Crear `POST /api/v1/evaluar-reglas`.
- [x] T030 Crear DTOs alineados con `rules-api.yaml`.
- [x] T031 Implementar validación del request y ProblemDetails.
- [x] T032 Implementar respuestas 400, 404, 422 y 500 controladas.

## Observabilidad

- [x] T033 Configurar OpenTelemetry.
- [x] T034 Implementar middleware X-Correlation-ID.
- [x] T035 Medir duración y propagar IDs de evaluación, correlación y traza.
- [x] T036 Crear métricas definidas en `plan.md`.
- [x] T037 Implementar logging estructurado y protección de PII/contexto.

## Validación manual completada - Etapa 4

- [x] T038 Validar RV-001 con tipo de pensión permitido.
- [x] T039 Validar RV-002 para TP-01.
- [x] T040 Validar RV-003 para TP-02.
- [x] T041 Validar RV-101 con mesada mínima.
- [x] T042 Validar RV-102 con grupos anidados `Y` / `O`.
- [x] T043 Validar RV-103 con tipos de documento permitidos.
- [x] T044 Validar resultado CUMPLE con múltiples reglas.
- [x] T045 Validar resultado NO_CUMPLE con regla BLOQUEANTE.
- [x] T046 Validar resultado INDETERMINADO por dato ausente.
- [x] T047 Validar Caso A del escenario POC.
- [x] T048 Validar Caso B: RV-102 rechaza edad 55 para pensión de vejez.
- [x] T049 Validar Etapa 4 del Caso C con TP-02.
- [x] T050 Confirmar persistencia y trazabilidad de la evaluación.

## Parámetros Core y SMMLV

- [x] T051 Mapear `cfg.parametro` como entidad Core con `ExcludeFromMigrations()`.
- [x] T052 Implementar `IRepositorioParametros`.
- [x] T053 Implementar `RepositorioSqlParametros`.
- [x] T054 Extender RuleDefinition con `PARAMETRO_MULTIPLICADO`.
- [x] T055 Validar parámetro, factor y compatibilidad con campo NUMERO.
- [x] T056 Implementar `ResolverParametrosRegla`.
- [x] T057 Resolver parámetros según `fechaEvaluacion` y `vigencia`.
- [x] T058 Resolver SMMLV antes de `ExpressionBuilder`.
- [x] T059 Actualizar RV-002 a `pension.mesada <= 1.5 × SMMLV`.
- [x] T060 Actualizar RV-003 a `pension.mesada > 1.5 × SMMLV`.
- [x] T061 Actualizar RV-101 a `pension.mesada >= 1 × SMMLV`.
- [x] T062 Validar manualmente Casos A, B y C usando mesada monetaria.

## Etapa 5 - FD-PENS / RV-104

- [x] T063 Parametrizar flujos CAJA / VINCULACION / TP-01 y TP-02 para Etapa 5.
- [x] T064 Parametrizar RV-104 versión 1.
- [x] T065 Configurar `fuente.pension.respuesta` como campo TEXTO permitido.
- [x] T066 Configurar operador `IGUAL` para `fuente.pension.respuesta`.
- [x] T067 Registrar `FD-PENS` en `ref.fuente_dato` para la POC.

- [ ] T068 Mapear `ref.fuente_dato` como entidad Core usando `ExcludeFromMigrations()`.
- [ ] T069 Crear abstracción de Application para consultar la fuente de pensionados.
- [ ] T070 Crear modelo de resultado de fuente con estados `ENCONTRADO`, `SIN_RESPUESTA`, `NO_DISPONIBLE` e `INVALIDO`.
- [ ] T071 Agregar configuración para `FD-PENS`, contenedor y blob `fuente_externa_simulada`.
- [ ] T072 Agregar `Azure.Storage.Blobs` y `Azure.Identity` únicamente a Infrastructure.
- [ ] T073 Implementar proveedor Azure Blob Storage de `FD-PENS`.
- [ ] T074 Utilizar `DefaultAzureCredential` y permitir Managed Identity en Azure.
- [ ] T075 Abrir la fuente mediante stream sin persistir una copia local permanente.
- [ ] T076 Validar encabezados `NUMERO_DOCUMENTO` y `RESPUESTA`.
- [ ] T077 Tratar `NUMERO_DOCUMENTO` como texto y preservar ceros iniciales.
- [ ] T078 Detectar registros duplicados o ambiguos como fuente `INVALIDO`.
- [ ] T079 Consultar el documento recibido mediante `persona.numeroDocumento`.
- [ ] T080 Crear una copia enriquecida del contexto sin modificar de forma insegura el contexto original.
- [ ] T081 Agregar `fuente.pension.respuesta` únicamente cuando exista una respuesta válida.
- [ ] T082 Ejecutar RV-104 sobre el contexto enriquecido.
- [ ] T083 Convertir documento no encontrado o respuesta vacía en `INDETERMINADO`.
- [ ] T084 Convertir indisponibilidad esperada de FD-PENS en `INDETERMINADO`.
- [ ] T085 Mantener severidad `ADVERTENCIA` para RV-104.
- [ ] T086 Evitar convertir indisponibilidad de la fuente en `NO_CUMPLE`.
- [ ] T087 Garantizar que RulesValidator no cree `TC-03` ni modifique estados del Core.
- [ ] T088 Impedir logging o persistencia de `persona.numeroDocumento`.
- [ ] T089 Agregar observabilidad de consultas externas sin dimensiones de alta cardinalidad.
- [ ] T090 Validar manualmente documento encontrado con respuesta `Reconocida`.
- [ ] T091 Validar manualmente documento inexistente.
- [ ] T092 Validar manualmente blob no disponible.
- [ ] T093 Validar manualmente archivo inválido.
- [ ] T094 Compilar proyectos productivos y verificar que no se generaron migraciones.

## Pruebas automatizadas pendientes

> No ejecutar ni modificar los proyectos de pruebas durante la implementación
> actual. Estas tareas permanecen pendientes hasta habilitar nuevamente ese
> alcance.

- [ ] T095 Implementar pruebas unitarias funcionales de RV-001 a RV-104.
- [ ] T096 Probar consolidación de `resultadoGeneral`.
- [ ] T097 Probar combinación GLOBAL + negocio específico.
- [ ] T098 Probar vigencias ACTIVA, INACTIVA, APROBADA, futura y vencida.
- [ ] T099 Probar límites inclusivos de vigencia.
- [ ] T100 Probar campos, operadores y tipos no permitidos.
- [ ] T101 Probar profundidad, cantidad de condiciones y EN/NO_EN.
- [ ] T102 Probar que una definición rechazada no invoca el Adapter.
- [ ] T103 Probar `ExpressionBuilder` y `MicrosoftRuleEngineAdapter`.
- [ ] T104 Probar resolución de `PARAMETRO_MULTIPLICADO`.
- [ ] T105 Probar errores de parámetros inexistentes, inválidos y fuera de vigencia.
- [ ] T106 Probar contratos 400, 404, 422 y 500.
- [ ] T107 Probar Correlation ID y ausencia de PII.
- [ ] T108 Probar persistencia de `reglas.ejecucion_regla`.
- [ ] T109 Probar proveedor FD-PENS.
- [ ] T110 Ejecutar Architecture Tests y suite completa.
- [ ] T111 Ejecutar convergencia final contra `spec.md`, `plan.md`, `data-model.md` y `rules-api.yaml`.