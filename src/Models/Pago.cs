using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BibliotecaPublica.Models;

public enum TipoPago
{
    Cuota,
    Multa
}

public class Pago
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debe asociar el pago a un afiliado.")]
    [Display(Name = "Afiliado")]
    public int IdAfiliado { get; set; }

    public int CreadoPorUserId { get; set; }

    public int? AnuladoPorUserId { get; set; }

    // Auditoría
    public DateTime? CreadoEn { get; set; }
    public DateTime? AnuladoEn { get; set; }

    [Display(Name = "Tipo de pago")]
    public TipoPago Tipo { get; set; } = TipoPago.Cuota;

    [Display(Name = "Suscripción")]
    public int? IdSuscripcion { get; set; }

    [Display(Name = "Préstamo")]
    public int? IdPrestamo { get; set; }

    [Required(ErrorMessage = "La fecha del pago es obligatoria.")]
    [Display(Name = "Fecha de pago")]
    [DataType(DataType.Date)]
    public DateTime FechaPago { get; set; } = DateTime.Today;

    [Range(0.01, 1000000.00, ErrorMessage = "El monto debe ser un valor mayor a cero.")]
    public decimal Monto { get; set; }

    [Required(ErrorMessage = "El concepto del pago es obligatorio.")]
    [StringLength(255, ErrorMessage = "El concepto no puede superar los 255 caracteres.")]
    public string Concepto { get; set; } = string.Empty;

    [Display(Name = "Saldado")]
    public bool Saldado { get; set; } = true;

    [Display(Name = "Activo")]
    public bool Activo { get; set; } = true;

    // Objetos mapeados mediante JOINs
    [ValidateNever] public Afiliado? Afiliado { get; set; }
    [ValidateNever] public Usuario? CreadoPorUsuario { get; set; }
    [ValidateNever] public Usuario? AnuladoPorUsuario { get; set; }
    [ValidateNever] public Suscripcion? Suscripcion { get; set; }   // NUEVO
    [ValidateNever] public Prestamo? Prestamo { get; set; }         // NUEVO

    public override string ToString() => $"Pago #{Id} - {Concepto} - ${Monto:N2}";
}