using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BibliotecaPublica.Helpers;
using BibliotecaPublica.Models;
using BibliotecaPublica.Repositories;

namespace BibliotecaPublica.Controllers;

[Authorize]
[AutoValidateAntiforgeryToken]
public class SuscripcionesController : Controller
{
    private const int TamPagina = 10;

    private readonly RepositorioSuscripcion _repositorioSuscripcion;
    private readonly RepositorioPago _repositorioPago;
    private readonly RepositorioAfiliado _repositorioAfiliado;
    private readonly ILogger<SuscripcionesController> _logger;

    public SuscripcionesController(
        RepositorioSuscripcion repositorioSuscripcion,
        RepositorioPago repositorioPago,
        RepositorioAfiliado repositorioAfiliado,
        ILogger<SuscripcionesController> logger)
    {
        _repositorioSuscripcion = repositorioSuscripcion;
        _repositorioPago = repositorioPago;
        _repositorioAfiliado = repositorioAfiliado;
        _logger = logger;
    }

    // GET: /Suscripciones/Activas (todas las que no terminaron, de todos los afiliados)
    public IActionResult Activas(string? buscar, int pagina = 1)
    {
        var listado = _repositorioSuscripcion.ObtenerActivasPaginado(buscar, pagina, TamPagina);
        this.CargarPaginado(buscar, pagina, _repositorioSuscripcion.ObtenerCantidadActivas(buscar), TamPagina);

        return View(listado);
    }

    // GET: /Suscripciones/Index?idAfiliado=3
    public IActionResult Index(int idAfiliado)
    {
        var afiliado = _repositorioAfiliado.ObtenerPorId(idAfiliado);
        if (afiliado == null) return NotFound();

        ViewBag.Afiliado = afiliado;
        return View(_repositorioSuscripcion.ObtenerPorAfiliado(idAfiliado));
    }

    // GET: /Suscripciones/Create (el afiliado se elige con Select2)
    // GET: /Suscripciones/Create?idAfiliado=3 (viene con el afiliado ya elegido)
    public IActionResult Create(int? idAfiliado)
    {
        var suscripcion = new Suscripcion();

        if (idAfiliado.HasValue)
        {
            var afiliado = _repositorioAfiliado.ObtenerPorId(idAfiliado.Value);
            if (afiliado == null) return NotFound();

            suscripcion.IdAfiliado = afiliado.Id;
            suscripcion.Afiliado = afiliado;
        }

        return View(suscripcion);
    }

    // GET: /Suscripciones/Ocupadas?idAfiliado=3
    // Rangos de días que ya cubre una suscripción del afiliado, para deshabilitarlos en el calendario.
    [HttpGet]
    public JsonResult Ocupadas(int idAfiliado)
    {
        var rangos = _repositorioSuscripcion.ObtenerPorAfiliado(idAfiliado)
            .Select(s => new
            {
                desde = s.FechaInicio.ToString("yyyy-MM-dd"),
                hasta = s.FechaFin.ToString("yyyy-MM-dd")
            });
        return Json(rangos);
    }

    [HttpPost]
    public IActionResult Create(Suscripcion suscripcion)
    {
        suscripcion.Afiliado = suscripcion.IdAfiliado > 0 ? _repositorioAfiliado.ObtenerPorId(suscripcion.IdAfiliado) : null;

        if (suscripcion.IdAfiliado > 0 && suscripcion.Afiliado == null)
        {
            ModelState.AddModelError(nameof(suscripcion.IdAfiliado), "El afiliado seleccionado no existe.");
        }

        if (suscripcion.FechaFin.Date <= suscripcion.FechaInicio.Date)
        {
            ModelState.AddModelError(nameof(suscripcion.FechaFin),
                "La fecha de fin debe ser posterior a la de inicio.");
        }
        else if (_repositorioSuscripcion.ExisteSuperposicion(
                     suscripcion.IdAfiliado, suscripcion.FechaInicio, suscripcion.FechaFin))
        {
            ModelState.AddModelError("", "Ya existe una suscripción que se superpone con esas fechas.");
        }

        if (!ModelState.IsValid)
        {
            return View(suscripcion);
        }

        try
        {
            _repositorioSuscripcion.Crear(suscripcion);
            TempData["Mensaje"] = "Suscripción creada. Ahora podés registrar el pago.";
            return RedirectToAction(nameof(Index), new { idAfiliado = suscripcion.IdAfiliado });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear suscripción del afiliado {IdAfiliado}", suscripcion.IdAfiliado);
            ModelState.AddModelError("", "No se pudo crear la suscripción.");
            return View(suscripcion);
        }
    }

    [HttpPost]
    public IActionResult Anular(int id)
    {
        var suscripcion = _repositorioSuscripcion.ObtenerPorId(id);
        if (suscripcion == null) return NotFound();

        // No se puede anular una suscripción que ya tiene un pago registrado
        if (_repositorioPago.ExisteCuotaActiva(id))
        {
            TempData["Error"] = "No se puede anular: tiene un pago registrado. Anulá primero el pago.";
        }
        else
        {
            _repositorioSuscripcion.BajaLogica(id);
            TempData["Mensaje"] = "Suscripción anulada.";
        }

        return RedirectToAction(nameof(Index), new { idAfiliado = suscripcion.IdAfiliado });
    }
}