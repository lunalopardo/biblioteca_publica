using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BibliotecaPublica.Models;

public class Autor
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(30, ErrorMessage = "El nombre no puede superar los 30 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(30, ErrorMessage = "El apellido no puede superar los 30 caracteres.")]
    public string Apellido { get; set; } = string.Empty;

    [StringLength(30, ErrorMessage = "La nacionalidad no puede superar los 30 caracteres.")]
    public string? Nacionalidad { get; set; }

    [Display(Name = "Biografía")]
    public string? Biografia { get; set; }

    public string? Foto { get; set; }

    [Display(Name = "Foto del autor")]
    public IFormFile? FotoFile { get; set; }

    [Display(Name = "Activo")]
    public bool Activo { get; set; } = true;

    // Propiedad poblada manualmente
    public List<Libro>? Libros { get; set; }

    public override string ToString() => $"{Apellido}, {Nombre}";
}