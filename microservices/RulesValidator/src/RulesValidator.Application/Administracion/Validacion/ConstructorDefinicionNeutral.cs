using System.Text.Json.Nodes;
using RulesValidator.Application.Administracion.Dtos;
using RulesValidator.Application.Administracion.Errores;

namespace RulesValidator.Application.Administracion.Validacion;

// Construye el formato neutral persistido. Solo la raíz recibe versionEsquema "1.0" (lo exige el esquema canónico).
// Los nodos anidados se transforman tal cual: si traen propiedades no permitidas, el esquema las rechaza.
public static class ConstructorDefinicionNeutral
{
    public const string VersionEsquema = "1.0";

    public static string Construir(DefinicionReglaDto definicion)
    {
        ArgumentNullException.ThrowIfNull(definicion);

        if (definicion.VersionEsquema is { } versionInformada && !string.Equals(versionInformada, VersionEsquema, StringComparison.Ordinal))
        {
            throw new SolicitudAdministracionInvalidaException("versionEsquema debe ser 1.0 cuando se informa.");
        }

        return ConstruirNodo(definicion, raiz: true).ToJsonString();
    }

    private static JsonObject ConstruirNodo(DefinicionReglaDto dto, bool raiz)
    {
        var nodo = new JsonObject();

        if (raiz)
        {
            nodo["versionEsquema"] = VersionEsquema;
        }
        else if (dto.VersionEsquema is not null)
        {
            nodo["versionEsquema"] = dto.VersionEsquema;
        }

        if (dto.Tipo is not null)
        {
            nodo["tipo"] = dto.Tipo;
        }

        if (dto.Campo is not null)
        {
            nodo["campo"] = dto.Campo;
        }

        if (dto.Operador is not null)
        {
            nodo["operador"] = dto.Operador;
        }

        if (dto.Valor is { } valor)
        {
            nodo["valor"] = JsonNode.Parse(valor.GetRawText());
        }

        if (dto.OperadorLogico is not null)
        {
            nodo["operadorLogico"] = dto.OperadorLogico;
        }

        if (dto.Aplicabilidad is not null)
        {
            nodo["aplicabilidad"] = ConstruirNodo(dto.Aplicabilidad, raiz: false);
        }

        if (dto.Condiciones is not null)
        {
            var condiciones = new JsonArray();
            foreach (var condicion in dto.Condiciones)
            {
                condiciones.Add(condicion is null ? null : ConstruirNodo(condicion, raiz: false));
            }

            nodo["condiciones"] = condiciones;
        }

        return nodo;
    }
}
