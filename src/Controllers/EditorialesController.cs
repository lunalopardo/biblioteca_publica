using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BibliotecaPublica.Models;
using BibliotecaPublica.Repositories;

namespace BibliotecaPublica.Controllers;

[Authorize]
public class EditorialesController : Controller
{
    private readonly RepositorioEditorial _repositorioEditorial;

    public EditorialesController(RepositorioEditorial repoEditorial)
    {
        _repositorioEditorial = repoEditorial;
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

    // GET: Editoriales/Details/5
    public IActionResult Details(int id)
    {
        var Editorial = _repositorioEditorial.ObtenerPorId(id);
        if (Editorial == null)
        {
            TempData["Error"] = "La editorial solicitada no existe.";
            return RedirectToAction(nameof(Index));
        }

        return View(Editorial);
    }

    // GET: Editoriales/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Editoriales/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Editorial Editorial)
    {
        if (ModelState.IsValid)
        {
            try
            {
                _repositorioEditorial.Create(Editorial);
                TempData["Mensaje"] = "Editorial creada exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al guardar la editorial: " + ex.Message;
            }
        }

        return View(Editorial);
    }

    // GET: Editoriales/Edit/5
    public IActionResult Edit(int id)
    {
        var Editorial = _repositorioEditorial.ObtenerPorId(id);
        if (Editorial == null)
        {
            TempData["Error"] = "La editorial a editar no existe.";
            return RedirectToAction(nameof(Index));
        }

        return View(Editorial);
    }

    // POST: Editoriales/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Editorial Editorial)
    {
        if (id != Editorial.Id)
        {
            return BadRequest();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _repositorioEditorial.Update(Editorial);
                TempData["Mensaje"] = "Editorial actualizada exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al actualizar la editorial: " + ex.Message;
            }
        }

        return View(Editorial);
    }

    // GET: Editoriales/Delete/5
    public IActionResult Delete(int id)
    {
        var Editorial = _repositorioEditorial.ObtenerPorId(id);
        if (Editorial == null)
        {
            TempData["Error"] = "El editorial a eliminar no existe.";
            return RedirectToAction(nameof(Index));
        }

        return View(Editorial);
    }

    // POST: Editoriales/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        try
        {
            _repositorioEditorial.DeleteLogico(id);
            TempData["Mensaje"] = "Editorial eliminada exitosamente.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "No se pudo eliminar la editorial: " + ex.Message;
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
}