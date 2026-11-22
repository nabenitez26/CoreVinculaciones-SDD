# Tareas - Feature 002

## Reglas de ejecución

- Implementar un incremento por vez. Al terminar: informe, `dotnet build` de RulesValidator.Api y detenerse.
- No ejecutar proyectos de pruebas (`microservices/RulesValidator/AGENTS.md`, "Política de ejecución de pruebas"). Permitido: `dotnet restore` y `dotnet build` de proyectos productivos. No modificar ni compilar proyectos de pruebas sin instrucción explícita.
- No generar migraciones. No modificar Feature 001 salvo extensiones compatibles. `admin-api.yaml` solo con cambios aprobados. No modificar `acceptance-tests.md` salvo cambios aprobados ni el schema canónico `specs/001-evaluate-parameterized-rules/schemas/rule-definition-1.0.schema.json`.
- No hacer commit.
- Nombres de código según `docs/global/CODING_STANDARDS.md` (sufijos Command, Query, Handler, Dto, Repository). Los nombres de `plan.md` son conceptuales.
- ProblemDetails con `codigo`, `idCorrelacion` y `traceId` a nivel superior. El arreglo `errores` solo en la respuesta de `POST /api/v1/reglas/validar-definicion`.
- Alcance NULL: coincidencia exacta de flujo (NULL incluido). En `GET /reglas`, un parámetro omitido significa "sin filtro". En `GET /catalogos/campos`, omitido/NULL resuelve el catálogo global aplicable (T128).
- Schema canónico: el de Feature 001. Feature 002 no mantiene copia propia.
- Identidad (POC, temporal): el campo `usuario` viene en el body de las escrituras (crear regla, crear versión, actualizar BORRADOR, enviar-revisión, aprobar, activar, inactivar). Obligatorio, trim, máximo 100; inválido → 400 `SOLICITUD_INVALIDA`. Es declarado, no autenticado, y no se usa para autorización. No hay `IUsuarioActual` ni 401/403 en la POC. Las consultas GET y `validar-definicion` no reciben `usuario` (ver T141).

## Estado de los incrementos

| Incremento | Contenido | Estado |
|---|---|---|
| 1 | Consultas, catálogos, validación sin persistencia, endpoints de lectura | Cerrado. `dotnet build` de RulesValidator.Api: 0 errores, 0 advertencias. Pruebas no ejecutadas. T126–T129 cerradas. T130 diferido (no bloquea) |
| 2 | Creación de regla, versiones, actualización de BORRADOR | Cerrado. `dotnet build` de RulesValidator.Api: 0 errores, 0 advertencias. T133–T137 cerradas. T132 pendiente (verificación en BD, no bloquea código). Pruebas diferidas |
| 3 | Ciclo de vida, vigencias, concurrencia de activación | Implementado. `dotnet build` de RulesValidator.Api: 0 errores, 0 advertencias. Usuario de aprobación declarado en body (POC, T141). Identidad real diferida (T131). Pruebas diferidas |
| 4 | Observabilidad, autorización, convergencia, compilación, preparación de validación manual | No iniciado |

---

## Incremento 1 - Consultas, catálogos y validación sin persistencia

### Contrato y dominio

- [x] T001 Contrato OpenAPI: usar `specs/002AdminRules/contracts/admin-api.yaml` existente. No crear `rules-admin-api.yaml`. Se modificó solo para incorporar `aplicabilidad` (T127), documentar el alcance de `/catalogos/campos` (T128) y añadir `usuario` a las escrituras. Los 401/403 se retiraron de la POC (T141).
- [x] T002 Definir DTOs de consulta administrativa (`Application/Administracion/Dtos`).
- [x] T006 Definir respuesta de validación de RuleDefinition (`valida`, `errores`).
- [x] T007 Definir errores funcionales de administración y su mapeo a ProblemDetails (`AdministracionReglasExceptionHandler`, códigos en `CodigosAdministracion`).
- [ ] T008 Formalizar las transiciones de estado permitidas. (Incremento 3)

### Consultas

- [x] T009 Implementar consulta de reglas por alcance (filtros omitidos = sin filtro).
- [x] T010 Implementar detalle de una regla por `reglaId`.
- [x] T011 Implementar historial de versiones.
- [x] T012 Implementar consulta de versión específica.

