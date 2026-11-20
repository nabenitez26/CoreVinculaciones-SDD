# Clarificaciones - Feature 001

## C-001 Ruta única

La ruta oficial es:

```http
POST /api/v1/evaluaciones-reglas
```

## C-002 Mesada

El nombre único es `pension.mesadaEnSmlmv`. El valor llega normalizado en SMLMV.

## C-003 idEvaluacion

RulesValidator genera `idEvaluacion`; el consumidor no lo envía. `idCorrelacion` se recibe opcionalmente en `X-Correlation-ID` o se genera.

## C-004 fechaEvaluacion

Es opcional y se expresa en ISO 8601 UTC. Si falta, Application usa `IClock.UtcNow`. SqlRuleRepository recibe la fecha como parámetro.

## C-005 Reglas globales

`GLOBAL` es un negocio especial. Para cada evaluación se combinan flujos GLOBAL y del negocio solicitado que coincidan en proceso y etapa.

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

Los campos, tipos y operadores permitidos se administran en las tablas `campo_regla` y `campo_regla_operador` de Azure SQL mediante un proceso externo. RulesValidator carga el catálogo activo una vez por evaluación; no lo cachea entre evaluaciones. El catálogo inicial de la migración cubre los campos y operadores requeridos por RV-001, RV-101 y RV-002.

## C-014 Alcance de globales y tipo de plan

Las reglas de GLOBAL se limitan únicamente por proceso y aplican en cualquier negocio, etapa y `tipo_plan_id` de ese proceso. Los flujos GLOBAL se almacenan con `etapa = NULL` y `tipo_plan_id = NULL`. Los flujos específicos se seleccionan por negocio, proceso y etapa; cuando el proceso es VINCULACION también deben coincidir con `tipo_plan_id`. Este campo es varchar de máximo 10 caracteres y obligatorio en requests de VINCULACION; se omite para otros procesos.
