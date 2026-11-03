using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BibliotecaPublica.Models;

public enum EstadoEjemplar
{
    Disponible,
    Prestado
}

public class Ejemplar
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debe asociar el ejemplar a un libro.")]
    [Display(Name = "Libro")]
    public int IdLibro { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El código de inventario debe ser un número positivo.")]
    [Display(Name = "Código de inventario")]
    public int CodigoInventario { get; set; }

    [Required(ErrorMessage = "El estado del ejemplar es obligatorio.")]
    [RegularExpression("^(Disponible|Prestado)$", ErrorMessage = "Estado inválido.")]
    [Display(Name = "Estado")]
    public string Estado { get; set; } = EstadoEjemplar.Disponible.ToString();

    [StringLength(100, ErrorMessage = "La ubicación no puede superar los 100 caracteres.")]
    [Display(Name = "Ubicación")]
    public string? Ubicacion { get; set; }

    [Display(Name = "Activo")]
    public bool Activo { get; set; } = true;

    [ValidateNever]
    public Libro? Libro { get; set; }

    public override string ToString() => $"Código #{CodigoInventario} - {Libro?.Titulo ?? "Sin título"}";
}