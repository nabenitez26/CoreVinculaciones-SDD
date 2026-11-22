# Feature 001 - Evaluación de reglas parametrizables

## 1. Objetivo

Evaluar reglas parametrizables activas y vigentes para un negocio, proceso, tipo de plan y etapa mediante un modelo neutral propio.

RulesValidator encapsula Microsoft RulesEngine en Infrastructure y evita exponer expresiones ejecutables a consumidores o configuradores.

RulesValidator utiliza los catálogos existentes del Core como fuente de verdad para conceptos tales como:

- negocio;
- tipo de vinculación;
- tipo de contrato;
- tipo de plan;
- datos requeridos;
- parámetros;
- fuentes de datos.

RulesValidator no replica estos catálogos.

---

## 2. Endpoint

```http
POST /api/v1/evaluar-reglas
```

Esta es la única ruta aprobada para Feature 001.

---

## 3. Entrada

La solicitud contiene:

- `negocioId`, obligatorio y definido por el Core;
- `proceso`, obligatorio;
- `tipoPlanId`, opcional; se informa cuando la selección de reglas depende del tipo de plan;
- `etapa`, opcional; si se informa debe ser mayor que cero;
- `fechaEvaluacion`, opcional, ISO 8601 UTC;
- `contexto`, obligatorio.

`contexto` es un objeto JSON plano cuyas claves son aliases literales del catálogo y pueden contener puntos.

Para la Etapa 4 se utilizan, entre otros:

```text
pension.tipo
pension.mesada
persona.sexo
persona.edad
persona.tipoDocumento
```

Para la Etapa 5, RV-104 requiere:

```text
persona.numeroDocumento
```

`tipoPlanId` selecciona la configuración y no se repite como campo dentro de `contexto`.

`idEvaluacion` no se recibe del consumidor. RulesValidator lo genera para cada ejecución.

`X-Correlation-ID` es un header opcional. Si no llega, RulesValidator genera `idCorrelacion`.

Cuando `fechaEvaluacion` no se proporciona, Application usa el reloj inyectado y obtiene la fecha actual UTC.

El repositorio no consulta directamente el reloj del sistema.

---

## 4. Datos del Core y aliases del motor

RulesValidator utiliza `campo_regla` como capa de mapeo y allow-list entre los datos funcionales del Core y los aliases empleados por `RuleDefinition`.

Mappings actualmente identificados:

```text
DR-001
TIPO_DE_PENSION_RECONOCIDA
        ↓
pension.tipo


DR-002
MESADA_PENSIONAL
        ↓
pension.mesada


DR-017
Tipo de documento
        ↓
persona.tipoDocumento
```

`cfg.dato_requerido` continúa siendo la fuente de verdad sobre la definición funcional del dato.

`reglas.campo_regla.tipo_valor` define cómo RulesValidator interpreta el valor durante la evaluación.

Ambos conceptos son independientes.

Por ejemplo:

```text
cfg.dato_requerido
DR-002
tipo_dato = TEXTO

reglas.campo_regla
codigo = pension.mesada
tipo_valor = NUMERO
```

RulesValidator puede normalizar el dato como número sin modificar la definición existente en el Core.

---

## 5. Convención del campo de mesada

El atributo de contexto se denomina:

```text
pension.mesada
```

`pension.mesada` contiene el valor monetario de la mesada en pesos.

Ejemplo:

```json
{
  "pension.mesada": 2100000
}
```

El consumidor no convierte la mesada previamente a SMLMV.

El campo se mapea a:

```text
cfg.dato_requerido.DR-002
```

Las reglas que requieren comparar la mesada contra SMMLV utilizan dinámicamente el parámetro Core:

```text
cfg.parametro
parametro_id = SMMLV
```

Para los escenarios actuales:

```text
SMMLV = 1750000
vigencia = 2026
```

El valor no se hardcodea dentro de RulesValidator.

---

## 6. Selección de reglas globales y específicas

Un flujo global se identifica con:

```text
reglas.flujo_reglas.negocio_id = NULL
```

No se crea un negocio `GLOBAL` duplicado en RulesValidator.

Para una solicitud de un negocio específico, RulesValidator puede seleccionar:

1. flujos globales activos aplicables al proceso;
2. flujos activos específicos del negocio;
3. coincidencia de `tipo_plan_id` cuando se informa;
4. coincidencia de `etapa` cuando se informa.

