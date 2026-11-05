namespace BibliotecaPublica.Models;

public class SelectorImagen
{
    public string Titulo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Campo { get; set; } = string.Empty;
    public string CampoEliminar { get; set; } = string.Empty;
    public string Icono { get; set; } = string.Empty;
    public string? ImagenActual { get; set; }
    public bool EsPortada { get; set; }
    public bool Editable { get; set; } = true;
}
