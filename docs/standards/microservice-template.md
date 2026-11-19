
# Microservice Template Instructions
Se puede ejecutar inicialmente el comando dotnet new enterprise-microservice -n Products --force ya que contiene la plantilla de creacion de proyecto

{Microservice}.Api
{Microservice}.Application
{Microservice}.Domain
{Microservice}.Infrastructure

## Objetivo
Esta plantilla establece la arquitectura base
para todos los microservicios de la plataforma.

## Reglas obligatorias

- Mantener Clean Architecture.
- Respetar las dependencias entre capas.
- No introducir lógica de negocio en la plantilla.
- Utilizar inyección de dependencias.
- Mantener convenciones de nomenclatura.
- Implementar pruebas.
- Respetar los estándares de logging.
- No agregar dependencias innecesarias.

## Dependencias permitida
Api ───────────────► Application
Api ───────────────► Infrastructure

Application ───────► Domain

Infrastructure ────► Application
Infrastructure ────► Domain

Domain ────────────► Ningún otro proyecto


## Desarrollo asistido por IA

Antes de implementar una funcionalidad:

1. Leer el contexto del microservicio.
2. Leer las reglas globales.
3. Revisar las especificaciones.
4. Diseñar la solución.
5. Solicitar aprobación del diseño.
6. Implementar las tareas aprobadas.
7. Ejecutar pruebas.
8. Verificar reglas arquitectónicas.

