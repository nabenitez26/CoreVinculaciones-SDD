# Rules Validator Context

## Responsabilidad

Rules Validator es responsable de:

- almacenar metadatos de reglas;
- administrar versiones;
- recuperar reglas activas y vigentes;
- evaluar workflows;
- ejecutar reglas parametrizables;
- delegar reglas especializadas;
- devolver resultados normalizados;
- registrar la versión utilizada.

## Fuera del alcance

Rules Validator no es responsable de:

- crear una vinculación;
- cambiar el estado de una vinculación;
- crear directamente una inconsistencia;
- conceder o retirar beneficios;
- enviar notificaciones;
- garantizar restricciones de integridad de otros dominios;
- revertir operaciones históricas.

## Clasificación

### Parametrizables

Evaluadas con Microsoft RulesEngine:

- RV-001
- RV-002
- RV-003
- RV-101
- RV-102
- RV-103

### Especializadas

Implementadas mediante handlers:

- RV-104, validación de fuente externa.
- Pensionado con vinculación TC-01 vigente.

### Integridad

Implementadas por el servicio propietario y su base:

- Titular + Contratante + Plan vigente único.