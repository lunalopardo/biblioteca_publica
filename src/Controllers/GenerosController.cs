using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BibliotecaPublica.Models;
using BibliotecaPublica.Repositories;

namespace BibliotecaPublica.Controllers;

[Authorize]
public class GenerosController : Controller
{
    private readonly RepositorioGenero _repoGenero;

    public GenerosController(RepositorioGenero repoGenero)
    {
        _repoGenero = repoGenero;
    }

    // GET: Generos/Index
    public IActionResult Index(string? buscar, int pagina = 1)
    {
        int registrosPorPagina = 10;
        var listado = _repoGenero.ObtenerPaginado(buscar, pagina, registrosPorPagina);
        int totalRegistros = _repoGenero.ObtenerCantidad(buscar);
        int totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);

        ViewData["FiltroActual"] = buscar;
        ViewData["PaginaActual"] = pagina;
        ViewData["TotalPaginas"] = totalPaginas == 0 ? 1 : totalPaginas;

        return View(listado);
    }

    // GET: Generos/Details/5
    public IActionResult Details(int id)
    {
        var genero = _repoGenero.ObtenerPorId(id);
        if (genero == null)
        {
            TempData["Error"] = "El género solicitado no existe.";
            return RedirectToAction(nameof(Index));
        }

        return View(genero);
    }

    // GET: Generos/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Generos/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Genero genero)
    {
        if (ModelState.IsValid)
        {
            try
            {
                _repoGenero.Create(genero);
                TempData["Mensaje"] = "Género creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al guardar el género: " + ex.Message;
            }
        }

        return View(genero);
    }

    // GET: Generos/Edit/5
    public IActionResult Edit(int id)
    {
        var genero = _repoGenero.ObtenerPorId(id);
        if (genero == null)
        {
            TempData["Error"] = "El género a editar no existe.";
            return RedirectToAction(nameof(Index));
        }

        return View(genero);
    }

    // POST: Generos/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Genero genero)
    {
        if (id != genero.Id)
        {
            return BadRequest();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _repoGenero.Update(genero);
                TempData["Mensaje"] = "Género actualizado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al actualizar el género: " + ex.Message;
            }
        }

        return View(genero);
    }

    // GET: Generos/Delete/5
    public IActionResult Delete(int id)
    {
        var genero = _repoGenero.ObtenerPorId(id);
        if (genero == null)
        {
            TempData["Error"] = "El género a eliminar no existe.";
            return RedirectToAction(nameof(Index));
        }

        return View(genero);
    }

    // POST: Generos/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        try
        {
            _repoGenero.DeleteLogico(id);
            TempData["Mensaje"] = "Género eliminado exitosamente.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "No se pudo eliminar el género: " + ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}