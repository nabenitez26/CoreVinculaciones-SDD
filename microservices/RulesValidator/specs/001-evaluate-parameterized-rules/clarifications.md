# Clarificaciones - Feature 001

## C-001 Ruta única

La ruta oficial es:

```http
POST /api/v1/evaluar-reglas
```

## C-002 Mesada

El alias del contrato para la mesada es `pension.mesadaEnSmlmv`. El valor llega normalizado en SMLMV.

## C-003 idEvaluacion

RulesValidator genera `idEvaluacion`; el consumidor no lo envía. `idCorrelacion` se recibe opcionalmente en `X-Correlation-ID` o se genera.

## C-004 fechaEvaluacion

Es opcional y se expresa en ISO 8601 UTC. Si falta, Application usa `IClock.UtcNow`. SqlRuleRepository recibe la fecha como parámetro.

## C-005 Reglas globales

Los flujos globales tienen `reglas.flujo_reglas.negocio_id = NULL`; no existe una fila GLOBAL duplicada. Para cada evaluación se combinan los flujos globales aplicables y los del `negocio_id` solicitado.

## C-006 NO_APLICA

No equivale a CUMPLE ni NO_CUMPLE. No bloquea la consolidación.

## C-007 INDETERMINADO

Se usa cuando la regla aplica y falta un dato. Si falta un dato requerido por el request, se retorna HTTP 400; si falta un dato requerido por una regla, esa regla retorna INDETERMINADO.

## C-008 Vigencia

Los límites de inicio y fin son inclusivos. `fecha_fin_vigencia = NULL` significa vigencia abierta.

## C-009 EN y NO_EN

`EN` se genera como comparaciones IGUAL unidas con O. `NO_EN` se genera como comparaciones DIFERENTE unidas con Y. Máximo 50 valores.

## C-010 Regla inválida

Una definición ACTIVA inválida es error de configuración, no INDETERMINADO. Retorna HTTP 500 controlado.

## C-011 Aplicabilidad explícita

`RuleDefinition 1.0` admite un nodo opcional `aplicabilidad` en la raíz, con la misma estructura neutral de condición o grupo. La aplicabilidad se evalúa antes de la condición principal: falsa produce `NO_APLICA`; si falta un campo requerido para decidirla, produce `INDETERMINADO` / `DATOS_INSUFICIENTES`. Si es verdadera, se evalúa la condición principal.

## C-012 Tipos incompatibles en contexto

Un campo conocido de `contexto` presente con un tipo JSON incompatible se retorna como HTTP 422 ProblemDetails con `errors[].codigo = CONTEXTO_TIPO_INVALIDO` y `errors[].campo` con la ruta del campo. Si el campo está ausente o es `null`, se aplica C-007: la regla que lo requiera retorna `INDETERMINADO`.

## C-013 Catálogo dinámico de campos

Los aliases y operadores permitidos se administran en `reglas.campo_regla` y `reglas.campo_regla_operador`, con scope global o negocio/proceso/tipo de plan. `reglas.campo_regla.dato_requerido_id` mapea el alias a `cfg.dato_requerido`; el Core conserva tipo, captura, fuentes y exigibilidad. RulesValidator carga el allow-list aplicable una vez por evaluación, sin cache entre requests. Una entrada específica reemplaza el scope global por alias, incluso cuando está inactiva.

## C-014 Alcance de globales y tipo de plan

Los registros de `reglas.flujo_reglas` con `negocio_id = NULL` se limitan únicamente por proceso y aplican en cualquier negocio, etapa y `tipoPlanId` de ese proceso. Usan `etapa = NULL` y `tipo_plan_id = NULL`. Los flujos específicos se seleccionan por `negocio_id`, proceso y etapa; para VINCULACION también por `tipo_plan_id`. Este último referencia `mae.tipo_plan.tipo_plan_id` (`nvarchar(10)`).

## C-015 Contexto plano

El contexto es un objeto JSON plano. Los aliases del ejemplo OpenAPI son `pension.tipo`, `pension.mesadaEnSmlmv`, `person.genero`, `person.edad` y `person.tipoDocumento`. `tipoPlanId` participa en la selección del flujo y no se repite como campo del contexto.
