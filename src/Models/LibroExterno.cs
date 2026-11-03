namespace BibliotecaPublica.Models;

// Un resultado de la búsqueda en Open Library, ya ordenado para mostrarlo e importarlo
public class LibroExterno
{
    public string Titulo { get; set; } = string.Empty;
    public string? AutorNombre { get; set; }
    public int? Anio { get; set; }
    public string? Isbn { get; set; }
    public int? Paginas { get; set; }
    public string? PortadaUrl { get; set; }
    public string? Editorial { get; set; }
    public string? Idioma { get; set; }
    public List<string> IdiomasDisponibles { get; set; } = new();
    public string? WorkKey { get; set; }
}