### Catálogos

- [x] T013 Implementar catálogo de negocios (`ref.negocio`, solo lectura).
- [x] T014 Implementar catálogo de tipos de plan por negocio (jerarquía completa). Negocio inexistente → 404 `NEGOCIO_NO_EXISTE` (T105).
- [x] T015 Implementar catálogo de campos por alcance (reutiliza `IRepositorioCatalogoCampos`).
- [x] T016 Incluir operadores permitidos por campo.
- [x] T017 Incluir información funcional de `cfg.dato_requerido` (`nombre` como nombre funcional; `tipoValor` sigue viniendo de `reglas.campo_regla`, no de `tipo_dato_id`). Ejemplo verificado: DR-002 = "Mesada pensional", `tipo_dato_id` = TEXTO; `pension.mesada` = NUMERO.
- [x] T018 Implementar catálogo de parámetros (`cfg.parametro`, filtro opcional por `fecha`).
- [x] T019 Verificar que los catálogos sean únicamente de lectura (repositorios sin escritura; `AsNoTracking`).

### Validación sin persistencia

- [x] T020 Reutilizar `IValidadorDefinicionRegla` de Feature 001.
- [x] T021 Implementar validación administrativa sin persistencia (`ValidarDefinicionReglaHandler` delegando en `ValidacionDefinicionAdministrativa`).
- [x] T022 Validar campo permitido (`CAMPO_NO_PERMITIDO`).
- [x] T023 Validar operador permitido (`OPERADOR_NO_PERMITIDO`).
- [x] T024 Validar compatibilidad de tipo (allow-list de Feature 001, código `DEFINICION_REGLA_INVALIDA`).
- [x] T025 Validar `PARAMETRO_MULTIPLICADO` (`PARAMETRO_NO_EXISTE` consultando `cfg.parametro`).
- [x] T027 Validar alcance negocio/plan. Error 400 `SOLICITUD_INVALIDA` (T129).
- [x] T066 Rechazar expresiones ejecutables (solo se aceptan campos tipados; el JSON neutral pasa por el esquema canónico).
- [x] T067 Rechazar propiedades desconocidas en RuleDefinition (`UnmappedMemberHandling.Disallow` y esquema canónico).
- [x] T106 Extensión compatible del validador de Feature 001 para devolver códigos de error (`ErroresDetallados`, `InvalidaConCodigos`). `Errores` conserva los mismos mensajes.
- [x] T127 `aplicabilidad` incorporada a `DefinicionReglaRequestDto` y a `admin-api.yaml` (raíz; el esquema canónico la rechaza en nodos anidados).

### Endpoints de lectura y validación

- [x] T053 Exponer `GET /api/v1/reglas`.
- [x] T054 Exponer `GET /api/v1/reglas/{reglaId}` (se usa `reglaId`, no `codigoRegla`).
- [x] T055 Exponer historial de versiones.
- [x] T059 Exponer validación de definición. Alcance inválido → 400 `SOLICITUD_INVALIDA`; RuleDefinition inválida → 200 con `valida=false` y `errores[]` (T129).
- [x] T064 Exponer endpoints de catálogos.
- [x] T065 Mantener ProblemDetails consistente con Feature 001 (`codigo`, `idCorrelacion` en el nivel superior).
- [x] T068 Verificar que Microsoft RulesEngine no quede expuesto por la API (ningún tipo del motor en DTOs ni respuestas).
- [x] T079 Propagar `X-Correlation-ID` (middleware existente; los errores lo incluyen en `idCorrelacion`).

### Cierre del incremento 1

- [x] T120 Compilar `RulesValidator.Api` (`dotnet build`): 0 errores, 0 advertencias. EF: `has-pending-model-changes` sin cambios (no se generó migración).
- [x] T121 Informe y detención.

### Confirmaciones del incremento 1