Las reglas globales se evalúan primero.

Después se evalúan las reglas específicas ordenadas por prioridad.

Una versión se selecciona solamente cuando:

```text
estado = ACTIVA
AND fecha_inicio_vigencia <= fechaEvaluacion
AND (
    fecha_fin_vigencia IS NULL
    OR fechaEvaluacion <= fecha_fin_vigencia
)
```

No se ejecutan versiones:

```text
BORRADOR
EN_REVISION
APROBADA
INACTIVA
futuras
vencidas
```

---

## 7. Modelo neutral RuleDefinition

Azure SQL almacena `definicion_json` conforme a `RuleDefinition 1.0`.

Una condición simple utiliza:

```json
{
  "versionEsquema": "1.0",
  "tipo": "CONDICION",
  "campo": "pension.tipo",
  "operador": "IGUAL",
  "valor": "Vejez"
}
```

Los operadores neutrales permitidos son:

```text
IGUAL
DIFERENTE
MAYOR
MAYOR_IGUAL
MENOR
MENOR_IGUAL
EN
NO_EN
```

Los grupos lógicos utilizan:

```text
Y
O
```

No se utilizan:

```text
AND
OR
```

dentro del formato neutral.

Los grupos pueden contener recursivamente condiciones y otros grupos.

Ejemplo:

```json
{
  "versionEsquema": "1.0",
  "tipo": "GRUPO",
  "operadorLogico": "Y",
  "condiciones": []
}
```

---

## 8. Parámetros Core en RuleDefinition

Un valor numérico puede referenciar un parámetro Core mediante un objeto cerrado:

```json
{
  "tipo": "PARAMETRO_MULTIPLICADO",
  "parametro": "SMMLV",
  "factor": 1.5
}
```

Solo se permite:

- sobre campos `NUMERO`;
- con operadores escalares compatibles;
- con un parámetro permitido;
- con un factor numérico válido mayor que cero.

No acepta expresiones arbitrarias.

El parámetro se resuelve desde:

```text
cfg.parametro
```

utilizando:

```text
parametro_id
fechaEvaluacion
vigencia
```

Una vigencia:

```text
NULL
```

es atemporal.

Una vigencia:

```text
2026
```

aplica únicamente durante 2026.

El valor configurado debe ser convertible a decimal invariante.

El cálculo ocurre antes de construir la expresión para Microsoft RulesEngine:

```text
valorResuelto = valorParametro × factor
```

Ejemplo:

```text
SMMLV = 1.750.000
factor = 1.5

valorResuelto = 2.625.000
```

Microsoft RulesEngine recibe el valor numérico ya resuelto.

Las versiones activas son inmutables. Una definición nueva debe publicarse incrementando la versión.

---

## 9. Aplicabilidad

La raíz de una definición puede incluir opcionalmente `aplicabilidad`.

`aplicabilidad` utiliza la misma estructura neutral de condiciones y grupos.

Se evalúa antes de la condición principal.

Si la aplicabilidad resulta falsa:

```text
NO_APLICA
```

Si falta un campo requerido para determinarla:

```text
INDETERMINADO
DATOS_INSUFICIENTES
```

`NO_APLICA` es un estado interno del motor y no se serializa como resultado público.

---

## 10. Seguridad del modelo neutral

`definicion_json` no almacena:

- Workflow de Microsoft RulesEngine;
- Rule de Microsoft RulesEngine;
- expresiones C#;
- expresiones Dynamic LINQ;
- scripts;
- SQL ejecutable;
- URI externas;
- nombres de contenedores;
- nombres físicos de blobs;
- credenciales;
- tokens SAS.

Toda definición debe superar:

```text
JSON Schema
      ↓
allow-list de campos
      ↓
allow-list de operadores
      ↓
validación de tipos
      ↓
validación de complejidad
      ↓
resolución controlada de parámetros
      ↓
Adapter
      ↓
Microsoft RulesEngine
```

---

## 11. Estados individuales

Los estados internos son:

- `CUMPLE`: aplica y cumple;
- `NO_CUMPLE`: aplica y no cumple;
- `NO_APLICA`: no corresponde al contexto;
- `INDETERMINADO`: aplica, pero falta información o una dependencia necesaria para determinar el resultado.

