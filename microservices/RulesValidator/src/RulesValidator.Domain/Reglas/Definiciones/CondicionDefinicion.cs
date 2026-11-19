namespace RulesValidator.Domain.Reglas.Definiciones;

// Valor: string, número, booleano o lista de esos tipos (EN/NO_EN); su validez la comprueba el validador de definiciones.
public sealed record CondicionDefinicion(string Campo, OperadorComparacion Operador, object Valor) : NodoDefinicion;
