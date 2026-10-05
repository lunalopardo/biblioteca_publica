using System.ComponentModel.DataAnnotations;

namespace BibliotecaPublica.Models;

public class Pago
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Debe asociar el pago a un afiliado.")]
    [Display(Name = "Afiliado")]
    public int IdAfiliado { get; set; }

    [Required]
    public int CreadoPorUserId { get; set; }

    public int? AnuladoPorUserId { get; set; }

    [Required(ErrorMessage = "La fecha del pago es obligatoria.")]
    [Display(Name = "Fecha de pago")]
    [DataType(DataType.Date)]
    public DateTime FechaPago { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "El monto es obligatorio.")]
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
    public Afiliado? Afiliado { get; set; }
    public Usuario? CreadoPorUsuario { get; set; }
    public Usuario? AnuladoPorUsuario { get; set; }

    public override string ToString() => $"Pago #{Id} - {Concepto} - ${Monto:N2}";
}