Cuando el estado sea `NO_CUMPLE`, el resultado incluye:

- severidad;
- código de error;
- mensaje.

Cuando sea `INDETERMINADO` por dato ausente:

```text
codigoError = DATOS_INSUFICIENTES
```

Debe incluir un mensaje funcional y no debe producir HTTP 500.

---

## 12. Consolidación de resultadoGeneral

La precedencia es:

1. Si existe al menos una regla `BLOQUEANTE` con `NO_CUMPLE`:

```text
resultadoGeneral = NO_CUMPLE
```

2. En ausencia de incumplimiento bloqueante, si existe al menos una regla `INDETERMINADO`:

```text
resultadoGeneral = INDETERMINADO
```

3. En los demás casos:

```text
resultadoGeneral = CUMPLE
```

Esto incluye combinaciones de:

```text
CUMPLE
NO_APLICA
```

Una regla `ADVERTENCIA` o `INFORMATIVA` con `NO_CUMPLE` no cambia por sí sola el resultado general a `NO_CUMPLE`, pero permanece reportada en los resultados individuales.

---

# 13. Reglas Etapa 4

## RV-001 - Tipo de pensión reconocida

Dato Core:

```text
DR-001
TIPO_DE_PENSION_RECONOCIDA
```

Campo:

```text
pension.tipo
```

Valores permitidos:

```text
Vejez
Invalidez
Sobrevivientes
```

Comparación:

```text
EN
```

Severidad:

```text
BLOQUEANTE
```

Código:

```text
ERR-TIPO-PENSION
```

---

## RV-002 - Tope TP-01

El flujo asociado selecciona RV-002 según:

```text
tipoPlanId = TP-01
```

El plan no se repite dentro de `contexto`.

Dato Core:

```text
DR-002
MESADA_PENSIONAL
```

Campo:

```text
pension.mesada
```

Condición:

```text
pension.mesada <= 1.5 × SMMLV
```

RuleDefinition:

```json
{
  "versionEsquema": "1.0",
  "tipo": "CONDICION",
  "campo": "pension.mesada",
  "operador": "MENOR_IGUAL",
  "valor": {
    "tipo": "PARAMETRO_MULTIPLICADO",
    "parametro": "SMMLV",
    "factor": 1.5
  }
}
```

Severidad:

```text
BLOQUEANTE
```

Código:

```text
ERR-MESADA-TP01
```

---

## RV-003 - Mesada sobre el tope TP-02

El flujo asociado selecciona RV-003 según:

```text
tipoPlanId = TP-02
```

Dato Core:

```text
DR-002
MESADA_PENSIONAL
```

Campo:

```text
pension.mesada
```

Condición:

```text
pension.mesada > 1.5 × SMMLV
```

RuleDefinition:

```json
{
  "versionEsquema": "1.0",
  "tipo": "CONDICION",
  "campo": "pension.mesada",
  "operador": "MAYOR",
  "valor": {
    "tipo": "PARAMETRO_MULTIPLICADO",
    "parametro": "SMMLV",
    "factor": 1.5
  }
}
```

Severidad:

```text
BLOQUEANTE
```

---

## RV-101 - Mesada mínima

Dato Core:

```text
DR-002
MESADA_PENSIONAL
```

Campo:

```text
pension.mesada
```

Condición:

```text
pension.mesada >= 1 × SMMLV
```

RuleDefinition:

```json
{
  "versionEsquema": "1.0",
  "tipo": "CONDICION",
  "campo": "pension.mesada",
  "operador": "MAYOR_IGUAL",
  "valor": {
    "tipo": "PARAMETRO_MULTIPLICADO",
    "parametro": "SMMLV",
    "factor": 1.0
  }
}
```

Severidad:

```text
BLOQUEANTE
```

Código:

```text
ERR-MESADA-MINIMA
```

---

## RV-102 - Edad mínima para pensión de vejez

Utiliza:

```text
pension.tipo
persona.sexo
persona.edad
```

Condición funcional:

```text
pension.tipo <> "Vejez"

O

(
    persona.sexo = "Femenino"
    Y
    persona.edad >= 57
)

O

(
    persona.sexo = "Masculino"
    Y
    persona.edad >= 62
)
```

RuleDefinition:

