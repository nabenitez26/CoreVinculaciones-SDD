# Feature 001 - Evaluación de reglas parametrizables

## 1. Objetivo

Evaluar reglas parametrizables activas y vigentes para un negocio, proceso y etapa mediante un modelo neutral propio y Microsoft RulesEngine encapsulado en Infrastructure.

## 2. Endpoint

```http
POST /api/v1/evaluaciones-reglas
```

Esta es la única ruta aprobada para Feature 001.

## 3. Entrada

La solicitud contiene:

- `negocio`, obligatorio;
- `proceso`, obligatorio;
- `tipoPlanId`, identificador varchar de hasta 10 caracteres del plan asociado al tipo de vinculación, obligatorio cuando `proceso = VINCULACION`;
- `etapa`, obligatoria y mayor que cero;
- `fechaEvaluacion`, opcional, ISO 8601 UTC;
- `contexto`, obligatorio.

`contexto` es un objeto JSON plano. Sus claves son nombres funcionales en español, son rutas literales del catálogo dinámico y pueden contener puntos; no se representan como objetos anidados. Por ejemplo: `"pension.tipo"`, `"pension.mesadaEnSmlmv"`, `"persona.genero"`, `"persona.edad"`, `"persona.tipoDocumento"` y `"plan.codigo"`.

`idEvaluacion` no se recibe del consumidor. RulesValidator lo genera para cada ejecución.

`X-Correlation-ID` es un header opcional. Si no llega, RulesValidator genera `idCorrelacion`.

Cuando `fechaEvaluacion` no se proporciona, Application usa el reloj inyectado y obtiene la fecha actual UTC. El repositorio no consulta directamente el reloj del sistema.

## 4. Convención del campo de mesada

El atributo funcional se denomina en todos los artefactos:

```text
pension.mesadaEnSmlmv
```

No se permiten como sinónimos técnicos `AllowanceInSmlmv`, `subsidioEnSmlmv` ni `mesadaSmlmv`.

## 5. Selección de reglas globales y específicas

Existe un registro especial en `negocio` con `codigo = GLOBAL`.

Para una solicitud de negocio `SALUD`, RulesValidator selecciona:

1. flujos activos de GLOBAL limitados únicamente al mismo proceso;
2. flujos activos de SALUD para el mismo proceso y etapa, y el mismo `tipo_plan_id` si el proceso es VINCULACION.

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

Campo: `pension.mesadaEnSmlmv`  
Condición: mayor o igual a 1  
Severidad: BLOQUEANTE  
Código: `ERR-MESADA-MINIMA`

### RV-002 - Tope TP-01

Aplicabilidad: `plan.codigo = TP-01`  
Condición: `pension.mesadaEnSmlmv <= 1.5`  
Severidad: BLOQUEANTE  
Código: `ERR-MESADA-TP01`

Si `plan.codigo` no es TP-01, RV-002 retorna `NO_APLICA`.

Su definición neutral utiliza `aplicabilidad` para `plan.codigo = TP-01` y la condición principal para el tope de mesada. La condición principal no se evalúa cuando la regla no aplica.

## 10. Definición activa inválida

Si una versión ACTIVA no supera validación:

- no se ejecuta Microsoft RulesEngine;
- se incrementa la métrica de definición rechazada;
- se registra telemetría sin exponer `definicion_json`;
- la evaluación finaliza con ProblemDetails HTTP 500 controlado.

## 11. Observabilidad

La respuesta 200 incluye:

- idEvaluacion generado;
- idCorrelacion;
- duracionMs;
- resultadoGeneral;
- flujosReglas;
- resultados individuales.

## 12. Fuera de alcance

- administración, aprobación o activación desde API;
- rollback;
- retroactividad;
- RV-104;
- handlers especializados;
- Service Bus;
- inconsistencias;
- notificaciones.
