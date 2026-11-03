using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BibliotecaPublica.Models;
using BibliotecaPublica.Repositories;

namespace BibliotecaPublica.Controllers;

[Authorize]
[AutoValidateAntiforgeryToken]
public class SuscripcionesController : Controller
{
    private readonly RepositorioSuscripcion _repoSuscripcion;
    private readonly RepositorioPago _repoPago;
    private readonly RepositorioAfiliado _repoAfiliado;
    private readonly ILogger<SuscripcionesController> _logger;

    public SuscripcionesController(
        RepositorioSuscripcion repoSuscripcion,
        RepositorioPago repoPago,
        RepositorioAfiliado repoAfiliado,
        ILogger<SuscripcionesController> logger)
    {
        _repoSuscripcion = repoSuscripcion;
        _repoPago = repoPago;
        _repoAfiliado = repoAfiliado;
        _logger = logger;
    }

    // GET: /Suscripciones/Index?idAfiliado=3
    public IActionResult Index(int idAfiliado)
    {
        var afiliado = _repoAfiliado.ObtenerPorId(idAfiliado);
        if (afiliado == null) return NotFound();

        ViewBag.Afiliado = afiliado;
        return View(_repoSuscripcion.ObtenerPorAfiliado(idAfiliado));
    }

    // GET: /Suscripciones/Create?idAfiliado=3
    public IActionResult Create(int idAfiliado)
    {
        var afiliado = _repoAfiliado.ObtenerPorId(idAfiliado);
        if (afiliado == null) return NotFound();

        ViewBag.Afiliado = afiliado;
        return View(new Suscripcion { IdAfiliado = idAfiliado });
    }

    [HttpPost]
    public IActionResult Create(Suscripcion suscripcion)
    {
        if (suscripcion.FechaFin.Date <= suscripcion.FechaInicio.Date)
        {
            ModelState.AddModelError(nameof(suscripcion.FechaFin),
                "La fecha de fin debe ser posterior a la de inicio.");
        }
        else if (_repoSuscripcion.ExisteSuperposicion(
                     suscripcion.IdAfiliado, suscripcion.FechaInicio, suscripcion.FechaFin))
        {
            ModelState.AddModelError("", "Ya existe una suscripción que se superpone con esas fechas.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Afiliado = _repoAfiliado.ObtenerPorId(suscripcion.IdAfiliado);
            return View(suscripcion);
        }

        try
        {
            _repoSuscripcion.Create(suscripcion);
            TempData["Exito"] = "Suscripción creada. Ahora podés registrar el pago.";
            return RedirectToAction(nameof(Index), new { idAfiliado = suscripcion.IdAfiliado });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear suscripción del afiliado {IdAfiliado}", suscripcion.IdAfiliado);
            ModelState.AddModelError("", "No se pudo crear la suscripción.");
            ViewBag.Afiliado = _repoAfiliado.ObtenerPorId(suscripcion.IdAfiliado);
            return View(suscripcion);
        }
    }

    [HttpPost]
    public IActionResult Anular(int id)
    {
        var suscripcion = _repoSuscripcion.ObtenerPorId(id);
        if (suscripcion == null) return NotFound();

        // No se puede anular una suscripción que ya tiene un pago registrado
        if (_repoPago.ExisteCuotaActiva(id))
        {
            TempData["Error"] = "No se puede anular: tiene un pago registrado. Anulá primero el pago.";
        }
        else
        {
            _repoSuscripcion.AnularLogico(id);
            TempData["Exito"] = "Suscripción anulada.";
        }

        return RedirectToAction(nameof(Index), new { idAfiliado = suscripcion.IdAfiliado });
    }
}