- [x] T126 **Cerrada.** Verificado en BD que `cfg.dato_requerido` tiene la columna `nombre`. Mapping sin cambios.
- [x] T128 **Cerrada.** En `GET /reglas`, query omitido = sin filtro. En `GET /catalogos/campos`, los parámetros son alcance de resolución: omitido/NULL resuelve el catálogo global aplicable, no una búsqueda abierta sobre todos los campos.
- [x] T129 **Cerrada.** `validar-definicion`: request/alcance inválido → 400 `SOLICITUD_INVALIDA`; request correcto con RuleDefinition inválida → 200, `valida=false`, `errores[]`.
- [ ] T130 **Diferido por decisión (no bloquea).** `EvaluarReglasHandlerTests` implementa `IRepositorioCatalogoCampos` con `string`; la interfaz ahora usa `string?`. Advertencias de nulabilidad al compilar pruebas. Resolver en el gate de pruebas posterior. No modificar ni compilar proyectos de pruebas en incrementos actuales.

---

## Incremento 2 - Creación y versionamiento

### Gate previo

- [x] T103 Comparación estática **cerrada**: el modelo EF declara `HasAlternateKey` para `UK_regla_flujo_codigo` (`FlujoReglasId`, `Codigo`) y `UK_version_regla_regla_version` (`ReglaId`, `Version`); el snapshot tiene las mismas alternate keys y la migración inicial crea las UK con los mismos nombres. (Corrige una afirmación anterior: el modelo sí las declara.)
- [ ] T132 Verificación en BD real de `UK_regla_flujo_codigo` y `UK_version_regla_regla_version`. **Pendiente:** requiere consulta sobre la BD (sys.key_constraints). Las escrituras dependen de estas UK para la última defensa de unicidad; el código ya traduce violaciones a 409 y no depende de la BD para la validación de negocio (pre-chequeos en aplicación).

### Crear regla

- [x] T003 Definir DTO de creación de regla (`CrearReglaRequestDto`, sin prioridad ni usuario).
- [x] T028 Implementar `CrearReglaCommand` / `CrearReglaHandler`.
- [x] T029 Resolver flujo existente por alcance exacto (NULL incluido). Filtrado en memoria para evitar comodines de NULL.
- [x] T030 Rechazar creación cuando el flujo no exista (`FLUJO_NO_EXISTE`, 404).
- [x] T031 Garantizar unicidad de código dentro del flujo (`REGLA_YA_EXISTE`, 409): pre-chequeo en aplicación y UK en la transacción.
- [x] T026 Validar vigencia (`VIGENCIA_INVALIDA`, 422 cuando `fechaFinVigencia < fechaInicioVigencia`). Límites inclusivos.
- [x] T032 Crear regla y versión 1 en transacción (serializable, un único `SaveChanges`).
- [x] T033 Crear versión 1 como BORRADOR.
- [x] T034 No activar automáticamente la regla (versión 1 en BORRADOR; regla `Activo = true` solo indica identidad).
- [x] T035 Registrar usuario y fecha de creación. POC (T141): `usuario` viene del body, se valida (400) y se persiste en `usuario_creacion`. Sin valores de respaldo.
- [x] T110 Prioridad técnica = 0 en reglas nuevas (C015). Tipo `PARAMETRIZABLE`. Sin exponer prioridad.

### Versionamiento

- [x] T004 Definir DTO de creación de versión (`VersionReglaRequestDto`).
- [x] T036 Implementar creación de nueva versión (`CrearVersionReglaCommand` / `CrearVersionReglaHandler`).
- [x] T037 Determinar siguiente versión con protección de concurrencia: MAX(version)+1 en transacción serializable; violación de UK → 409.
- [x] T038 Preservar versiones anteriores (solo se insertan filas nuevas).
- [x] T039 Crear nueva versión como BORRADOR.
- [x] T057 Exponer `POST /api/v1/reglas/{reglaId}/versiones` (201).
- [x] T072 Controlar creación simultánea del mismo número de versión (`CONFLICTO_CONCURRENCIA`, 409).

### Actualización de BORRADOR

- [x] T005 Definir DTO de actualización de BORRADOR (mismo cuerpo que creación de versión).
- [x] T040 Implementar actualización de BORRADOR (solo severidad, codigoError, mensajeError, vigencias y definición). No cambia reglaId, código, número de versión ni alcance.
- [x] T041 Rechazar modificación de EN_REVISION (`VERSION_INMUTABLE`, 409).
- [x] T042 Rechazar modificación de APROBADA (`VERSION_INMUTABLE`, 409).
- [x] T043 Rechazar modificación de ACTIVA (`VERSION_INMUTABLE`, 409).
- [x] T044 Rechazar modificación de INACTIVA (`VERSION_INMUTABLE`, 409).
- [x] T058 Exponer `PUT /api/v1/reglas/{reglaId}/versiones/{version}` (200).
- [x] T071 Controlar creación simultánea de la misma regla (`REGLA_YA_EXISTE`, 409 por UK).

