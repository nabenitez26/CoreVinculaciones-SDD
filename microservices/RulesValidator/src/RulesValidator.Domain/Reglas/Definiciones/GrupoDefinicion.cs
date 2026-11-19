namespace RulesValidator.Domain.Reglas.Definiciones;

public sealed record GrupoDefinicion(OperadorLogico OperadorLogico, IReadOnlyList<NodoDefinicion> Condiciones) : NodoDefinicion;
