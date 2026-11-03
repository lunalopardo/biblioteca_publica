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
}
