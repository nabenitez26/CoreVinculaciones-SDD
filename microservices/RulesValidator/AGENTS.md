# Instrucciones para GitHub Copilot - RulesValidator

## Contexto obligatorio

Antes de modificar código, revisar:

1. ../../AGENTS.md
2. ../../docs/global/ARCHITECTURE.md
3. ../../docs/global/CODING_STANDARDS.md
4. ../../docs/standards/microservice-template.md
5. constitution.md
6. RuleValidator_Context.md
7. La especificación activa en /specs
8. El plan técnico y tasks.md asociados

## Reglas de desarrollo

- Implementar únicamente el alcance definido en la especificación activa.
- No agregar funcionalidades que no estén definidas en la Spec.
- Utilizar Clean Architecture.
- Domain no puede depender de Infrastructure, Application ni Api.
- Application no puede depender directamente de Azure SQL.
- Application y Domain no pueden depender directamente de Microsoft RulesEngine.
- Microsoft RulesEngine debe encapsularse mediante un Adapter en Infrastructure.
- No incluir lógica de negocio en Controllers.
- No almacenar código ejecutable dentro de las reglas.
- No permitir expresiones C# arbitrarias provenientes de consumidores o administradores.
- Las reglas activas son inmutables.
- Toda nueva funcionalidad debe incluir pruebas.
- Los errores de API deben utilizar ProblemDetails.
- No registrar información sensible completa en logs.

## Persistencia

- Utilizar Azure SQL.
- Mantener de forma relacional la información de gobierno:
  - negocio;
  - regla;
  - versión;
  - estado;
  - vigencia;
  - auditoría.
- Utilizar JSON únicamente donde la estructura de la regla requiera flexibilidad.

## Motor de reglas

Las reglas parametrizables serán evaluadas utilizando Microsoft RulesEngine.

Las reglas que requieran consultas especializadas, integración con sistemas externos
o comportamiento no expresable adecuadamente mediante Microsoft RulesEngine
deben implementarse mediante handlers especializados.

## Tipos de validación

El sistema distingue:

1. Reglas parametrizables.
2. Reglas especializadas.
3. Restricciones de integridad.

No convertir restricciones de integridad de base de datos en reglas parametrizadas.

## Convenciones

- seguir las convenciones de codigo ## Convenciones de nombres  del archivo ../../CODING_STANDARDS.md
- Documentación funcional: español.
- Mensajes funcionales: español.
- Identificadores técnicos de reglas: conservar nomenclatura oficial, por ejemplo RV-001.