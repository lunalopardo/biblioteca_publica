using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BibliotecaPublica.Models;

public enum EnRoles
{
    Administrador = 1,
    Empleado = 2
}

public class Usuario
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [StringLength(20, ErrorMessage = "El nombre de usuario no puede superar los 20 caracteres.")]
    [Display(Name = "Nombre de usuario")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(30, ErrorMessage = "El nombre no puede superar los 30 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(30, ErrorMessage = "El apellido no puede superar los 300 caracteres.")]
    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingrese una dirección de email válida.")]
    [StringLength(100, ErrorMessage = "El email no puede superar los 100 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public string? Avatar { get; set; }

    [Display(Name = "Avatar")]
    public IFormFile? AvatarFile { get; set; }

    [Required(ErrorMessage = "El rol es obligatorio.")]
    public int Rol { get; set; } = (int)EnRoles.Empleado;

    [Display(Name = "Rol")]
    public string RolNombre => Rol == (int)EnRoles.Administrador ? "Administrador" : "Empleado";

    [Display(Name = "Activo")]
    public bool Activo { get; set; } = true;

    public override string ToString() => $"{Apellido}, {Nombre} ({NombreUsuario})";
}