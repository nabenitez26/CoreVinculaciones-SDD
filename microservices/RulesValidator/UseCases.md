# Casos de uso - reglasValidator

## Objetivo

reglasValidator es responsable de administrar, versionar,
simular y evaluar reglas de negocio.

reglasValidator no administra solicitudes de vinculación,
casos, cargues, inconsistencias ni beneficios.

---

# 1. Evaluación de reglas

## UC-RV-001 - Evaluar reglas

POST /api/v1/evaluar-reglas

Caso de uso:
EvaluarReglasCommand

Responsabilidad:

- Recibir contexto de evaluación.
- Identificar negocio, proceso y etapa.
- Obtener reglas activas y vigentes.
- Ejecutar las reglas parametrizables mediante Microsoft reglasEngine.
- Ejecutar handlers especializados cuando corresponda.
- Consolidar resultados.
- Retornar las versiones de reglas ejecutadas.

No debe:

- modificar una vinculación;
- crear una inconsistencia;
- emitir notificaciones;
- realizar reversión de beneficios.

---

# 2. Simulación

## UC-RV-002 - Simular reglas

POST /api/v1/simular-reglas

Caso de uso:
SimularReglasCommand

Responsabilidad:

Permitir evaluar una regla o versión antes de ser publicada,
sin afectar operaciones reales.

---

# 3. Consulta de reglas

## UC-RV-003 - Consultar regla

GET /api/v1/reglas/{ReglaId}

Caso de uso:
ObtenerReglaQuery


## UC-RV-004 - Listar reglas

GET /api/v1/reglas?negocio=&estado=&etapa=

Caso de uso:
ListarReglasQuery

---

# 4. Administración

## UC-RV-005 - Crear regla

POST /api/v1/reglas

Caso de uso:
CrearReglaCommand


## UC-RV-006 - Crear nueva versión

POST /api/v1/reglas/{reglaId}/versiones

Caso de uso:
CrearversionReglaCommand


## UC-RV-007 - Activar versión

POST /api/v1/reglas/{reglaId}/versiones/{version}/activar

Caso de uso:
ActivarVersionReglaCommand


## UC-RV-008 - Inactivar versión

POST /api/v1/reglas/{reglaId}/versiones/{version}/desactivar

Caso de uso:
DesactivarVersionReglaCommand


## UC-RV-009 - Consultar versiones

GET /api/v1/reglas/{reglaId}/versiones

Caso de uso:
ObtenerVersionReglasQuery
