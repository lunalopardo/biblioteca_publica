using System.ComponentModel.DataAnnotations;

namespace BibliotecaPublica.Models;

public class Afiliado
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(50, ErrorMessage = "El apellido no puede superar los 50 caracteres.")]
    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingrese una dirección de email válida.")]
    [StringLength(100, ErrorMessage = "El email no puede superar los 100 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [StringLength(15, ErrorMessage = "El teléfono no puede superar los 15 caracteres.")]
    [Display(Name = "Teléfono")]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de alta es obligatoria.")]
    [Display(Name = "Fecha de alta")]
    [DataType(DataType.Date)]
    public DateTime FechaAlta { get; set; } = DateTime.Today;

    [Display(Name = "Estado")]
    public bool Estado { get; set; } = true;

    [Display(Name = "Sancionado")]
    public bool Sancionado { get; set; } = false;

    [Display(Name = "Activo")]
    public bool Activo { get; set; } = true;

    // para hacer JOINs
    public List<Pago>? Pagos { get; set; }
    public List<Prestamo>? Prestamos { get; set; }

    public override string ToString() => $"{Apellido}, {Nombre}";
}