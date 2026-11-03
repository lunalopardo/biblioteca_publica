using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BibliotecaPublica.Helpers;
using BibliotecaPublica.Models;
using BibliotecaPublica.Repositories;

namespace BibliotecaPublica.Controllers;

// El alta, la edición y la baja se hacen con modales desde el Index,
// por eso todas las acciones POST vuelven al Index con el mensaje en TempData.
[Authorize]
[AutoValidateAntiforgeryToken]
public class EditorialesController : Controller
{
    private const int TamPagina = 10;

    private readonly RepositorioEditorial _repositorioEditorial;
    private readonly ILogger<EditorialesController> _logger;

    public EditorialesController(RepositorioEditorial repositorioEditorial, ILogger<EditorialesController> logger)
    {
        _repositorioEditorial = repositorioEditorial;
        _logger = logger;
    }

    // GET: Editoriales/Index
    public IActionResult Index(string? buscar, int pagina = 1)
    {
        var listado = _repositorioEditorial.ObtenerPaginado(buscar, pagina, TamPagina);
        this.CargarPaginado(buscar, pagina, _repositorioEditorial.ObtenerCantidad(buscar), TamPagina);

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
            _repositorioEditorial.Crear(editorial);
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
            _repositorioEditorial.Actualizar(editorial);
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
            _repositorioEditorial.BajaLogica(id);
            TempData["Mensaje"] = "Editorial eliminada exitosamente.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar la editorial {Id}", id);
            TempData["Error"] = "No se pudo eliminar la editorial.";
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: /Editoriales/Buscar (Select2 del formulario de libros; filtra y limita en SQL)
    [HttpGet]
    public IActionResult Buscar(string? q)
    {
        var resultado = _repositorioEditorial.ObtenerParaSelect(q).Select(e => new
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
