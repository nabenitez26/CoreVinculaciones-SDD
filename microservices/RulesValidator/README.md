# RulesValidator

Microservicio responsable de administrar y evaluar
reglas de negocio multinegocio.

## Stack

- .NET
- ASP.NET Core
- Azure SQL
- Entity Framework Core
- Microsoft RulesEngine
- xUnit

## Arquitectura

Clean Architecture:

- RulesValidator.Api
- RulesValidator.Application
- RulesValidator.Domain
- RulesValidator.Infrastructure

## Desarrollo

La implementación utiliza Spec-Driven Development.

Antes de desarrollar una funcionalidad revisar:

1. constitution.md
2. RuleValidator_Context.md
3. specs/{feature}/spec.md
4. specs/{feature}/clarifications.md
5. specs/{feature}/plan.md
6. specs/{feature}/tasks.md