using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BibliotecaPublica.Helpers;
using BibliotecaPublica.Models;
using BibliotecaPublica.Repositories;
using BibliotecaPublica.Services;

namespace BibliotecaPublica.Controllers;

[Authorize]
[AutoValidateAntiforgeryToken]
public class PrestamosController : Controller
{
    private const int TamPagina = 15;

    private readonly RepositorioPrestamo _repositorioPrestamo;
    private readonly RepositorioAfiliado _repositorioAfiliado;
    private readonly RepositorioEjemplar _repositorioEjemplar;
    private readonly ServicioPrestamo _servicioPrestamo;

    public PrestamosController(
        RepositorioPrestamo repositorioPrestamo,
        RepositorioAfiliado repositorioAfiliado,
        RepositorioEjemplar repositorioEjemplar,
        ServicioPrestamo servicioPrestamo)
    {
        _repositorioPrestamo = repositorioPrestamo;
        _repositorioAfiliado = repositorioAfiliado;
        _repositorioEjemplar = repositorioEjemplar;
        _servicioPrestamo = servicioPrestamo;
    }

    // Id del usuario logueado
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // GET: /Prestamos - solo muestra la página; los datos los pide Vue
    // GET: /Prestamos?idAfiliado=3 - solo los préstamos de ese afiliado (desde el listado de afiliados)
    public IActionResult Index(int? idAfiliado)
    {
        if (idAfiliado.HasValue)
        {
            var afiliado = _repositorioAfiliado.ObtenerPorId(idAfiliado.Value);
            if (afiliado == null) return NotFound();

            ViewData["Afiliado"] = afiliado;
        }

        return View();
    }

    // GET: /Prestamos/Listar?buscar=...&idAfiliado=3&vencimiento=vencidos&pagina=1
    // vencimiento para el filtro
    [HttpGet]
    public IActionResult Listar(string? buscar, int? idAfiliado, string? vencimiento, int pagina = 1)
    {
        pagina = Math.Max(1, pagina);
        bool soloVencidos = vencimiento == "vencidos";
        int? venceEnDias = vencimiento is "5" or "10" or "15" ? int.Parse(vencimiento) : null;
        int total = _repositorioPrestamo.ObtenerCantidad(buscar, idAfiliado, soloVencidos, venceEnDias);

        return Json(new
        {
            items = _repositorioPrestamo.ObtenerPaginado(buscar, idAfiliado, pagina, TamPagina, soloVencidos, venceEnDias),
            pagina,
            totalPaginas = Paginacion.TotalPaginas(total, TamPagina)
        });
    }

    // GET: /Prestamos/BuscarEjemplares?term=...  (para el select del formulario)
    [HttpGet]
    public IActionResult BuscarEjemplares(string? term)
    {
        var resultado = _repositorioEjemplar.ObtenerDisponibles(term, 10)
            .Select(e => new { id = e.Id, text = $"{e.Libro?.Titulo} - Cód. #{e.CodigoInventario}" });
        return Json(resultado);
    }

    [HttpPost]
    public IActionResult Crear([FromBody] Prestamo prestamo)
    {
        if (prestamo == null) return BadRequest(new { message = "Datos inválidos." });

        ModelState.Clear();
        TryValidateModel(prestamo);
        if (!ModelState.IsValid) return BadRequest(new { message = PrimerError() });

        return Responder(_servicioPrestamo.Crear(prestamo, UserId));
    }

    // POST: /Prestamos/Devolver?id=5
    [HttpPost]
    public IActionResult Devolver(int id) => Responder(_servicioPrestamo.Devolver(id, UserId));

    // POST: /Prestamos/Anular?id=5
    [HttpPost]
    public IActionResult Anular(int id) => Responder(_servicioPrestamo.Anular(id, UserId));

    private IActionResult Responder(ResultadoPrestamo resultado) =>
        StatusCode(resultado.Estado, new { id = resultado.Id, message = resultado.Mensaje });

    private string PrimerError() =>
        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                  .FirstOrDefault(m => !string.IsNullOrEmpty(m)) ?? "Datos inválidos.";
}