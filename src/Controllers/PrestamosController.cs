using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BibliotecaPublica.Models;
using BibliotecaPublica.Repositories;

namespace BibliotecaPublica.Controllers;

[Authorize]
[AutoValidateAntiforgeryToken]
public class PrestamosController : Controller
{
    private const int TamPagina = 10;
    private const decimal MultaPorDia = 100m; // $ por cada día de atraso

    private readonly RepositorioPrestamo _repoPrestamo;
    private readonly RepositorioAfiliado _repoAfiliado;
    private readonly RepositorioSuscripcion _repoSuscripcion;
    private readonly RepositorioEjemplar _repoEjemplar;
    private readonly ILogger<PrestamosController> _logger;

    public PrestamosController(
        RepositorioPrestamo repoPrestamo,
        RepositorioAfiliado repoAfiliado,
        RepositorioSuscripcion repoSuscripcion,
        RepositorioEjemplar repoEjemplar,
        ILogger<PrestamosController> logger)
    {
        _repoPrestamo = repoPrestamo;
        _repoAfiliado = repoAfiliado;
        _repoSuscripcion = repoSuscripcion;
        _repoEjemplar = repoEjemplar;
        _logger = logger;
    }

    // Id del usuario logueado
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // GET: /Prestamos - solo muestra la página; los datos los pide Vue
    public IActionResult Index() => View();

    // GET: /Prestamos/Listar?buscar=...&pagina=1
    [HttpGet]
    public IActionResult Listar(string? buscar, int pagina = 1)
    {
        pagina = Math.Max(1, pagina);
        int total = _repoPrestamo.ObtenerCantidad(buscar);

        return Json(new
        {
            items = _repoPrestamo.ObtenerPaginado(buscar, pagina, TamPagina),
            pagina,
            totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)TamPagina))
        });
    }

    // GET: /Prestamos/BuscarEjemplares?term=...  (para el select del formulario)
    [HttpGet]
    public IActionResult BuscarEjemplares(string? term)
    {
        var resultado = _repoEjemplar.ObtenerDisponibles(term, 10)
            .Select(e => new { id = e.Id, text = $"{e.Libro?.Titulo} - Cód. #{e.CodigoInventario}" });
        return Json(resultado);
    }

    [HttpPost]
    public IActionResult Crear([FromBody] Prestamo prestamo)
    {
        if (prestamo == null) return BadRequest(new { message = "Datos inválidos." });

        prestamo.Id = 0;
        prestamo.FechaPrestamo = DateTime.Today;
        prestamo.FechaDevolucionReal = null;
        prestamo.Estado = Prestamo.EstadoEnProgreso;
        prestamo.CreadoPorUserId = UserId;
        prestamo.AnuladoPorUserId = null;
        prestamo.Activo = true;

        ModelState.Clear();
        TryValidateModel(prestamo);
        if (prestamo.FechaDevolucionPactada.Date <= prestamo.FechaPrestamo)
        {
            ModelState.AddModelError(nameof(prestamo.FechaDevolucionPactada),
                "La devolución pactada debe ser posterior a hoy.");
        }
        if (!ModelState.IsValid) return BadRequest(new { message = PrimerError() });

        var afiliado = _repoAfiliado.ObtenerPorId(prestamo.IdAfiliado);
        if (afiliado == null)
            return BadRequest(new { message = "El afiliado no existe." });

        if (afiliado.Sancionado)
            return BadRequest(new { message = "El afiliado está sancionado." });

        if (_repoSuscripcion.ObtenerVigente(afiliado.Id) == null)
            return BadRequest(new { message = "El afiliado no tiene una suscripción vigente y pagada." });

        if (_repoPrestamo.TieneVencidos(afiliado.Id))
            return BadRequest(new { message = "El afiliado tiene préstamos vencidos sin devolver." });

        try
        {
            int id = _repoPrestamo.Create(prestamo);
            if (id == 0)
                return Conflict(new { message = "El ejemplar ya no está disponible." });

            return Ok(new { id, message = "Préstamo registrado correctamente." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear préstamo del afiliado {IdAfiliado}", prestamo.IdAfiliado);
            return StatusCode(500, new { message = "No se pudo registrar el préstamo." });
        }
    }

    // POST: /Prestamos/Devolver?id=5
    [HttpPost]
    public IActionResult Devolver(int id)
    {
        var prestamo = _repoPrestamo.ObtenerPorId(id);
        if (prestamo == null)
            return NotFound(new { message = "El préstamo no existe." });

        if (prestamo.FechaDevolucionReal != null)
            return Conflict(new { message = "El préstamo ya fue devuelto." });

        // Si se pasó de la fecha pactada, se genera una multa
        var hoy = DateTime.Today;
        int diasAtraso = (hoy - prestamo.FechaDevolucionPactada.Date).Days;

        Pago? multa = null;
        if (diasAtraso > 0)
        {
            multa = new Pago
            {
                Tipo = TipoPago.Multa,
                IdAfiliado = prestamo.IdAfiliado,
                IdPrestamo = prestamo.Id,
                CreadoPorUserId = UserId,
                FechaPago = hoy,
                Monto = diasAtraso * MultaPorDia,
                Concepto = $"Multa por {diasAtraso} día(s) de atraso - Préstamo #{prestamo.Id}",
                Saldado = true
            };
        }

        try
        {
            if (!_repoPrestamo.Devolver(id, hoy, multa))
                return Conflict(new { message = "El préstamo ya fue devuelto." });

            var mensaje = multa == null
                ? "Devolución registrada."
                : $"Devolución registrada. Se generó una multa de ${multa.Monto:N2}.";
            return Ok(new { message = mensaje });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al devolver el préstamo {Id}", id);
            return StatusCode(500, new { message = "No se pudo registrar la devolución." });
        }
    }

    // POST: /Prestamos/Anular?id=5
    [HttpPost]
    public IActionResult Anular(int id)
    {
        try
        {
            return _repoPrestamo.Anular(id, UserId)
                ? Ok(new { message = "Préstamo anulado." })
                : Conflict(new { message = "Solo se pueden anular préstamos que todavía no se devolvieron." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al anular el préstamo {Id}", id);
            return StatusCode(500, new { message = "No se pudo anular el préstamo." });
        }
    }

    private string PrimerError() =>
        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                  .FirstOrDefault(m => !string.IsNullOrEmpty(m)) ?? "Datos inválidos.";
}