using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BibliotecaPublica.Models;
using BibliotecaPublica.Repositories;

namespace BibliotecaPublica.Controllers;

// El alta, la edición y la baja se hacen con modales desde el Index,
// por eso todas las acciones POST vuelven al Index con el mensaje en TempData.
[Authorize]
[AutoValidateAntiforgeryToken]
public class NacionalidadesController : Controller
{
    private readonly RepositorioNacionalidad _repoNacionalidad;
    private readonly ILogger<NacionalidadesController> _logger;

    public NacionalidadesController(RepositorioNacionalidad repoNacionalidad, ILogger<NacionalidadesController> logger)
    {
        _repoNacionalidad = repoNacionalidad;
        _logger = logger;
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

    // POST: Nacionalidades/Create
    [HttpPost]
    public IActionResult Create(Nacionalidad nacionalidad)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = PrimerError();
            return RedirectToAction(nameof(Index));
        }

        try
        {
            _repoNacionalidad.Create(nacionalidad);
            TempData["Mensaje"] = "Nacionalidad creada exitosamente.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear la nacionalidad {Nombre}", nacionalidad.Nombre);
            TempData["Error"] = "No se pudo guardar la nacionalidad.";
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: Nacionalidades/Edit/5
    [HttpPost]
    public IActionResult Edit(int id, Nacionalidad nacionalidad)
    {
        if (id != nacionalidad.Id)
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
            _repoNacionalidad.Update(nacionalidad);
            TempData["Mensaje"] = "Nacionalidad actualizada exitosamente.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar la nacionalidad {Id}", nacionalidad.Id);
            TempData["Error"] = "No se pudo actualizar la nacionalidad.";
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: Nacionalidades/Delete/5
    [HttpPost]
    public IActionResult Delete(int id)
    {
        try
        {
            _repoNacionalidad.DeleteLogico(id);
            TempData["Mensaje"] = "Nacionalidad eliminada exitosamente.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar la nacionalidad {Id}", id);
            TempData["Error"] = "No se pudo eliminar la nacionalidad.";
        }

        return RedirectToAction(nameof(Index));
    }

    private string PrimerError() =>
        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                  .FirstOrDefault(m => !string.IsNullOrEmpty(m)) ?? "Datos inválidos.";
}