```json
{
  "versionEsquema": "1.0",
  "tipo": "GRUPO",
  "operadorLogico": "O",
  "condiciones": [
    {
      "tipo": "CONDICION",
      "campo": "pension.tipo",
      "operador": "DIFERENTE",
      "valor": "Vejez"
    },
    {
      "tipo": "GRUPO",
      "operadorLogico": "Y",
      "condiciones": [
        {
          "tipo": "CONDICION",
          "campo": "persona.sexo",
          "operador": "IGUAL",
          "valor": "Femenino"
        },
        {
          "tipo": "CONDICION",
          "campo": "persona.edad",
          "operador": "MAYOR_IGUAL",
          "valor": 57
        }
      ]
    },
    {
      "tipo": "GRUPO",
      "operadorLogico": "Y",
      "condiciones": [
        {
          "tipo": "CONDICION",
          "campo": "persona.sexo",
          "operador": "IGUAL",
          "valor": "Masculino"
        },
        {
          "tipo": "CONDICION",
          "campo": "persona.edad",
          "operador": "MAYOR_IGUAL",
          "valor": 62
        }
      ]
    }
  ]
}
```

Severidad:

```text
BLOQUEANTE
```

Código:

```text
ERR-RV-102
```

---

## RV-103 - Tipo de documento permitido

Dato Core:

```text
DR-017
```

Campo:

```text
persona.tipoDocumento
```

Valores:

```text
CC
CE
PEP
PA
```

RuleDefinition:

```json
{
  "versionEsquema": "1.0",
  "tipo": "CONDICION",
  "campo": "persona.tipoDocumento",
  "operador": "EN",
  "valor": [
    "CC",
    "CE",
    "PEP",
    "PA"
  ]
}
```

Severidad:

```text
BLOQUEANTE
```

Código:

```text
ERR-RV-103
```

---

# 14. Etapa 5 - Confirmación externa de pensión

La Etapa 5 confirma externamente la pensión mediante RV-104.

La fuente se identifica en el catálogo Core mediante:

```text
ref.fuente_dato
fuente_dato_id = FD-PENS
```

Configuración funcional:

```text
FD-PENS
Fuente externa de pensionados (simulada en la POC)
Fuente externa de datos
guarda_documento = 0
```

`FD-PENS` no reemplaza ni modifica `DR-001`.

La diferencia es:

```text
DR-001
TIPO_DE_PENSION_RECONOCIDA
forma_captura = DECLARADO
        ↓
Etapa 4
        ↓
RV-001 / RV-102
```

mientras:

```text
persona.numeroDocumento
        ↓
FD-PENS
        ↓
confirmación externa
        ↓
Etapa 5
        ↓
RV-104
```

---

## 15. Fuente física FD-PENS

La fuente física para la POC se encuentra en Azure Blob Storage.

Nombre lógico del archivo:

```text
fuente_externa_simulada
```

El nombre físico puede incluir la extensión configurada, por ejemplo:

```text
fuente_externa_simulada.csv
```

Si el artefacto físico es realmente `.xlsx`, la implementación deberá utilizar el lector correspondiente.

No se debe asumir formato Excel únicamente por el nombre funcional.

Para el formato delimitado inicial, el contenido esperado es:

```text
NUMERO_DOCUMENTO;RESPUESTA
52310057;Reconocida
52310103;Reconocida
52310122;Reconocida
```

Las columnas obligatorias son:

```text
NUMERO_DOCUMENTO
RESPUESTA
```

`NUMERO_DOCUMENTO` debe manejarse como texto para preservar cualquier cero inicial.

---

## 16. Configuración Azure Blob Storage

La ubicación física de FD-PENS pertenece exclusivamente a Infrastructure.

Ejemplo conceptual de configuración de infraestructura:

```text
FuentesExternas:
  Pensionados:
    FuenteId: FD-PENS
    BlobServiceUri: <cuenta>
    ContainerName: <contenedor>
```

Variables equivalentes:

```text
FuentesExternas__Pensionados__FuenteId
FuentesExternas__Pensionados__BlobServiceUri
FuentesExternas__Pensionados__ContainerName
```

El nombre del blob, el separador, las columnas y los campos del contexto se leen de
`reglas.fuente_externa_mapeo` (ver data-model, sección 11).

La cuenta o URI de Storage debe obtenerse desde configuración de infraestructura.

No se permiten:

- secretos en código;
- credenciales en RuleDefinition;
- SAS tokens en RuleDefinition;
- BlobUri en RuleDefinition.

