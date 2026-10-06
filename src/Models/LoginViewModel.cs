using System.ComponentModel.DataAnnotations;

namespace BibliotecaPublica.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "El usuario o correo es obligatorio.")]
    [Display(Name = "Correo o Usuario")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;
}