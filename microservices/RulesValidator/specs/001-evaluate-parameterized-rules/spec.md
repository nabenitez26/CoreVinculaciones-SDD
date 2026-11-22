# Feature 001 - Evaluación de reglas parametrizables

## 1. Objetivo

Evaluar reglas parametrizables activas y vigentes para un negocio, proceso y etapa mediante un modelo neutral propio y Microsoft RulesEngine encapsulado en Infrastructure.

## 2. Endpoint

```http
POST /api/v1/evaluar-reglas
```

Esta es la única ruta aprobada para Feature 001.

## 3. Entrada

La solicitud contiene:

- `negocioId`, obligatorio y definido por el Core;
- `proceso`, obligatorio;
- `tipoPlanId`, opcional; se informa cuando la selección de reglas depende del tipo de plan;
- `etapa`, opcional; si se informa debe ser mayor que cero;
- `fechaEvaluacion`, opcional, ISO 8601 UTC;
- `contexto`, obligatorio.

`contexto` es un objeto JSON plano cuyas claves son aliases literales del catálogo y pueden contener puntos. El ejemplo del contrato usa `pension.tipo`, `pension.mesadaEnSmlmv`, `person.genero`, `person.edad` y `person.tipoDocumento`. `tipoPlanId` selecciona la configuración y no se repite como un campo del contexto.

`idEvaluacion` no se recibe del consumidor. RulesValidator lo genera para cada ejecución.

`X-Correlation-ID` es un header opcional. Si no llega, RulesValidator genera `idCorrelacion`.

Cuando `fechaEvaluacion` no se proporciona, Application usa el reloj inyectado y obtiene la fecha actual UTC. El repositorio no consulta directamente el reloj del sistema.

## 4. Convención del campo de mesada

El atributo de contexto se denomina:

```text
pension.mesada
```

`pension.mesada` contiene el valor en pesos y se mapea a `cfg.dato_requerido.DR-002`.

## 5. Selección de reglas globales y específicas

Un flujo global se identifica con `reglas.flujo_reglas.negocio_id = NULL`; no se crea un negocio GLOBAL duplicado en RulesValidator.

Para una solicitud de negocio `SALUD`, RulesValidator selecciona:

1. flujos activos globales (`negocio_id = NULL`) limitados únicamente al mismo proceso;
2. flujos activos de `SALUD` para el mismo proceso, etapa cuando fue informada y `tipo_plan_id` cuando fue informado.

Las reglas globales se evalúan primero. Después se evalúan las reglas específicas ordenadas por prioridad.

Una versión se selecciona solamente cuando:

```text
estado = ACTIVA
AND fecha_inicio_vigencia <= fechaEvaluacion
AND (
    fecha_fin_vigencia IS NULL
    OR fechaEvaluacion <= fecha_fin_vigencia
)
```

No se ejecutan versiones BORRADOR, EN_REVISION, APROBADA, INACTIVA, futuras o vencidas.

## 6. Modelo neutral

Azure SQL almacena `definicion_json` conforme a `RuleDefinition 1.0`.

Un valor numérico puede referenciar un parámetro Core con este objeto cerrado:

```json
{
    "tipo": "PARAMETRO_MULTIPLICADO",
    "parametro": "SMMLV",
    "factor": 1.5
}
```

Solo se permite para campos NUMERO y comparaciones escalares. No acepta expresiones ni nombres de operadores. El valor se resuelve en `cfg.parametro` por `parametro_id` y `fechaEvaluacion`; `vigencia = NULL` es atemporal y un año como `2026` aplica solo durante ese año. `valor` debe ser decimal invariante. Las versiones activas son inmutables: una definición nueva se publica incrementando la versión.

La raíz puede incluir opcionalmente `aplicabilidad`, como condición o grupo neutral con la misma estructura de nodos. Se evalúa antes de la condición principal. Si resulta falsa, la regla retorna `NO_APLICA`; si falta un campo requerido para evaluarla, retorna `INDETERMINADO` con `DATOS_INSUFICIENTES`.

No almacena:

- Workflow o Rule de Microsoft RulesEngine;
- expresiones C#;
- expresiones Dynamic LINQ;
- scripts o SQL ejecutable.

Toda definición debe superar JSON Schema y validación allow-list antes de llegar al Adapter.

## 7. Estados individuales

- `CUMPLE`: aplica y cumple.
- `NO_CUMPLE`: aplica y no cumple.
- `NO_APLICA`: no corresponde al contexto.
- `INDETERMINADO`: aplica, pero falta información necesaria.

Cuando el estado sea `NO_CUMPLE`, el resultado incluye severidad, código de error y mensaje.

Cuando sea `INDETERMINADO` por dato ausente:

- `codigoError = DATOS_INSUFICIENTES`;
- se incluye mensaje funcional;
- no se produce HTTP 500 ni excepción no controlada.

## 8. Consolidación de resultadoGeneral

La precedencia es:

1. Si existe al menos una regla `BLOQUEANTE` con `NO_CUMPLE`, `resultadoGeneral = NO_CUMPLE`.
2. En ausencia de incumplimiento bloqueante, si existe al menos una regla `INDETERMINADO`, `resultadoGeneral = INDETERMINADO`.
3. En los demás casos, incluidas combinaciones de `CUMPLE` y `NO_APLICA`, `resultadoGeneral = CUMPLE`.

Una regla `ADVERTENCIA` o `INFORMATIVA` con `NO_CUMPLE` no cambia por sí sola el resultado general a `NO_CUMPLE`, pero queda reportada en resultados.

## 9. Reglas iniciales

### RV-001 - Tipo de pensión reconocida

Campo: `pension.tipo`  
Valores: Vejez, Invalidez, Sobrevivientes  
Comparación: literal  
Severidad: BLOQUEANTE  
Código: `ERR-TIPO-PENSION`

### RV-101 - Mesada mínima

Campo: `pension.mesada`  
Condición: `pension.mesada >= 1 * SMMLV`  
Severidad: BLOQUEANTE  
Código: `ERR-MESADA-MINIMA`

### RV-002 - Tope TP-01

El flujo asociado selecciona RV-002 según `tipoPlanId`; el plan no se repite en el contexto.  
Condición: `pension.mesada <= 1.5 * SMMLV`  
Severidad: BLOQUEANTE  
Código: `ERR-MESADA-TP01`

### RV-003 - Mesada sobre el tope

Campo: `pension.mesada`  
Condición: `pension.mesada > 1.5 * SMMLV`

## 10. Definición activa inválida

Si una versión ACTIVA no supera validación:

- no se ejecuta Microsoft RulesEngine;
- se incrementa la métrica de definición rechazada;
- se registra telemetría sin exponer `definicion_json`;
- la evaluación finaliza con ProblemDetails HTTP 500 controlado.

## 11. Observabilidad

La respuesta 200 se limita al esquema OpenAPI e incluye:

- idEvaluacion generado;
- idCorrelacion;
- resultadoGeneral;
- fechaEvaluacion efectiva;
- resultados individuales.

Los resultados públicos individuales solo usan `CUMPLE`, `NO_CUMPLE` e `INDETERMINADO`, según el enum de `rules-api.yaml`. `NO_APLICA` se resuelve internamente y no se serializa como resultado público.

## 12. Fuera de alcance

- administración, aprobación o activación desde API;
- rollback;
- retroactividad;
- RV-104;
- handlers especializados;
- Service Bus;
- inconsistencias;
- notificaciones.
