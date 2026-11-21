              CORE VINCULACIONES
                     │
              ref.negocio
                     │ negocio_id
                     ▼
          mae.tipo_vinculacion
                     │ tipo_vinculacion_id
                     ▼
            mae.tipo_contrato
                     │ tipo_contrato_id
                     ▼
              mae.tipo_plan
                     │ tipo_plan_id + negocio_id
                     ▼
          RULES VALIDATOR API
                     │
              reglas.flujo_reglas
                     │
                     ▼
                reglas.regla
                     │
                     ▼
            reglas.version_regla
                     │
                     ▼
              RuleDefinition
                     │
        ┌────────────┴─────────────┐
        ▼                          ▼
reglas.campo_regla      reglas.campo_regla_operador
        │
        ├── dato_requerido_id -> cfg.dato_requerido
        └── contexto recibido del Core
                     │
                     ▼
           Microsoft RulesEngine
                     │
                     ▼
        reglas.ejecucion_regla

`trx.dato_requerido_valor` y `trx.vinculacion` siguen bajo propiedad del Core; RulesValidator no crea copias ni persiste esos datos en su esquema.

`reglas.flujo_reglas.negocio_id` referencia `ref.negocio.negocio_id` y
`reglas.flujo_reglas.tipo_plan_id` referencia `mae.tipo_plan.tipo_plan_id`.
`reglas.campo_regla.dato_requerido_id` mapea el alias de RuleDefinition a
`cfg.dato_requerido`; Core sigue siendo dueño de los metadatos del dato.