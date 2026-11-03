using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BibliotecaPublica.Models;

public class Prestamo
{
    public const string EstadoEnProgreso = "En progreso";
    public const string EstadoDevuelto = "Devuelto";

    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un afiliado.")]
    [Display(Name = "Afiliado")]
    public int IdAfiliado { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un ejemplar.")]
    [Display(Name = "Ejemplar")]
    public int IdEjemplar { get; set; }

    public int CreadoPorUserId { get; set; }

    public int? AnuladoPorUserId { get; set; }

    [Display(Name = "Fecha de préstamo")]
    [DataType(DataType.Date)]
    public DateTime FechaPrestamo { get; set; } = DateTime.Today;

    [Display(Name = "Devolución pactada")]
    [DataType(DataType.Date)]
    public DateTime FechaDevolucionPactada { get; set; }

    [Display(Name = "Devolución real")]
    [DataType(DataType.Date)]
    public DateTime? FechaDevolucionReal { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Estado")]
    public string Estado { get; set; } = EstadoEnProgreso;

    [Display(Name = "Activo")]
    public bool Activo { get; set; } = true;

    // Objetos mapeados mediante JOINs
    [ValidateNever] public Afiliado? Afiliado { get; set; }
    [ValidateNever] public Ejemplar? Ejemplar { get; set; }
    [ValidateNever] public Usuario? CreadoPorUsuario { get; set; }
    [ValidateNever] public Usuario? AnuladoPorUsuario { get; set; }

    // Se calcula solo, no se guarda en la base
    public bool Vencido => Activo
                           && FechaDevolucionReal == null
                           && FechaDevolucionPactada.Date < DateTime.Today;

    public override string ToString() => $"Préstamo #{Id} - {Afiliado?.Apellido ?? "Afiliado"} ({FechaPrestamo:dd/MM/yyyy})";
}