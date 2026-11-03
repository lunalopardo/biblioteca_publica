using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BibliotecaPublica.Models;
using BibliotecaPublica.Repositories;

namespace BibliotecaPublica.Controllers;

// El alta, la edición y la baja se hacen con modales desde el Index,
// por eso todas las acciones POST vuelven al Index con el mensaje en TempData.
[Authorize]
[AutoValidateAntiforgeryToken]
public class GenerosController : Controller
{
    private readonly RepositorioGenero _repositorioGenero;
    private readonly ILogger<GenerosController> _logger;

    public GenerosController(RepositorioGenero repoGenero, ILogger<GenerosController> logger)
    {
        _repositorioGenero = repoGenero;
        _logger = logger;
    }

    // GET: Generos/Index
    public IActionResult Index(string? buscar, int pagina = 1)
    {
        int registrosPorPagina = 10;
        var listado = _repositorioGenero.ObtenerPaginado(buscar, pagina, registrosPorPagina);
        int totalRegistros = _repositorioGenero.ObtenerCantidad(buscar);
        int totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);

        ViewData["FiltroActual"] = buscar;
        ViewData["PaginaActual"] = pagina;
        ViewData["TotalPaginas"] = totalPaginas == 0 ? 1 : totalPaginas;

        return View(listado);
    }

    // POST: Generos/Create
    [HttpPost]
    public IActionResult Create(Genero genero)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = PrimerError();
            return RedirectToAction(nameof(Index));
        }

        try
        {
            _repositorioGenero.Create(genero);
            TempData["Mensaje"] = "Género creado exitosamente.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear el género {Nombre}", genero.Nombre);
            TempData["Error"] = "No se pudo guardar el género.";
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: Generos/Edit/5
    [HttpPost]
    public IActionResult Edit(int id, Genero genero)
    {
        if (id != genero.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            TempData["Error"] = PrimerError();
            return RedirectToAction(nameof(Index));
        }

        try
        {
            _repositorioGenero.Update(genero);
            TempData["Mensaje"] = "Género actualizado exitosamente.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar el género {Id}", genero.Id);
            TempData["Error"] = "No se pudo actualizar el género.";
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: Generos/Delete/5
    [HttpPost]
    public IActionResult Delete(int id)
    {
        try
        {
            _repositorioGenero.DeleteLogico(id);
            TempData["Mensaje"] = "Género eliminado exitosamente.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar el género {Id}", id);
            TempData["Error"] = "No se pudo eliminar el género.";
        }

        return RedirectToAction(nameof(Index));
    }

    private string PrimerError() =>
        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                  .FirstOrDefault(m => !string.IsNullOrEmpty(m)) ?? "Datos inválidos.";
}
