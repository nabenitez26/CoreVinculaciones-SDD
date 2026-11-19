# Constitución - RulesValidator

## 1. Propósito

RulesValidator es responsable de administrar y evaluar reglas de negocio
para múltiples negocios.

El microservicio debe proporcionar una capacidad transversal de evaluación
sin incorporar lógica específica de los procesos consumidores.

---

## 2. Arquitectura

RulesValidator utilizará Clean Architecture y estará compuesto por:

- RulesValidator.Api
- RulesValidator.Application
- RulesValidator.Domain
- RulesValidator.Infrastructure

Dependencias permitidas:

Api -> Application
Api -> Infrastructure
Application -> Domain
Infrastructure -> Application
Infrastructure -> Domain

Domain no dependerá de:

- Application
- Infrastructure
- Api
- Entity Framework Core
- Microsoft RulesEngine

Application no dependerá de:

- Infrastructure
- Api
- Entity Framework Core
- Microsoft RulesEngine

Microsoft RulesEngine deberá estar encapsulado exclusivamente
en Infrastructure mediante un Adapter.

---

## 3. Persistencia

Azure SQL será la fuente persistente de RulesValidator.

La información estructural y de gobierno se almacenará
relacionalmente:

- negocio
- flujo
- regla
- versión
- estado
- vigencia
- severidad
- auditoría
- ejecuciones

Las definiciones variables de las reglas podrán almacenarse
como JSON utilizando exclusivamente el modelo declarativo neutral
definido por RulesValidator.

La base de datos no almacenará como fuente funcional:

- Workflow serializados de Microsoft RulesEngine;
- Rule serializados de Microsoft RulesEngine;
- expresiones C# arbitrarias;
- expresiones Dynamic LINQ arbitrarias;
- objetos propietarios de librerías externas.

---

## 4. Modelo neutral de reglas

RulesValidator tendrá un modelo declarativo propio e independiente
del motor utilizado para ejecutar reglas.

El modelo inicial permitirá:

Operadores:

- IGUAL
- DIFERENTE
- MAYOR
- MAYOR_IGUAL
- MENOR
- MENOR_IGUAL
- EN
- NO_EN

Operadores lógicos:

- Y
- O

Una definición podrá contener:

- condición;
- grupo de condiciones;
- campo;
- operador;
- valor.

El modelo declarativo será versionado.

La versión inicial será:

1.0

---

## 5. Microsoft RulesEngine

Las reglas parametrizables serán ejecutadas mediante Microsoft RulesEngine.

MicrosoftRuleEngineAdapter será el único componente autorizado
para traducir RuleDefinition al modelo requerido por Microsoft RulesEngine.

Domain y Application no conocerán:

- Workflow de Microsoft RulesEngine;
- Rule de Microsoft RulesEngine;
- expresiones Dynamic LINQ;
- ParsingConfig;
- CustomTypes;
- configuración interna del motor.

No se implementará lógica como:

if ruleId == "RV-001"

para reglas parametrizables.

---

## 6. Seguridad de definiciones

Toda definición recuperada desde Azure SQL será considerada
no confiable hasta superar la validación.

RulesValidator utilizará un modelo de seguridad allow-list.

Se validarán:

- versión del esquema;
- campos permitidos;
- operadores permitidos;
- compatibilidad campo/operador;
- tipos de valores;
- operadores lógicos;
- propiedades admitidas;
- profundidad del árbol;
- cantidad de condiciones.

No se permitirá que consumidores o administradores proporcionen
directamente expresiones C# o Dynamic LINQ.

La seguridad no dependerá exclusivamente de:

- regex;
- sanitización textual;
- listas de palabras prohibidas.

---

## 7. Seguridad de ejecución

MicrosoftRuleEngineAdapter contendrá un ExpressionBuilder controlado.

ExpressionBuilder únicamente recibirá una RuleDefinition previamente validada.

ExpressionBuilder será el único componente autorizado para construir
la expresión requerida por Microsoft RulesEngine.

No recibirá expresiones desde:

- API;
- Azure SQL;
- headers;
- parámetros de consumidores;
- configuración administrativa.

No se habilitarán capacidades que permitan:

- Reflection;
- System.IO;
- ejecución de procesos;
- acceso arbitrario a red;
- acceso a variables de entorno;
- carga dinámica de assemblies;
- invocación arbitraria de métodos;
- ejecución de scripts.

CustomTypes y AutoRegisterInputType deberán configurarse explícitamente
en MicrosoftRuleEngineAdapter.

No se registrarán tipos, métodos o helpers adicionales sin:

- especificación;
- revisión de seguridad;
- pruebas.

---

## 8. Tipos de validación

RulesValidator reconoce tres categorías:

1. Reglas parametrizables.
2. Reglas especializadas.
3. Restricciones de integridad.

Las reglas parametrizables utilizan el modelo neutral.

Las reglas especializadas utilizan handlers explícitos.

Las restricciones de integridad pertenecen