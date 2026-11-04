using System.Security.Claims;
using BibliotecaPublica.Models;

namespace BibliotecaPublica.Helpers;

// Datos del usuario logueado que se leen de sus claims (controladores y vistas)
public static class UsuarioActual
{
    // Nombre de la policy y del rol de administrador
    public const string PolicyAdministrador = nameof(EnRoles.Administrador);

    public static int ObtenerId(this ClaimsPrincipal usuario) =>
        int.TryParse(usuario.FindFirstValue(ClaimTypes.NameIdentifier), out int id) ? id : 0;

    public static bool EsAdministrador(this ClaimsPrincipal usuario) =>
        usuario.IsInRole(nameof(EnRoles.Administrador));

    // Los mismos datos van en la cookie (páginas) y en el token JWT (Web API)
    public static List<Claim> CrearClaims(Usuario usuario) => new()
    {
        new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
        new(ClaimTypes.Name, usuario.Email),
        new("FullName", $"{usuario.Nombre} {usuario.Apellido}"),
        new(ClaimTypes.Role, usuario.RolNombre),
        new("Avatar", usuario.Avatar ?? "")
    };
}
