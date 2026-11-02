using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BibliotecaPublica.Models;
using BibliotecaPublica.Repositories;

namespace BibliotecaPublica.Controllers;

[Authorize]
[AutoValidateAntiforgeryToken]
public class PagosController : Controller
{
    private const int TamPagina = 10;

    private readonly RepositorioPago _repoPago;
    private readonly RepositorioSuscripcion _repoSuscripcion;
    private readonly RepositorioAfiliado _repoAfiliado;
    private readonly ILogger<PagosController> _logger;

    public PagosController(
        RepositorioPago repoPago,
        RepositorioSuscripcion repoSuscripcion,
        RepositorioAfiliado repoAfiliado,
        ILogger<PagosController> logger)
    {
        _repoPago = repoPago;
        _repoSuscripcion = repoSuscripcion;
        _repoAfiliado = repoAfiliado;
        _logger = logger;
    }

    // Id del usuario logueado
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // GET: /Pagos/Index
    public IActionResult Index(int? idAfiliado, string? buscar, int pagina = 1)
    {
        if (idAfiliado.HasValue)
        {
            var afiliado = _repoAfiliado.ObtenerPorId(idAfiliado.Value);
            if (afiliado == null) return NotFound();
            ViewBag.Afiliado = afiliado;
        }

        pagina = Math.Max(1, pagina);
        int total = _repoPago.ObtenerCantidad(idAfiliado, buscar);

        ViewBag.IdAfiliado = idAfiliado;
        ViewBag.Buscar = buscar;
        ViewBag.Pagina = pagina;
        ViewBag.TotalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)TamPagina));

        return View(_repoPago.ObtenerPaginado(idAfiliado, buscar, pagina, TamPagina));
    }

    // GET: /Pagos/Create?idSuscripcion=5
    public IActionResult Create(int idSuscripcion)
    {
        var suscripcion = _repoSuscripcion.ObtenerPorId(idSuscripcion);
        if (suscripcion == null) return NotFound();

        if (_repoPago.ExisteCuotaActiva(idSuscripcion))
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
            ? _repoSuscripcion.ObtenerPorId(pago.IdSuscripcion.Value)
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

        if (_repoPago.ExisteCuotaActiva(suscripcion.Id))
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
            _repoPago.Create(pago);
            TempData["Exito"] = "Pago registrado correctamente.";
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
        var pago = _repoPago.ObtenerPorId(id);
        if (pago == null)
        {
            TempData["Error"] = "El pago no existe o ya estaba anulado.";
            return RedirectToAction(nameof(Index));
        }

        if (_repoPago.Anular(id, UserId) > 0)
            TempData["Exito"] = "Pago anulado.";
        else
            TempData["Error"] = "No se pudo anular el pago.";

        // Volvemos a la lista del afiliado al que pertenecía el pago
        return RedirectToAction(nameof(Index), new { idAfiliado = pago.IdAfiliado });
    }

    private void CargarDatosVista(Suscripcion suscripcion)
    {
        ViewBag.Afiliado = _repoAfiliado.ObtenerPorId(suscripcion.IdAfiliado);
        ViewBag.Suscripcion = suscripcion;
    }
}