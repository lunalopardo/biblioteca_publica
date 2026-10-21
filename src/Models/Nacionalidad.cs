using System.ComponentModel.DataAnnotations;

namespace BibliotecaPublica.Models;

public class Nacionalidad
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre de la nacionalidad es obligatorio.")]
    [StringLength(50)]
    public string Nombre { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;
}