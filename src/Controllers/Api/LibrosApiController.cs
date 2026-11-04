using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BibliotecaPublica.Helpers;
using BibliotecaPublica.Models;
using BibliotecaPublica.Repositories;

namespace BibliotecaPublica.Controllers.Api;

// Catálogo para clientes externos. Pide un JWT válido.
[ApiController]
[Route("api/libros")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class LibrosApiController : ControllerBase
{
    private const int TamPagina = 10;

    private readonly RepositorioLibro _repositorioLibro;

    public LibrosApiController(RepositorioLibro repositorioLibro)
    {
        _repositorioLibro = repositorioLibro;
    }

    // GET: /api/libros?buscar=borges&pagina=1
    [HttpGet]
    public IActionResult Listar(string? buscar, int pagina = 1)
    {
        pagina = Math.Max(1, pagina);
        int total = _repositorioLibro.ObtenerCantidad(buscar);

        return Ok(new
        {
            items = _repositorioLibro.ObtenerPaginado(buscar, pagina, TamPagina).Select(Resumen),
            pagina,
            totalPaginas = Paginacion.TotalPaginas(total, TamPagina)
        });
    }

    // GET: /api/libros/5
    [HttpGet("{id:int}")]
    public IActionResult Obtener(int id)
    {
        var libro = _repositorioLibro.ObtenerPorId(id);
        return libro == null
            ? NotFound(new { message = "El libro no existe." })
            : Ok(Resumen(libro));
    }

    // DELETE: /api/libros/5 (solo administradores, igual que en la página)
    [HttpDelete("{id:int}")]
    [Authorize(Policy = UsuarioActual.PolicyAdministrador)]
    public IActionResult Eliminar(int id)
    {
        if (_repositorioLibro.ObtenerPorId(id) == null)
            return NotFound(new { message = "El libro no existe." });

        if (_repositorioLibro.TienePrestamosEnCurso(id))
            return Conflict(new { message = "No se puede dar de baja el libro: tiene ejemplares prestados." });

        _repositorioLibro.BajaLogica(id);
        return Ok(new { message = "Libro eliminado correctamente." });
    }

    private static object Resumen(Libro libro) => new
    {
        libro.Id,
        libro.Titulo,
        autor = libro.Autor?.ToString(),
        editorial = libro.Editorial?.Nombre,
        libro.Isbn,
        libro.AnioPublicacion,
        libro.Idioma,
        libro.Paginas,
        libro.Descripcion,
        libro.Portada,
        generos = libro.Generos?.Select(g => g.Nombre) ?? [],
        ejemplares = libro.Ejemplares?.Count ?? 0,
        disponibles = libro.Ejemplares?.Count(e => e.Estado == nameof(EstadoEjemplar.Disponible)) ?? 0
    };
}