### Decisiones de Incremento 2 (cerradas)

- [x] T133 **Precedencia (cerrada):** 1) validación estructural HTTP (400); 2) autenticación/autorización requerida (401/403); 3) caso de uso. Request malformado sin identidad → 400.
- [x] T134 **Formato neutral en Application (cerrada):** `DefinicionReglaDto` (Application) → `ConstructorDefinicionNeutral` → validación → persistencia. La API solo mapea HTTP → DTO → Command/Query; no construye `definicion_json`.
- [x] T135 **Flujo ambiguo (cerrada):** más de 1 flujo ACTIVO con coincidencia exacta → 409 `FLUJO_AMBIGUO`. No se selecciona arbitrariamente. Código añadido a spec §27 y plan §19.
- [x] T136 **Solo flujos ACTIVOS (cerrada):** 0 activos → 404 `FLUJO_NO_EXISTE`; 1 → continuar; >1 → 409 `FLUJO_AMBIGUO`. Inactivos no elegibles. No se activan ni reactivan flujos automáticamente. Implementado en `ObtenerFlujosActivosExactosAsync`.
- [x] T137 **Last-write-wins (cerrada, solo PUT de BORRADOR):** sin rowversion, ETag ni If-Match. C008 cerrada para Feature 002 v1. Se mantienen protección transaccional y constraints para creación concurrente, número de versión, aprobación, activación, reemplazo ACTIVA y vigencias.

### Cierre del incremento 2

- [x] T122 Compilar `RulesValidator.Api` (`dotnet build`): 0 errores, 0 advertencias.
- [x] T123 Informe y detención.

---

## Incremento 3 - Ciclo de vida y vigencias

### Transiciones

- [x] T008 Transiciones formalizadas por operación explícita: BORRADOR → EN_REVISION (enviar), EN_REVISION → APROBADA (aprobar), APROBADA → ACTIVA (activar), ACTIVA → INACTIVA (inactivar). No hay endpoint genérico de estado.
- [x] T045 Implementar BORRADOR → EN_REVISION (`EnviarRevisionVersionHandler`).
- [x] T046 Validar definición antes de enviar a revisión: 422 `DEFINICION_REGLA_INVALIDA` sin cambiar el estado.
- [x] T047 Implementar EN_REVISION → APROBADA (`AprobarVersionHandler`).
- [x] T048 Registrar aprobador y fecha. POC (T141): `usuario_aprobacion` desde el body (validado, 400) y `fecha_aprobacion` UTC desde `TimeProvider`. Sin autorización por rol: pendiente de T069/C001.
- [x] T049 Implementar APROBADA → ACTIVA (`ActivarVersionHandler`).
- [x] T050 Validar antes de activar: estado APROBADA (409), vigencia (422 `VIGENCIA_INVALIDA`), definición (422) y solapamiento dentro de la transacción.
- [x] T113 Reemplazo transaccional en `ActivarVersionAsync`: versiones ACTIVA con vigencia solapada → INACTIVA y la objetivo → ACTIVA en la misma transacción serializable. Las no solapadas coexisten (C005/C006). Rollback si falla cualquier cambio.
- [x] T051 Implementar ACTIVA → INACTIVA (`InactivarVersionHandler`).
- [x] T052 Rechazar transiciones no permitidas (`TRANSICION_ESTADO_INVALIDA`, 409), incluido el estado de origen recomprobado dentro de la transacción.

### Endpoints de ciclo de vida

- [x] T060 Exponer `POST .../versiones/{version}/enviar-revision` (200).
- [x] T061 Exponer `POST .../versiones/{version}/aprobar` (200). Requiere body con `usuario` (POC, T141).
- [x] T062 Exponer `POST .../versiones/{version}/activar` (200).
- [x] T063 Exponer `POST .../versiones/{version}/inactivar` (200).

### Concurrencia de activación

