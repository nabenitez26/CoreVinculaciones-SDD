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
