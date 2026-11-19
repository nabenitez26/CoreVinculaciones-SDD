# Estandar de Codigo

## Convenciones de nombres 

La solución combina español e inglés con una regla única: el lenguaje del negocio se escribe en español y los patrones técnicos conservan su nombre en inglés, según la convención del ecosistema .NET. 

Español (lenguaje del negocio): entidades, acciones, tablas, columnas, rutas REST, espacios de nombres y códigos de error. Coincide con el modelo de datos de Compensar y con el vocabulario del negocio (Solicitud, Vinculación, Cargue, Etapa). 

Inglés (patrón técnico): el sufijo que identifica el tipo de componente (Command, Query, Handler, Repository, Dto) y los nombres de capas y proyectos (Api, Application, Domain, Infrastructure). Son nombres de patrones de diseño y se mantienen como los usan las librerías .NET, la documentación oficial y los agentes de IA. 

Cómo se forma un nombre: [Acción en español] + [Entidad en español] + [Sufijo técnico en inglés] 

Componente 

Regla 

Ejemplo 

Comando 

Verbo + entidad + Command 

CrearSolicitudCommand 

Consulta 

Obtener/Listar + entidad + Query 

ObtenerSolicitudQuery 

Manejador 

Nombre del comando o consulta + Handler 

CrearSolicitudHandler 

Repositorio 

I + entidad + Repository 

ISolicitudRepository 

Objeto de transferencia 

Entidad + Dto 

SolicitudDto 

Entidad de dominio 

Español, PascalCase 

Solicitud, ProcesoCargueMasivo 

Espacio de nombres 

Español 

Compensar.Vinculaciones.Solicitudes 

Tablas y columnas 

Español, snake_case, llave <tabla>_id 

trx.solicitud, solicitud_id 

Rutas REST 

Español, plural, minúsculas 

/api/v1/solicitudes, /api/v1/cargues 

Contenedores 

Minúsculas con guion 

core-vinculaciones-api 

Capas y proyectos 

Inglés (convención .NET) 

Vinculaciones.Domain, Vinculaciones.Infrastructure 

No se permite: 

Traducir el sufijo técnico (ManejadorCrearSolicitud, RepositorioSolicitud). 

Escribir el negocio en inglés (CreateRequestCommand, IApplicationRepository). 

Mezclar idiomas dentro de un mismo término (CreateSolicitud, SolicitudRequestHandler). 


## Contrato de errores 

Todas las APIs retornan los errores en formato ProblemDetails (RFC 9457), con tipo de contenido application/problem+json. Se usan los campos estándar type, title, status y detail, y dos extensiones: traceId, para correlacionar con la telemetría, y errors, con las validaciones de negocio no superadas identificadas con los códigos del reto. 

{ 

  "type": "about:blank", 

  "title": "La solicitud no cumple las reglas de negocio", 

  "status": 422, 

  "detail": "Se encontraron 2 validaciones no superadas", 

  "traceId": "00-4bf92f35...", 

  "errors": [ 

    { "codigo": "RV-102", "detalle": "..." }, 

    { "codigo": "DR-206", "detalle": "..." } 

  ] 

} 

Códigos HTTP: 

Código 

Uso 

400 

Petición mal formada o campos obligatorios ausentes 

404 

Recurso inexistente (solicitud, cargue, regla) 

409 

Conflicto: duplicado, Idempotency-Key reutilizada con datos distintos o publicación concurrente de reglas 

422 

Petición válida que no cumple reglas de negocio (códigos RV/DR en errors) 

500 

Error interno no controlado 

503 

Dependencia no disponible (por ejemplo, Rule Engine con circuit breaker abierto) 

Reglas: 

El campo errors se usa solo con el código 422. 

La respuesta no incluye datos personales ni detalles técnicos internos (trazas de pila, consultas SQL). 

En el cargue masivo, los códigos RV/DR de cada fila rechazada se registran con la misma estructura codigo y detalle. 

Los errores 422 no se reintentan; los reintentos aplican solo a errores técnicos transitorios (timeout, 503). 

Los errores de negocio lanzados por la base de datos (THROW 50001, mensaje con prefijo ERR-) se capturan y se retornan como 422, con el código ERR-… en errors, igual que los códigos RV/DR. 