- [x] T073 Aprobación concurrente: el estado de origen se recomprueba dentro de la transacción; la segunda operación recibe 409 `TRANSICION_ESTADO_INVALIDA`.
- [x] T074 Activación concurrente (AT-044): transacción serializable sobre el conjunto de versiones de la regla. Deadlock o conflicto de serialización (1205, 41302, 41305, 41325) → 409 `CONFLICTO_CONCURRENCIA`.
- [x] T075 Conflictos conocidos mapeados a `CONFLICTO_CONCURRENCIA`.

### Decisiones de Incremento 3

- [x] T138 **Cerrada.** `CONFLICTO_VERSION_ACTIVA` se mantiene en spec §27 y no se retira. Es un conflicto funcional de activación/vigencia: solo aplica si la nueva configuración no puede quedar válidamente activa. Los solapamientos con reemplazo válido se resuelven transaccionalmente (ACTIVA anterior → INACTIVA, APROBADA nueva → ACTIVA) sin devolver ese código. Versiones ACTIVA con vigencias no solapadas siguen permitidas. `CONFLICTO_CONCURRENCIA` es el conflicto técnico por operaciones concurrentes. Ninguno de los dos códigos se retira. Nota de implementación: hoy no se emite `CONFLICTO_VERSION_ACTIVA` porque todo solapamiento es reemplazo válido (T139).
- [x] T139 El reemplazo desactiva todas las ACTIVA solapadas, no solo una. Mantiene la invariante de una versión aplicable por fecha.
- [x] T140 Las lecturas GET y el resto de transiciones siguen sin requisito de rol; la autorización por operación queda en Incremento 4 (T069).

### Cierre del incremento 3

- [x] T124 Compilar `RulesValidator.Api` (`dotnet build`): 0 errores, 0 advertencias.
- [ ] T125 Informe y detención.

---

## Incremento 4 - Observabilidad, autorización, convergencia y validación manual

### Observabilidad

- [x] T076 Trazas de operaciones administrativas: `AdministracionReglasTelemetry.EjecutarAsync` (Application/Observabilidad) crea la actividad `admin.{operacion}` en el ActivitySource `RulesValidator.Application`, ya registrado en Program.cs. Aplicado a las 8 operaciones de escritura, transición y validación (`validar_definicion`, `crear_regla`, `crear_version`, `actualizar_borrador`, `enviar_revision`, `aprobar`, `activar`, `inactivar`).
- [x] T077 Métricas `rulesvalidator.admin.operations.total`, `rulesvalidator.admin.operation.duration`, `rulesvalidator.admin.validation.rejected` (`ValidacionAdministracionException`) y `rulesvalidator.admin.state_transition.rejected` (`ConflictoAdministracionException` con `TRANSICION_ESTADO_INVALIDA`). Dimensiones: `operacion` y `resultado` (ok | error). Sin `reglaId`, usuario ni código. Se usa el Meter `RulesValidator` ya registrado en Program.cs.
- [x] T078 No se registra RuleDefinition en trazas, métricas ni logs. El mensaje de excepciones no se registra en la telemetría.

### Autorización

- [ ] T069 Implementar autorización por operación (consultar, editar BORRADOR, enviar a revisión, aprobar, activar/inactivar) con las políticas estándar. Roles pendientes en C001: no inventar roles. Las lecturas GET podrán endurecerse aquí (decisión cerrada).
- [x] T070 Verificado: no hay `SYSTEM`, `ADMIN`, `USR-011`, `anonymous`, `oid`, `sub`, `upn`, `preferred_username` ni `employeeId` en Application ni Api. En la POC el usuario viene solo del body validado (T141).
- [ ] T100 Autorización: pruebas (no ejecutar en esta fase).

### Convergencia

- [x] T080 Confirmado: el diff de `RulesDbContext` no agrega `DbSet` ni `ToTable` nuevos. Feature 002 no crea tablas.
- [x] T081 Confirmado: `git status` no muestra cambios en maestros del Core (`git status -- src` sin rutas de catálogos Core).
- [x] T082 Confirmado: no hay migraciones nuevas de Feature 002. Las existentes (`InitialMigration`, `AgregarFuenteExternaMapeo`) están en el commit `07b5e9c` y no se modificaron.
- [x] T102 Validar convergencia Spec/OpenAPI/modelo/implementación. Completada: `data-model.md` §7 (transiciones explícitas, sin retrocesos) y §9 (reemplazo transaccional, invariante, códigos); `clarifications.md` (decisión T138); `spec.md`, `plan.md`, `acceptance-tests.md` y `admin-api.yaml` ya reflejan el comportamiento implementado.
- [x] T116 `dotnet build -c Release` de `RulesValidator.Api`: 0 errores, 0 advertencias.

