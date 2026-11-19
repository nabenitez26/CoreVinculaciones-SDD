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

## Política de ejecución de pruebas

Durante la implementación incremental, GitHub Copilot NO debe
ejecutar automáticamente proyectos de pruebas.

No ejecutar de forma automática:

- dotnet test
- tests unitarios
- tests de integración
- Architecture Tests
- Contract Tests
- Security Tests
- cobertura de código
- suites completas de regresión

Los proyectos de pruebas deben mantenerse y podrán ser modificados
cuando la tarea activa lo requiera, pero no deberán ejecutarse
automáticamente.

Durante las tareas de implementación se permite ejecutar únicamente:

dotnet restore
dotnet build

y únicamente cuando sea necesario validar compilación.

Las pruebas se ejecutarán solamente cuando:

1. una tarea solicite explícitamente ejecutar pruebas;
2. el usuario solicite explícitamente ejecutar pruebas;
3. se alcance el gate final de validación de la Feature.

Copilot no debe ejecutar pruebas como paso adicional para
"verificar", "asegurar", "validar" o "confirmar" una implementación
si la tarea activa no lo exige explícitamente.

Al terminar cada tarea debe:

1. indicar los archivos modificados;
2. indicar si la compilación fue ejecutada;
3. indicar qué pruebas corresponderían a la tarea;
4. NO ejecutar dichas pruebas;
5. detenerse