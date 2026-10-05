using System.ComponentModel.DataAnnotations;

namespace BibliotecaPublica.Models;

public class Prestamo
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un afiliado.")]
    [Display(Name = "Afiliado")]
    public int IdAfiliado { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un ejemplar.")]
    [Display(Name = "Ejemplar")]
    public int IdEjemplar { get; set; }

    [Required]
    public int CreadoPorUserId { get; set; }

    public int? AnuladoPorUserId { get; set; }

    [Required(ErrorMessage = "La fecha de préstamo es obligatoria.")]
    [Display(Name = "Fecha de préstamo")]
    [DataType(DataType.Date)]
    public DateTime FechaPrestamo { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "La fecha de devolución pactada es obligatoria.")]
    [Display(Name = "Devolución pactada")]
    [DataType(DataType.Date)]
    public DateTime FechaDevolucionPactada { get; set; }

    [Display(Name = "Devolución real")]
    [DataType(DataType.Date)]
    public DateTime? FechaDevolucionReal { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Estado")]
    public string Estado { get; set; } = "En progreso";

    [Display(Name = "Activo")]
    public bool Activo { get; set; } = true;

    // Objetos mapeados mediante JOINs
    public Afiliado? Afiliado { get; set; }
    public Ejemplar? Ejemplar { get; set; }
    public Usuario? CreadoPorUsuario { get; set; }
    public Usuario? AnuladoPorUsuario { get; set; }

    public override string ToString() => $"Préstamo #{Id} - {Afiliado?.Apellido ?? "Afiliado"} ({FechaPrestamo:dd/MM/yyyy})";
}