### Preparación de validación manual

- [ ] T083 Crear una regla simple en BORRADOR.
- [ ] T084 Consultar la regla creada.
- [ ] T085 Validar una definición sin persistir.
- [ ] T086 Crear una segunda versión.
- [ ] T087 Actualizar la segunda versión en BORRADOR.
- [ ] T088 Enviar la versión a revisión.
- [ ] T089 Aprobar la versión.
- [ ] T090 Activar la versión.
- [ ] T091 Verificar rechazo de edición sobre ACTIVA.
- [ ] T092 Inactivar la versión.
- [ ] T093 Confirmar que Feature 001 respeta el cambio de configuración sin despliegue.

---

## Pruebas automatizadas (diferidas)

No ejecutar ni modificar proyectos de pruebas durante estos incrementos salvo instrucción explícita.

- [ ] T094 Crear pruebas de creación.
- [ ] T095 Crear pruebas de versionamiento.
- [ ] T096 Crear pruebas de máquina de estados.
- [ ] T097 Crear pruebas de concurrencia.
- [ ] T098 Crear pruebas de catálogos.
- [ ] T099 Crear Contract Tests de administración.
- [ ] T101 Ejecutar suite completa en el gate final.
- [ ] T130 (ver Incremento 1): resolver advertencias de nulabilidad en `EvaluarReglasHandlerTests` en el gate de pruebas.

---

## Tareas nuevas y deuda documentada

- [ ] T104 **Identidad administrativa productiva: DIFERIDA** (no bloqueante para la POC). Se retiró `IUsuarioActual`, `IdentidadAdministrativa`, `UsuarioActualSinProveedor` y su registro en DI. Se conservan como concepto de arquitectura futura: Bearer Token → identidad corporativa → ClaimsPrincipal → `IUsuarioActual` → auditoría.
- [ ] T131 Implementación HTTP de identidad corporativa (Bearer, claim, `ClaimsPrincipal`). **DIFERIDA** para integración productiva. No bloquea la validación manual de la POC. Al implementarse, `body.usuario` se reemplaza por la identidad autenticada.
- [x] T141 **Usuario declarado en body (cambio aprobado para la POC).** Escrituras reciben `usuario` (obligatorio, string, trim, no vacío, máximo 100; inválido → 400 `SOLICITUD_INVALIDA`). Persistencia: `usuario_creacion` (regla y versión), `usuario_aprobacion` (aprobar). Enviar-revisión, activar, inactivar y actualizar BORRADOR validan sin persistir (no hay columna). GET y `validar-definicion` no lo reciben. `usuario` no se usa para autorización. Sin migraciones ni cambios al modelo SQL. Retirados 401/403 del alcance POC. Documentado en `spec.md` §25, `clarifications.md`, `acceptance-tests.md` (AT-051 a AT-053) y `admin-api.yaml`.
- [x] T105 **Código de negocio inexistente.** Cerrada: `GET /catalogos/tipos-plan` con negocio inexistente → 404 `NEGOCIO_NO_EXISTE`.
- [ ] T109 Corrección documental separada: `constitution.md` termina a mitad de frase en §8 ("Las restricciones de integridad pertenecen"). No bloquea.
- [ ] T117 Deuda de seguridad (separada de Feature 002): credencial SQL en texto plano en `RulesValidator.Api/appsettings.json`. Mover a secretos.
- [ ] T118 Deuda de configuración (separada de Feature 002): `AzureCliCredential` en `DependencyInjection.cs` es temporal solo para desarrollo local. Producción en AKS: Workload Identity / Managed Identity. No modificar dentro de Feature 002.
- [ ] T119 Limitación documentada: `GET /reglas` no puede seleccionar solo reglas de alcance global. Decidir si se requiere en el futuro; no cambia el contrato en Feature 002.
- [x] T107 Revisar que la prioridad no aparezca en DTOs ni endpoints administrativos (verificado en DTOs y respuestas de Incrementos 1 y 2).
