using System.ComponentModel.DataAnnotations;

namespace BibliotecaPublica.Models;

public enum EstadoEjemplar
{
    Disponible,
    Prestado
}

public class Ejemplar
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Debe asociar el ejemplar a un libro.")]
    [Display(Name = "Libro")]
    public int IdLibro { get; set; }

    [Required(ErrorMessage = "El código de inventario es obligatorio.")]
    [Display(Name = "Código de inventario")]
    public int CodigoInventario { get; set; }

    [Required(ErrorMessage = "El estado del ejemplar es obligatorio.")]
    [Display(Name = "Estado")]
    public string Estado { get; set; } = EstadoEjemplar.Disponible.ToString();

    [Display(Name = "Activo")]
    public bool Activo { get; set; } = true;

    // Objeto mapeado mediante JOIN
    public Libro? Libro { get; set; }

    public override string ToString() => $"Código #{CodigoInventario} - {Libro?.Titulo ?? "Sin título"}";
}