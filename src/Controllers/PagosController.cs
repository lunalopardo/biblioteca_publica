using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BibliotecaPublica.Helpers;
using BibliotecaPublica.Models;
using BibliotecaPublica.Repositories;

namespace BibliotecaPublica.Controllers;

[Authorize]
[AutoValidateAntiforgeryToken]
public class PagosController : Controller
{
    private const int TamPagina = 10;

    private readonly RepositorioPago _repositorioPago;
    private readonly RepositorioSuscripcion _repositorioSuscripcion;
    private readonly RepositorioAfiliado _repositorioAfiliado;
    private readonly ILogger<PagosController> _logger;

    public PagosController(
        RepositorioPago repositorioPago,
        RepositorioSuscripcion repositorioSuscripcion,
        RepositorioAfiliado repositorioAfiliado,
        ILogger<PagosController> logger)
    {
        _repositorioPago = repositorioPago;
        _repositorioSuscripcion = repositorioSuscripcion;
        _repositorioAfiliado = repositorioAfiliado;
        _logger = logger;
    }

    // Id del usuario logueado
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // GET: /Pagos/Index
    public IActionResult Index(int? idAfiliado, string? buscar, int pagina = 1)
    {
        if (idAfiliado.HasValue)
        {
            var afiliado = _repositorioAfiliado.ObtenerPorId(idAfiliado.Value);
            if (afiliado == null) return NotFound();
            ViewBag.Afiliado = afiliado;
        }

        var listado = _repositorioPago.ObtenerPaginado(idAfiliado, buscar, pagina, TamPagina);
        this.CargarPaginado(buscar, pagina, _repositorioPago.ObtenerCantidad(idAfiliado, buscar), TamPagina);
        ViewBag.IdAfiliado = idAfiliado;

        return View(listado);
    }

    // GET: /Pagos/Create?idSuscripcion=5
    public IActionResult Create(int idSuscripcion)
    {
        var suscripcion = _repositorioSuscripcion.ObtenerPorId(idSuscripcion);
        if (suscripcion == null) return NotFound();

        if (_repositorioPago.ExisteCuotaActiva(idSuscripcion))
        {
            TempData["Error"] = "Esta suscripción ya tiene un pago registrado.";
            return RedirectToAction("Index", "Suscripciones", new { idAfiliado = suscripcion.IdAfiliado });
        }

        CargarDatosVista(suscripcion);
        return View(new Pago
        {
            Tipo = TipoPago.Cuota,
            IdSuscripcion = suscripcion.Id,
            IdAfiliado = suscripcion.IdAfiliado,
            Monto = suscripcion.Monto,
            Concepto = $"Cuota {suscripcion.FechaInicio:dd/MM/yyyy} al {suscripcion.FechaFin:dd/MM/yyyy}"
        });
    }

    [HttpPost]
    public IActionResult Create(Pago pago)
    {
        var suscripcion = pago.IdSuscripcion.HasValue
            ? _repositorioSuscripcion.ObtenerPorId(pago.IdSuscripcion.Value)
            : null;
        if (suscripcion == null) return NotFound();

        // El monto, el afiliado y el tipo salen de la suscripción
        pago.Tipo = TipoPago.Cuota;
        pago.IdAfiliado = suscripcion.IdAfiliado;
        pago.Monto = suscripcion.Monto;
        pago.IdPrestamo = null;
        pago.CreadoPorUserId = UserId;
        pago.Saldado = true;

        ModelState.Clear();
        TryValidateModel(pago);

        if (_repositorioPago.ExisteCuotaActiva(suscripcion.Id))
        {
            ModelState.AddModelError("", "Esta suscripción ya tiene un pago registrado.");
        }

        if (!ModelState.IsValid)
        {
            CargarDatosVista(suscripcion);
            return View(pago);
        }

        try
        {
            _repositorioPago.Crear(pago);
            TempData["Mensaje"] = "Pago registrado correctamente.";
            return RedirectToAction("Index", "Suscripciones", new { idAfiliado = suscripcion.IdAfiliado });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar pago de la suscripción {Id}", suscripcion.Id);
            ModelState.AddModelError("", "No se pudo registrar el pago.");
            CargarDatosVista(suscripcion);
            return View(pago);
        }
    }

    [HttpPost]
    public IActionResult Anular(int id)
    {
        var pago = _repositorioPago.ObtenerPorId(id);
        if (pago == null)
        {
            TempData["Error"] = "El pago no existe o ya estaba anulado.";
            return RedirectToAction(nameof(Index));
        }

        if (_repositorioPago.Anular(id, UserId) > 0)
            TempData["Mensaje"] = "Pago anulado.";
        else
            TempData["Error"] = "No se pudo anular el pago.";

        // Volvemos a la lista del afiliado al que pertenecía el pago
        return RedirectToAction(nameof(Index), new { idAfiliado = pago.IdAfiliado });
    }

    private void CargarDatosVista(Suscripcion suscripcion)
    {
        ViewBag.Afiliado = _repositorioAfiliado.ObtenerPorId(suscripcion.IdAfiliado);
        ViewBag.Suscripcion = suscripcion;
    }
}