
# Arquitectura global
API Management
      │
      ▼
Core Vinculacion Service
      │ síncrono
      ▼
Rules Validator
      │
      ▼
Azure SQL


## Patrón arquitectónico

Todos los microservicios deben seguir
Clean Architecture.

## Estructura obligatoria

Cada microservicio tendrá:

- Api
- Application
- Domain
- Infrastructure

## Responsabilidades

### Api
- Controllers o endpoints.
- Configuración HTTP.
- Autenticación y autorización.
- Validación de entrada.

### Application
- Casos de uso.
- Interfaces de servicios externos.
- DTOs.
- Orquestación de operaciones.

### Domain
- Entidades.
- Value Objects.
- Agregados.
- Domain Services.
- Domain Events.
- Reglas de negocio.

### Infrastructure
- Entity Framework Core.
- Repositorios.
- Integraciones externas.
- Azure Service Bus.
- Implementaciones técnicas.

## Reglas de dependencia

- Domain no depende de otras capas.
- Application depende de Domain.
- Infrastructure implementa abstracciones
  definidas por las capas internas.
- Api consume Application.
- Las dependencias externas deben
  resolverse mediante inyección de dependencias.

## Reglas de independencia

- Cada microservicio administra su dominio.
- No compartir bases de datos entre servicios.
- La comunicación entre servicios debe
  realizarse mediante contratos definidos.
- Evitar dependencias directas entre dominios.


