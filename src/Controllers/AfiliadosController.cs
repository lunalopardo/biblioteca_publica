using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BibliotecaPublica.Helpers;
using BibliotecaPublica.Models;
using BibliotecaPublica.Repositories;

namespace BibliotecaPublica.Controllers;

[Authorize]
[AutoValidateAntiforgeryToken]
public class AfiliadosController : Controller
{
    private const int TamPagina = 10;

    private readonly RepositorioAfiliado _repositorioAfiliado;
    private readonly RepositorioSuscripcion _repositorioSuscripcion;

    public AfiliadosController(RepositorioAfiliado repositorioAfiliado, RepositorioSuscripcion repositorioSuscripcion)
    {
        _repositorioAfiliado = repositorioAfiliado;
        _repositorioSuscripcion = repositorioSuscripcion;
    }

    // GET: Afiliados (con paginado)
    public IActionResult Index(string? buscar, bool sancionados = false, int pagina = 1)
    {
        var listado = _repositorioAfiliado.ObtenerPaginado(buscar, pagina, TamPagina, sancionados);
        this.CargarPaginado(buscar, pagina, _repositorioAfiliado.ObtenerCantidad(buscar, sancionados), TamPagina);
        ViewBag.Sancionados = sancionados;

        // clave = id del afiliado, valor = true si la suscripción de hoy está pagada
        ViewData["EstadoSuscripciones"] = _repositorioSuscripcion.ObtenerEstadoActual(listado.Select(a => a.Id));

        return View(listado);
    }

    // GET: Afiliados/Details/5
    public IActionResult Details(int? id)
    {
        if (id == null) return NotFound();

        var afiliado = _repositorioAfiliado.ObtenerPorId(id.Value);
        if (afiliado == null) return NotFound();

        ViewData["EstadoSuscripciones"] = _repositorioSuscripcion.ObtenerEstadoActual(new[] { afiliado.Id });

        return View(afiliado);
    }

    // GET: Afiliados/Create
    public IActionResult Create() => View();

    // POST: Afiliados/Create
    [HttpPost]
    public IActionResult Create(Afiliado afiliado)
    {
        if (_repositorioAfiliado.ExisteEmail(afiliado.Email))
        {
            ModelState.AddModelError("Email", "Este correo electrónico ya se encuentra registrado.");
        }

        if (ModelState.IsValid)
        {
            _repositorioAfiliado.Crear(afiliado);
            TempData["Mensaje"] = "Afiliado registrado con éxito.";
            return RedirectToAction(nameof(Index));
        }

        return View(afiliado);
    }

    // GET: Afiliados/Edit/5
    public IActionResult Edit(int? id)
    {
        if (id == null) return NotFound();

        var afiliado = _repositorioAfiliado.ObtenerPorId(id.Value);
        if (afiliado == null) return NotFound();

        return View(afiliado);
    }

    // POST: Afiliados/Edit/5
    [HttpPost]
    public IActionResult Edit(int id, Afiliado afiliado)
    {
        if (id != afiliado.Id) return NotFound();

        if (_repositorioAfiliado.ExisteEmail(afiliado.Email, id))
        {
            ModelState.AddModelError("Email", "Este correo electrónico ya pertenece a otro afiliado.");
        }

        if (ModelState.IsValid)
        {
            _repositorioAfiliado.Actualizar(afiliado);
            TempData["Mensaje"] = "Afiliado actualizado con éxito.";
            return RedirectToAction(nameof(Index));
        }

        return View(afiliado);
    }

    // POST: Afiliados/Delete/5
    [HttpPost]
    public IActionResult Delete(int id)
    {
        _repositorioAfiliado.BajaLogica(id);
        TempData["Mensaje"] = "Afiliado dado de baja correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // Endpoint JSON para búsquedas asíncronas por AJAX
    [HttpGet]
    public JsonResult BuscarAsincrona(string term)
    {
        var afiliados = _repositorioAfiliado.ObtenerParaSelect(term, 10);
        var resultado = afiliados.Select(a => new {
            id = a.Id,
            text = $"{a.Apellido}, {a.Nombre} - Email: {a.Email}"
        });
        return Json(resultado);
    }
}
