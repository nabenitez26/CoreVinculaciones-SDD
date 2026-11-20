# Pruebas de aceptación - Feature 001

## 1. RV-001

| Caso | pension.tipo | Estado | Severidad | Código error | Mensaje |
|---|---|---|---|---|---|
| válido | Vejez | CUMPLE | BLOQUEANTE | null | null |
| válido | Invalidez | CUMPLE | BLOQUEANTE | null | null |
| válido | Sobrevivientes | CUMPLE | BLOQUEANTE | null | null |
| inválido | Vejéz | NO_CUMPLE | BLOQUEANTE | ERR-TIPO-PENSION | Tipo de pensión no admitido. |
| ausente | propiedad ausente | INDETERMINADO | BLOQUEANTE | DATOS_INSUFICIENTES | No existe información suficiente para evaluar RV-001. |

## 2. RV-101

| mesadaEnSmlmv | Estado | Severidad | Código error | Mensaje |
|---:|---|---|---|---|
| 0.99 | NO_CUMPLE | BLOQUEANTE | ERR-MESADA-MINIMA | La mesada debe ser de al menos un SMLMV. |
| 1.00 | CUMPLE | BLOQUEANTE | null | null |
| 1.01 | CUMPLE | BLOQUEANTE | null | null |
| ausente | INDETERMINADO | BLOQUEANTE | DATOS_INSUFICIENTES | No existe información suficiente para evaluar RV-101. |

## 3. RV-002

| plan.codigo | mesadaEnSmlmv | Estado | Severidad | Código error | Mensaje |
|---|---:|---|---|---|---|
| TP-01 | 1.49 | CUMPLE | BLOQUEANTE | null | null |
| TP-01 | 1.50 | CUMPLE | BLOQUEANTE | null | null |
| TP-01 | 1.51 | NO_CUMPLE | BLOQUEANTE | ERR-MESADA-TP01 | La mesada supera el tope del plan TP-01. |
| TP-02 | 1.40 | NO_APLICA | BLOQUEANTE | null | null |
| TP-01 | ausente | INDETERMINADO | BLOQUEANTE | DATOS_INSUFICIENTES | No existe información suficiente para evaluar RV-002. |
| ausente | 1.40 | INDETERMINADO | BLOQUEANTE | DATOS_INSUFICIENTES | No existe información suficiente para evaluar RV-002. |

La definición de RV-002 expresa `plan.codigo = TP-01` en el nodo raíz opcional `aplicabilidad`; el tope de mesada permanece en la condición principal. Si la aplicabilidad es falsa, la condición principal no se ejecuta.

## 4. Consolidación

- Una regla BLOQUEANTE NO_CUMPLE -> resultadoGeneral NO_CUMPLE.
- Sin incumplimiento bloqueante y con una regla INDETERMINADO -> resultadoGeneral INDETERMINADO.
- Solo CUMPLE y NO_APLICA -> resultadoGeneral CUMPLE.
- Una regla ADVERTENCIA NO_CUMPLE sin bloqueantes -> resultadoGeneral CUMPLE y detalle reportado.

## 5. Reglas globales

Dado negocio SALUD, proceso VINCULACION, `tipoPlanId` TP-01 y etapa 4, se recuperan primero las reglas GLOBAL del proceso sin filtrar etapa ni tipo, y luego las reglas de SALUD que coinciden con etapa y `tipo_plan_id`. Una regla GLOBAL inactiva no se evalúa.

## 5.1 tipoPlanId

- Proceso VINCULACION sin `tipoPlanId` -> 400 ProblemDetails.
- Proceso VINCULACION con `tipoPlanId` de más de 10 caracteres -> 400 ProblemDetails.
- Proceso distinto de VINCULACION con `tipoPlanId` -> 400 ProblemDetails.
- Proceso distinto de VINCULACION omite `tipoPlanId`.

## 6. Vigencia

- ACTIVA dentro de rango -> se evalúa.
- INACTIVA -> se excluye.
- APROBADA -> se excluye.
- fechaEvaluacion anterior al inicio -> se excluye.
- fechaEvaluacion posterior al fin -> se excluye.
- fecha_fin_vigencia NULL -> se evalúa.
- fechaEvaluacion igual al inicio -> se evalúa.
- fechaEvaluacion igual al fin -> se evalúa.

## 7. API y errores

- Ruta única: POST /api/v1/evaluaciones-reglas.
- Faltan negocio, proceso, etapa o contexto -> 400 ProblemDetails.
- JSON malformado, propiedad superior desconocida o X-Correlation-ID de más de 100 caracteres -> 400 ProblemDetails.
- Un campo conocido de contexto presente con tipo incompatible -> 422 ProblemDetails con `errors[].codigo = CONTEXTO_TIPO_INVALIDO` y `errors[].campo`.
- Un campo requerido por una regla ausente o `null` -> HTTP 200 y resultado individual INDETERMINADO.
- No existe flujo GLOBAL ni específico aplicable -> 404 ProblemDetails.
- Request técnicamente no procesable -> 422 ProblemDetails.
- Definición ACTIVA inválida -> 500 ProblemDetails sin filtrar definición, SQL o stack trace.

## 8. Observabilidad

- Con X-Correlation-ID se conserva el valor en respuesta, ejecución y traza.
- Sin header se genera un valor.
- `duracionMs >= 0` y coincide en respuesta, persistencia y telemetría.
- idEvaluacion es generado por el servicio y se devuelve.
- El contexto completo y PII no aparecen en logs.

## 9. Seguridad

- Campo desconocido -> definición rechazada.
- Campo activo agregado al catálogo SQL por el proceso administrador -> aceptado en la siguiente evaluación sin cambiar binarios.
- Campo inactivo o removido del catálogo -> definición que lo referencia rechazada.
- Operador desconocido -> rechazada.
- Tipo incompatible -> rechazada.
- Propiedad JSON extra -> rechazada.
- Profundidad > 5 -> rechazada.
- Condiciones > 50 -> rechazada.
- Valores EN/NO_EN > 50 -> rechazada.
- Una definición rechazada no llega a MicrosoftRuleEngineAdapter.

## 10. Arquitectura

- Domain no referencia Infrastructure, Api, EF Core ni Microsoft RulesEngine.
- Application no referencia Infrastructure, Api, EF Core ni Microsoft RulesEngine.
- Solo Infrastructure referencia Microsoft RulesEngine.
- Tipos Workflow y Rule externos no aparecen en contratos públicos.
