using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using BibliotecaPublica.Models;

namespace BibliotecaPublica.Helpers;

public static class OpenLibrary
{
    public const string UrlBase = "https://openlibrary.org";
    public const string PrefijoPortadas = "https://covers.openlibrary.org/";

    // Pedimos solo los campos que usamos
    public const string CamposBusqueda =
        "key,title,author_name,first_publish_year,isbn,number_of_pages_median,cover_i,publisher,language";

    // Una clave válida tiene forma /works/OL123W
    private static readonly Regex ClaveObra = new(@"^/works/OL\d+W$", RegexOptions.Compiled);

    // Open Library usa códigos de 3 letras (estándar MARC)
    private static readonly Dictionary<string, string> Idiomas = new(StringComparer.OrdinalIgnoreCase)
    {
        ["spa"] = "Español",
        ["eng"] = "Inglés",
        ["fre"] = "Francés",
        ["ger"] = "Alemán",
        ["ita"] = "Italiano",
        ["por"] = "Portugués",
        ["lat"] = "Latín",
        ["rus"] = "Ruso",
        ["jpn"] = "Japonés",
        ["chi"] = "Chino",
        ["ara"] = "Árabe",
        ["cat"] = "Catalán",
        ["dut"] = "Neerlandés",
        ["pol"] = "Polaco",
        ["swe"] = "Sueco",
        ["tur"] = "Turco",
        ["gre"] = "Griego",
        ["grc"] = "Griego antiguo",
        ["heb"] = "Hebreo",
        ["hin"] = "Hindi",
        ["kor"] = "Coreano",
        ["nor"] = "Noruego",
        ["dan"] = "Danés",
        ["fin"] = "Finés",
        ["cze"] = "Checo",
        ["hun"] = "Húngaro",
        ["rum"] = "Rumano",
        ["ukr"] = "Ucraniano"
    };

    public static bool EsClaveValida(string? clave) => clave != null && ClaveObra.IsMatch(clave);

    public static bool EsIdiomaConocido(string? idioma) => idioma != null && Idiomas.ContainsValue(idioma);

    // Convierte un resultado de la búsqueda (JSON) en un LibroExterno
    public static LibroExterno Parsear(JsonObject doc)
    {
        var libro = new LibroExterno
        {
            Titulo = Texto(doc["title"]) ?? "Sin título",
            AutorNombre = Primero(doc["author_name"]),
            Anio = (int?)Numero(doc["first_publish_year"]),
            Isbn = ElegirIsbn(doc["isbn"]),
            Paginas = (int?)Numero(doc["number_of_pages_median"]),
            Editorial = Primero(doc["publisher"]),
            WorkKey = Texto(doc["key"]),
            IdiomasDisponibles = ExtraerIdiomas(doc["language"])
        };

        var idPortada = Numero(doc["cover_i"]);
        if (idPortada != null)
        {
            libro.PortadaUrl = $"{PrefijoPortadas}b/id/{idPortada}-M.jpg";
        }
        return libro;
    }

    // La descripción puede venir como texto o como { "type": ..., "value": "..." }
    public static string? ExtraerDescripcion(JsonObject obra)
    {
        var nodo = obra["description"];
        string? texto = nodo switch
        {
            JsonObject o => Texto(o["value"]),
            JsonValue => Texto(nodo),
            _ => null
        };
        if (string.IsNullOrWhiteSpace(texto)) return null;

        // Muchas descripciones terminan con una línea de guiones y datos de otras ediciones
        int corte = texto.IndexOf("----------", StringComparison.Ordinal);
        if (corte > 0) texto = texto[..corte];

        return texto.Trim();
    }

    // Funciones auxiliares para leer el JSON sin que explote

    private static string? Texto(JsonNode? nodo) =>
        nodo is JsonValue v && v.TryGetValue<string>(out var s) ? s : null;

    private static long? Numero(JsonNode? nodo) =>
        nodo is JsonValue v && v.TryGetValue<long>(out var n) ? n : null;

    private static string? Primero(JsonNode? nodo) =>
        nodo is JsonArray arr && arr.Count > 0 ? Texto(arr[0]) : null;

    // Se prefiere un ISBN-13 (13 dígitos); si no hay, el primero que venga
    private static string? ElegirIsbn(JsonNode? nodo)
    {
        if (nodo is not JsonArray arr) return null;

        var isbns = arr.Select(Texto)
                       .Where(s => !string.IsNullOrWhiteSpace(s))
                       .Select(s => s!)
                       .ToList();

        return isbns.FirstOrDefault(i => i.Length == 13) ?? isbns.FirstOrDefault();
    }

    private static List<string> ExtraerIdiomas(JsonNode? nodo)
    {
        var resultado = new List<string>();
        if (nodo is not JsonArray arr) return resultado;

        foreach (var item in arr)
        {
            var codigo = Texto(item);
            if (codigo != null && Idiomas.TryGetValue(codigo, out var nombre) && !resultado.Contains(nombre))
            {
                resultado.Add(nombre);
            }
        }
        return resultado;
    }
}