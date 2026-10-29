using System.ComponentModel.DataAnnotations;

namespace BibliotecaPublica.Models;

public class Editorial
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre de la editorial es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    [Display(Name = "Editorial")]
    public string Nombre { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    public override string ToString() => Nombre;
}