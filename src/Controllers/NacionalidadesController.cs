using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BibliotecaPublica.Models;
using BibliotecaPublica.Repositories;

namespace BibliotecaPublica.Controllers;

[Authorize]
public class NacionalidadesController : Controller
{
    private readonly RepositorioNacionalidad _repoNacionalidad;

    public NacionalidadesController(RepositorioNacionalidad repoNacionalidad)
    {
        _repoNacionalidad = repoNacionalidad;
    }

    // GET: Nacionalidades/Index
    public IActionResult Index(string? buscar, int pagina = 1)
    {
        int registrosPorPagina = 10;
        var listado = _repoNacionalidad.ObtenerPaginado(buscar, pagina, registrosPorPagina);
        int totalRegistros = _repoNacionalidad.ObtenerCantidad(buscar);
        int totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);

        ViewData["FiltroActual"] = buscar;
        ViewData["PaginaActual"] = pagina;
        ViewData["TotalPaginas"] = totalPaginas == 0 ? 1 : totalPaginas;

        return View(listado);
    }

    // GET: Nacionalidades/Details/5
    public IActionResult Details(int id)
    {
        var Nacionalidad = _repoNacionalidad.ObtenerPorId(id);
        if (Nacionalidad == null)
        {
            TempData["Error"] = "La Nacionalidad solicitada no existe.";
            return RedirectToAction(nameof(Index));
        }

        return View(Nacionalidad);
    }

    // GET: Nacionalidades/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Nacionalidades/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Nacionalidad Nacionalidad)
    {
        if (ModelState.IsValid)
        {
            try
            {
                _repoNacionalidad.Create(Nacionalidad);
                TempData["Mensaje"] = "Nacionalidad creada exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al guardar la Nacionalidad: " + ex.Message;
            }
        }

        return View(Nacionalidad);
    }

    // GET: Nacionalidades/Edit/5
    public IActionResult Edit(int id)
    {
        var Nacionalidad = _repoNacionalidad.ObtenerPorId(id);
        if (Nacionalidad == null)
        {
            TempData["Error"] = "La Nacionalidad a editar no existe.";
            return RedirectToAction(nameof(Index));
        }

        return View(Nacionalidad);
    }

    // POST: Nacionalidades/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Nacionalidad Nacionalidad)
    {
        if (id != Nacionalidad.Id)
        {
            return BadRequest();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _repoNacionalidad.Update(Nacionalidad);
                TempData["Mensaje"] = "Nacionalidad actualizada exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al actualizar la Nacionalidad: " + ex.Message;
            }
        }

        return View(Nacionalidad);
    }

    // GET: Nacionalidades/Delete/5
    public IActionResult Delete(int id)
    {
        var Nacionalidad = _repoNacionalidad.ObtenerPorId(id);
        if (Nacionalidad == null)
        {
            TempData["Error"] = "El Nacionalidad a eliminar no existe.";
            return RedirectToAction(nameof(Index));
        }

        return View(Nacionalidad);
    }

    // POST: Nacionalidades/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        try
        {
            _repoNacionalidad.DeleteLogico(id);
            TempData["Mensaje"] = "Nacionalidad eliminada exitosamente.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "No se pudo eliminar la Nacionalidad: " + ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}