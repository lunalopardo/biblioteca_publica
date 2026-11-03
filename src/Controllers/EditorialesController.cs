using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BibliotecaPublica.Models;
using BibliotecaPublica.Repositories;

namespace BibliotecaPublica.Controllers;

// El alta, la edición y la baja se hacen con modales desde el Index,
// por eso todas las acciones POST vuelven al Index con el mensaje en TempData.
[Authorize]
[AutoValidateAntiforgeryToken]
public class EditorialesController : Controller
{
    private readonly RepositorioEditorial _repositorioEditorial;
    private readonly ILogger<EditorialesController> _logger;

    public EditorialesController(RepositorioEditorial repoEditorial, ILogger<EditorialesController> logger)
    {
        _repositorioEditorial = repoEditorial;
        _logger = logger;
    }

    // GET: Editoriales/Index
    public IActionResult Index(string? buscar, int pagina = 1)
    {
        int registrosPorPagina = 10;
        var listado = _repositorioEditorial.ObtenerPaginado(buscar, pagina, registrosPorPagina);
        int totalRegistros = _repositorioEditorial.ObtenerCantidad(buscar);
        int totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);

        ViewData["FiltroActual"] = buscar;
        ViewData["PaginaActual"] = pagina;
        ViewData["TotalPaginas"] = totalPaginas == 0 ? 1 : totalPaginas;

        return View(listado);
    }

    // POST: Editoriales/Create
    [HttpPost]
    public IActionResult Create(Editorial editorial)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = PrimerError();
            return RedirectToAction(nameof(Index));
        }

        try
        {
            _repositorioEditorial.Create(editorial);
            TempData["Mensaje"] = "Editorial creada exitosamente.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear la editorial {Nombre}", editorial.Nombre);
            TempData["Error"] = "No se pudo guardar la editorial.";
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: Editoriales/Edit/5
    [HttpPost]
    public IActionResult Edit(int id, Editorial editorial)
    {
        if (id != editorial.Id)
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
            _repositorioEditorial.Update(editorial);
            TempData["Mensaje"] = "Editorial actualizada exitosamente.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar la editorial {Id}", editorial.Id);
            TempData["Error"] = "No se pudo actualizar la editorial.";
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: Editoriales/Delete/5
    [HttpPost]
    public IActionResult Delete(int id)
    {
        try
        {
            _repositorioEditorial.DeleteLogico(id);
            TempData["Mensaje"] = "Editorial eliminada exitosamente.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar la editorial {Id}", id);
            TempData["Error"] = "No se pudo eliminar la editorial.";
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: /Editoriales/Buscar
    [HttpGet]
    public IActionResult Buscar(string? q)
    {
        var editoriales = _repositorioEditorial.ObtenerTodas();
        if (!string.IsNullOrWhiteSpace(q))
        {
            editoriales = editoriales.Where(e => e.Nombre.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var resultado = editoriales.Select(e => new
        {
            id = e.Id,
            text = e.Nombre
        });

        return Json(resultado);
    }

    private string PrimerError() =>
        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                  .FirstOrDefault(m => !string.IsNullOrEmpty(m)) ?? "Datos inválidos.";
}