En Azure se debe utilizar identidad administrada.

---

# 17. Request Etapa 5

El consumidor proporciona el número de documento:

```json
{
  "negocioId": "CAJA",
  "proceso": "VINCULACION",
  "tipoPlanId": "TP-02",
  "etapa": 5,
  "contexto": {
    "persona.numeroDocumento": "52310057"
  }
}
```

El consumidor no proporciona:

```text
fuente.pension.respuesta
```

RulesValidator obtiene ese valor desde FD-PENS.

---

# 18. Enriquecimiento del contexto

Antes de evaluar RV-104, RulesValidator consulta FD-PENS mediante:

```text
persona.numeroDocumento
```

Si encuentra:

```text
52310057;Reconocida
```

crea un contexto interno enriquecido con:

```json
{
  "persona.numeroDocumento": "52310057",
  "fuente.pension.respuesta": "Reconocida"
}
```

El contexto enriquecido se utiliza únicamente durante la evaluación.

El contexto original enviado por el consumidor no se modifica de manera insegura.

---

# 19. RV-104 - Pensión reconocida por fuente externa

Etapa:

```text
5
```

Fuente:

```text
FD-PENS
```

Campo:

```text
fuente.pension.respuesta
```

Condición:

```text
fuente.pension.respuesta = "Reconocida"
```

RuleDefinition:

```json
{
  "versionEsquema": "1.0",
  "tipo": "CONDICION",
  "campo": "fuente.pension.respuesta",
  "operador": "IGUAL",
  "valor": "Reconocida"
}
```

Severidad:

```text
ADVERTENCIA
```

RV-104 continúa siendo:

```text
PARAMETRIZABLE
```

La obtención del valor externo ocurre antes de la evaluación de RuleDefinition.

La integración con Azure Blob Storage no forma parte de ExpressionBuilder ni de RuleDefinition.

---

# 20. Resultados de la fuente externa

La consulta a FD-PENS maneja los siguientes estados conceptuales:

```text
ENCONTRADO
SIN_RESPUESTA
NO_DISPONIBLE
INVALIDO
```

## Encontrado

Si el documento existe y:

```text
RESPUESTA = Reconocida
```

RV-104 retorna:

```text
CUMPLE
```

---

## Sin respuesta

Si:

- el documento no existe;
- la respuesta está vacía;
- no existe información suficiente para determinar la confirmación;

RV-104 retorna:

```text
INDETERMINADO
```

con severidad:

```text
ADVERTENCIA
```

No debe convertirse en `NO_CUMPLE`.

---

## Fuente no disponible

Si Azure Blob Storage o el blob de FD-PENS no pueden consultarse:

```text
RV-104 = INDETERMINADO
```

con:

```text
severidad = ADVERTENCIA
```

La indisponibilidad de la fuente no se considera evidencia de que la pensión no esté reconocida.

---

## Fuente inválida

Si el archivo presenta condiciones que impiden interpretarlo de forma segura, por ejemplo:

- encabezados faltantes;
- estructura inválida;
- múltiples registros contradictorios para el mismo documento;

la evaluación debe tratarse de forma controlada.

No debe ejecutarse una comparación utilizando información ambigua.

---

# 21. Responsabilidad del Core después de RV-104

RulesValidator no:

- actualiza `trx.paso_tramite`;
- activa vinculaciones;
- rechaza solicitudes;
- crea casos;
- crea `TC-03`;
- modifica el estado de la Etapa 5.

RulesValidator únicamente devuelve el resultado de evaluación.

Para el escenario funcional:

```text
RV-104
INDETERMINADO
ADVERTENCIA
```

el Core/orquestador es responsable de aplicar:

```text
Etapa 5 = EN ESPERA
Vinculación continúa
Caso TC-03
```

---

# 22. Persistencia y trazabilidad

RulesValidator registra la evaluación en:

```text
reglas.ejecucion_regla
```

Debe permitir conocer:

- `idEvaluacion`;
- negocio;
- proceso;
- tipo de plan;
- etapa;
- flujos utilizados;
- resultados;
- fecha de evaluación;
- fecha de ejecución;
- duración;
- correlación;
- traza.

No debe almacenar:

- número de documento;
- contenido completo de FD-PENS;
- archivo descargado;
- contexto completo;
- PII innecesaria.

