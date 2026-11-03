using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BibliotecaPublica.Helpers;
using BibliotecaPublica.Repositories;

namespace BibliotecaPublica.Controllers;

[Authorize]
[AutoValidateAntiforgeryToken]
public class ImagenesController : Controller
{
    private readonly RepositorioUsuario _repositorioUsuario;
    private readonly IWebHostEnvironment _environment;

    public ImagenesController(RepositorioUsuario repositorioUsuario, IWebHostEnvironment environment)
    {
        _repositorioUsuario = repositorioUsuario;
        _environment = environment;
    }

    [HttpPost]
    public IActionResult EliminarAvatarUsuario(int idUsuario)
    {
        // Igual que en el perfil: el admin puede tocar cualquier usuario, el resto solo el suyo
        if (!User.EsAdministrador() && idUsuario != User.ObtenerId())
        {
            return Forbid();
        }

        var usuario = _repositorioUsuario.ObtenerPorId(idUsuario);
        if (usuario == null) return NotFound();

        if (!string.IsNullOrEmpty(usuario.Avatar))
        {
            Archivos.Borrar(usuario.Avatar, _environment);
            usuario.Avatar = null;
            _repositorioUsuario.ActualizarCompleto(usuario);
        }

        return RedirectToAction("Perfil", "Usuarios", new { id = idUsuario });
    }
}
