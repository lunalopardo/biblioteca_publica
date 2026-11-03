using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BibliotecaPublica.Repositories;

namespace BibliotecaPublica.Controllers
{
    [Authorize]
    [AutoValidateAntiforgeryToken]
    public class ImagenesController : Controller
    {
        private readonly RepositorioUsuario _repositorioUsuario;

        public ImagenesController(
            RepositorioUsuario repositorioUsuario)
        {
            _repositorioUsuario = repositorioUsuario;
        }

        public static async Task<string> GuardarArchivoAsync(IFormFile archivo, IWebHostEnvironment environment, string subcarpeta = "Inmuebles")
        {
            if (archivo == null || archivo.Length == 0)
                return string.Empty;

            string uploadsFolder = Path.Combine(environment.WebRootPath, "Uploads", subcarpeta);
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();

            string nombreArchivo = $"{Guid.NewGuid()}{extension}";
            string rutaCompleta = Path.Combine(uploadsFolder, nombreArchivo);

            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            return $"/Uploads/{subcarpeta}/{nombreArchivo}";
        }

        public static void BorrarArchivoFisico(string? urlRelativa, IWebHostEnvironment environment)
        {
            if (string.IsNullOrEmpty(urlRelativa)) return;

            string rutaFisica = Path.Combine(environment.WebRootPath, urlRelativa.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(rutaFisica))
            {
                System.IO.File.Delete(rutaFisica);
            }
        }


        [HttpPost]
        public IActionResult EliminarAvatarUsuario(
            int idUsuario,
            [FromServices] IWebHostEnvironment environment)
        {
            // Igual que en el perfil: el admin puede tocar cualquier usuario, el resto solo el suyo
            int currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (!User.IsInRole("Administrador") && idUsuario != currentUserId)
            {
                return Forbid();
            }

            var usuario = _repositorioUsuario.ObtenerPorId(idUsuario);
            if (usuario == null) return NotFound();

            if (!string.IsNullOrEmpty(usuario.Avatar))
            {
                BorrarArchivoFisico(usuario.Avatar, environment);
                usuario.Avatar = null;
                _repositorioUsuario.UpdateCompleto(usuario);
            }

            return RedirectToAction("Perfil", "Usuarios", new { id = idUsuario });
        }
    }
}