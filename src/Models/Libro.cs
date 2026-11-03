using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using BibliotecaPublica.Helpers;

namespace BibliotecaPublica.Models;

public class Libro
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El título del libro es obligatorio.")]
    [StringLength(255, ErrorMessage = "El título no puede superar los 255 caracteres.")]
    [Display(Name = "Título")]
    public string Titulo { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un autor.")]
    [Display(Name = "Autor")]
    public int IdAutor { get; set; }

    [StringLength(20, ErrorMessage = "El ISBN no puede superar los 20 caracteres.")]
    [Display(Name = "ISBN")]
    public string? Isbn { get; set; }

    [Range(1, 10000, ErrorMessage = "La cantidad de páginas debe ser un valor positivo.")]
    [Display(Name = "Páginas")]
    public int? Paginas { get; set; }

    [Range(1000, 2100, ErrorMessage = "Ingrese un año de publicación válido.")]
    [Display(Name = "Año de publicación")]
    public int? AnioPublicacion { get; set; }

    [StringLength(50, ErrorMessage = "El idioma no puede superar los 50 caracteres.")]
    [Display(Name = "Idioma")]
    public string? Idioma { get; set; }

    [StringLength(5000, ErrorMessage = "La descripción no puede superar los 5000 caracteres.")]
    [Display(Name = "Descripción")]
    public string? Descripcion { get; set; }

    public string? Portada { get; set; }

    [ImagenValida]
    [Display(Name = "Imagen de portada")]
    public IFormFile? PortadaFile { get; set; }

    public bool EliminarPortada { get; set; }

    [Display(Name = "Editorial")]
    public int? IdEditorial { get; set; }

    [Display(Name = "Activo")]
    public bool Activo { get; set; } = true;

    [Display(Name = "Géneros")]
    public List<int> GenerosSeleccionados { get; set; } = new();

    // Objetos mapeados mediante JOINs
    [ValidateNever] public Editorial? Editorial { get; set; }
    [ValidateNever] public Autor? Autor { get; set; }
    [ValidateNever] public List<Genero>? Generos { get; set; }
    [ValidateNever] public List<Ejemplar>? Ejemplares { get; set; }

    public override string ToString() => Titulo;
}