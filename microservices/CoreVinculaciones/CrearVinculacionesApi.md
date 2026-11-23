# Construcción de estructura base – Microservicio de Vinculaciones (.NET)

## 1. Objetivo

Necesito incorporar un nuevo microservicio llamado **Vinculaciones**, desarrollado en .NET, siguiendo los principios de Clean Architecture y separación de responsabilidades.

El objetivo de esta tarea es exclusivamente crear la estructura inicial de proyectos, referencias y carpetas necesarias para organizar el desarrollo futuro.

**IMPORTANTE: No debes implementar funcionalidades, operaciones, lógica de negocio ni código interno. Únicamente debes crear el esqueleto estructural de la solución.**

## 2. Primera instrucción: analizar antes de modificar

Antes de realizar cualquier acción:

1. Analiza la estructura actual del repositorio.
2. Identifica la solución .NET existente, proyectos, convenciones de nombres y organización de carpetas.
3. Identifica las versiones de .NET utilizadas.
4. Identifica si existen plantillas, configuraciones o estándares arquitectónicos que deban respetarse.
5. Determina dónde puede ubicarse el nuevo microservicio sin afectar los componentes existentes.

No asumas que puedes modificar archivos existentes para integrar el nuevo servicio.

Si necesitas modificar algún archivo existente para completar la tarea, detente y solicita autorización.

## 3. Arquitectura requerida

El microservicio debe utilizar Clean Architecture con los siguientes proyectos:

- **Vinculaciones.Api:** punto de entrada HTTP del microservicio.
- **Vinculaciones.Application:** capa de aplicación, organización de casos de uso y abstracciones.
- **Vinculaciones.Domain:** entidades, objetos de valor y conceptos propios del dominio.
- **Vinculaciones.Infrastructure:** implementaciones de persistencia, integraciones externas y servicios técnicos.
- **Vinculaciones.Worker:** proceso independiente para ejecutar trabajos en segundo plano, especialmente los relacionados con la carga masiva.

Cada proyecto debe tener su propio archivo `.csproj`.

## 4. Referencias entre proyectos

Respeta estrictamente las dependencias de Clean Architecture.

Las referencias permitidas son:

- Domain: no debe depender de ningún otro proyecto.
- Application: depende únicamente de Domain.
- Infrastructure: depende de Application y Domain.
- Api: depende de Application e Infrastructure.
- Worker: depende de Application e Infrastructure.

No permitas dependencias desde Domain o Application hacia Infrastructure o Api.

## 5. Organización de carpetas

Crea únicamente las siguientes estructuras iniciales:

### Vinculaciones.Domain

- Entities/
  - Vinculaciones/
  - Configuracion/
  - Inconsistencias/
- ValueObjects/
- Enums/
- Events/
- Exceptions/

### Vinculaciones.Application

- Abstractions/
  - Persistence/
  - Messaging/
  - ExternalServices/
- Common/
  - Behaviors/
  - Exceptions/
- Features/
  - Vinculaciones/
    - Commands/
    - Queries/
    - DTOs/
  - Configuracion/
    - Commands/
    - Queries/
    - DTOs/
  - Inconsistencias/
    - Commands/
    - Queries/
    - DTOs/
  - CargaMasiva/
    - Commands/
    - Queries/
    - DTOs/

### Vinculaciones.Infrastructure

- Persistence/
  - Context/
  - Configurations/
  - Repositories/
  - Migrations/
- Messaging/
- BackgroundJobs/
- ExternalServices/
- DependencyInjection/

### Vinculaciones.Api

- Controllers/
  - Vinculaciones/
  - Configuracion/
  - Inconsistencias/
  - CargaMasiva/
- Contracts/
- Middleware/
- Filters/
- Extensions/
- DependencyInjection/

### Vinculaciones.Worker

- Workers/
- Jobs/
- Extensions/
- DependencyInjection/

## 6. Reglas obligatorias

Debes cumplir las siguientes restricciones:

1. NO modificar ningún proyecto existente.
2. NO modificar archivos `.csproj` existentes.
3. NO modificar la solución `.sln` existente.
4. NO modificar configuraciones globales como `Directory.Build.props`, `Directory.Packages.props`, `global.json` o archivos similares.
5. NO eliminar, renombrar ni mover archivos existentes.
6. NO implementar controladores.
7. NO crear entidades de dominio.
8. NO crear interfaces ni clases de implementación.
9. NO crear servicios ni casos de uso.
10. NO crear repositorios.
11. NO implementar conexiones a bases de datos.
12. NO configurar colas, Azure Service Bus ni otros servicios externos.
13. NO crear lógica de procesamiento de archivos o cargas masivas.
14. NO agregar paquetes NuGet innecesarios.
15. NO modificar código de otros microservicios.

Puedes crear archivos `.csproj` mínimos, las carpetas indicadas y los archivos estructurales estrictamente necesarios.

Si la solución existente no puede incorporar los nuevos proyectos sin modificar archivos actuales, crea una solución independiente para Vinculaciones dentro de una nueva carpeta, sin alterar la solución original.

## 7. Solución esperada

La estructura debe permitir desarrollar posteriormente los cuatro módulos funcionales:

- Vinculaciones.
- Configuración.
- Inconsistencias y Casos.
- Carga masiva.

La arquitectura debe permitir que la API y el Worker compartan las capas Application e Infrastructure sin duplicar responsabilidades.

La solución debe quedar preparada para incorporar posteriormente persistencia, mensajería, procesamiento asíncrono y lógica de negocio, pero no debes implementar ninguno de estos componentes en esta tarea.

## 8. Validación final

Una vez creada la estructura:

1. Verifica que todos los proyectos compilen.
2. Verifica las referencias entre proyectos.
3. Verifica que Domain y Application no tengan dependencias hacia capas externas.
4. Verifica que no se haya modificado ningún archivo preexistente.
5. Presenta un resumen de los archivos y carpetas creados.
6. Indica la ubicación de la nueva solución.
7. Reporta cualquier decisión que hayas tenido que tomar por las convenciones existentes.

**CRITERIO DE ACEPTACIÓN: La tarea se considera terminada cuando existe la estructura de proyectos y carpetas solicitada, las referencias arquitectónicas son correctas y no se ha modificado ningún componente preexistente.**

No avances a ninguna implementación adicional sin mi autorización explícita.