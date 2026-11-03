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
public class NacionalidadesController : Controller
{
    private const int TamPagina = 10;

    private readonly RepositorioNacionalidad _repositorioNacionalidad;
    private readonly ILogger<NacionalidadesController> _logger;

    public NacionalidadesController(RepositorioNacionalidad repositorioNacionalidad, ILogger<NacionalidadesController> logger)
    {
        _repositorioNacionalidad = repositorioNacionalidad;
        _logger = logger;
    }

    // GET: Nacionalidades/Index
    public IActionResult Index(string? buscar, int pagina = 1)
    {
        var listado = _repositorioNacionalidad.ObtenerPaginado(buscar, pagina, TamPagina);
        this.CargarPaginado(buscar, pagina, _repositorioNacionalidad.ObtenerCantidad(buscar), TamPagina);

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
            _repositorioNacionalidad.Crear(nacionalidad);
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
            _repositorioNacionalidad.Actualizar(nacionalidad);
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
            _repositorioNacionalidad.BajaLogica(id);
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
