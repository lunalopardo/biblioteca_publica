using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BibliotecaPublica.Models;

public class Suscripcion
{

    public enum EstadoSuscripcion
    {
        Sin,
        PendientePago,
        Vigente
    }
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debe asociar la suscripción a un afiliado.")]
    [Display(Name = "Afiliado")]
    public int IdAfiliado { get; set; }

    [Display(Name = "Fecha de inicio")]
    [DataType(DataType.Date)]
    public DateTime FechaInicio { get; set; } = DateTime.Today;

    [Display(Name = "Fecha de fin")]
    [DataType(DataType.Date)]
    public DateTime FechaFin { get; set; } = DateTime.Today.AddMonths(1);

    [Range(0.01, 1000000.00, ErrorMessage = "El monto debe ser mayor a cero.")]
    public decimal Monto { get; set; }

    [Display(Name = "Activo")]
    public bool Activo { get; set; } = true;

    [ValidateNever]
    public Afiliado? Afiliado { get; set; }

    // Propiedad calculada: no se guarda en la base
    public bool Vigente => Activo && FechaFin.Date >= DateTime.Today;

    public bool Pagada { get; set; }
}