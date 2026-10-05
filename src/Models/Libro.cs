using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BibliotecaPublica.Models;

public class Libro
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El título del libro es obligatorio.")]
    [StringLength(255, ErrorMessage = "El título no puede superar los 255 caracteres.")]
    [Display(Name = "Título")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe seleccionar un autor.")]
    [Display(Name = "Autor")]
    public int IdAutor { get; set; }

    [StringLength(20, ErrorMessage = "El ISBN no puede superar los 20 caracteres.")]
    public string? Isbn { get; set; }

    [Range(1, 10000, ErrorMessage = "La cantidad de páginas debe ser un valor positivo.")]
    [Display(Name = "Páginas")]
    public int? Paginas { get; set; }

    [Range(1000, 2100, ErrorMessage = "Ingrese un año de publicación válido.")]
    [Display(Name = "Año de publicación")]
    public int? AnioPublicacion { get; set; }

    public string? Portada { get; set; }

    [Display(Name = "Imagen de portada")]
    public IFormFile? PortadaFile { get; set; }

    [StringLength(100, ErrorMessage = "La editorial no puede superar los 100 caracteres.")]
    public string? Editorial { get; set; }

    [Display(Name = "Activo")]
    public bool Activo { get; set; } = true;

    // Propiedades mapeadas mediante JOINs
    public Autor? Autor { get; set; }
    public List<Genero>? Generos { get; set; }
    public List<Ejemplar>? Ejemplares { get; set; }

    public override string ToString() => Titulo;
}