Las tablas Core de trazabilidad de fuentes externas continúan siendo responsabilidad del Core.

---

# 23. Seguridad de fuentes externas

RulesValidator debe cumplir:

- no registrar `persona.numeroDocumento` en logs;
- no persistir números de documento en `reglas.ejecucion_regla`;
- no guardar localmente el blob de forma permanente;
- no registrar el contenido completo de la fuente;
- no incluir BlobUri en RuleDefinition;
- no incluir credenciales en RuleDefinition;
- no permitir que una regla indique dinámicamente qué URI consultar;
- mantener Azure Storage exclusivamente en Infrastructure;
- usar configuración externa;
- utilizar identidad administrada en Azure;
- propagar `CancellationToken` durante la consulta.

---

# 24. Definición activa inválida

Si una versión ACTIVA no supera validación:

- no se ejecuta Microsoft RulesEngine;
- se incrementa la métrica de definición rechazada;
- se registra telemetría sin exponer `definicion_json`;
- la evaluación finaliza con ProblemDetails HTTP 500 controlado.

Una definición inválida es diferente de:

```text
INDETERMINADO
```

`INDETERMINADO` representa una evaluación válida cuyo resultado no puede determinarse por falta de información o una dependencia requerida.

---

# 25. Observabilidad

La respuesta HTTP 200 se limita al esquema OpenAPI e incluye:

- `idEvaluacion`;
- `idCorrelacion`;
- `resultadoGeneral`;
- `fechaEvaluacion`;
- resultados individuales.

Los resultados públicos individuales utilizan:

```text
CUMPLE
NO_CUMPLE
INDETERMINADO
```

`NO_APLICA` es interno y no se serializa como resultado público.

La telemetría de fuentes externas puede registrar:

- `fuenteId`;
- duración;
- estado de la consulta;
- disponibilidad;
- correlación.

No debe registrar el número de documento consultado.

---

# 26. Escenarios funcionales validados

## Caso A

```text
Negocio: CAJA
Plan: TP-01
Pensión: Vejez
Mesada: 2.100.000
Sexo: Femenino
Edad: 66
Documento: CC
```

Con:

```text
SMMLV = 1.750.000
```

Etapa 4:

```text
RV-001 CUMPLE
RV-002 CUMPLE
RV-101 CUMPLE
RV-102 CUMPLE
RV-103 CUMPLE

resultadoGeneral = CUMPLE
```

---

## Caso B

```text
Negocio: CAJA
Plan: TP-01
Pensión: Vejez
Mesada: 1.900.000
Sexo: Femenino
Edad: 55
Documento: CC
```

Etapa 4:

```text
RV-001 CUMPLE
RV-002 CUMPLE
RV-101 CUMPLE
RV-102 NO_CUMPLE
RV-103 CUMPLE

resultadoGeneral = NO_CUMPLE
```

El incumplimiento corresponde a la edad mínima de RV-102.

---

## Caso C

```text
Negocio: CAJA
Plan: TP-02
Pensión: Vejez
Mesada: 4.200.000
Sexo: Masculino
Edad: 68
Documento: CC
```

Etapa 4:

```text
RV-001 CUMPLE
RV-003 CUMPLE
RV-101 CUMPLE
RV-102 CUMPLE
RV-103 CUMPLE

resultadoGeneral = CUMPLE
```

Etapa 5:

```text
FD-PENS
        ↓
Sin respuesta
        ↓
RV-104 INDETERMINADO
ADVERTENCIA
```

El Core/orquestador debe interpretar este resultado para:

```text
Etapa 5 = EN ESPERA
Vinculación continúa
Caso TC-03
```

---

# 27. Fuera de alcance

Quedan fuera de Feature 001:

- administración de reglas desde API;
- aprobación de reglas desde API;
- activación de reglas desde API;
- rollback administrativo;
- retroactividad;
- Service Bus;
- notificaciones;
- creación directa de casos desde RulesValidator;
- actualización de estados del trámite desde RulesValidator;
- creación o modificación de vinculaciones desde RulesValidator;
- persistencia del contenido completo de fuentes externas;
- almacenamiento de archivos externos;
- selección dinámica de URI desde RuleDefinition;
- almacenamiento de credenciales dentro de reglas;
- lógica de negocio del Caso TC-03 dentro de RulesValidator.