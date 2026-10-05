using System.ComponentModel.DataAnnotations;

namespace BibliotecaPublica.Models;

public class Genero
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre del género es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    [Display(Name = "Género")]
    public string Nombre { get; set; } = string.Empty;

    [Display(Name = "Activo")]
    public bool Activo { get; set; } = true;

    public override string ToString() => Nombre;
}