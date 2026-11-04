using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BibliotecaPublica.Helpers;
using BibliotecaPublica.Models;
using BibliotecaPublica.Repositories;
using BibliotecaPublica.Services;

namespace BibliotecaPublica.Controllers.Api;

// Mismas reglas que la página de Préstamos: las aplica ServicioPrestamo
[ApiController]
[Route("api/prestamos")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class PrestamosApiController : ControllerBase
{
    private const int TamPagina = 10;

    private readonly RepositorioPrestamo _repositorioPrestamo;
    private readonly ServicioPrestamo _servicioPrestamo;

    public PrestamosApiController(RepositorioPrestamo repositorioPrestamo, ServicioPrestamo servicioPrestamo)
    {
        _repositorioPrestamo = repositorioPrestamo;
        _servicioPrestamo = servicioPrestamo;
    }

    // GET: /api/prestamos?buscar=...&idAfiliado=3&pagina=1
    [HttpGet]
    public IActionResult Listar(string? buscar, int? idAfiliado, int pagina = 1)
    {
        pagina = Math.Max(1, pagina);
        int total = _repositorioPrestamo.ObtenerCantidad(buscar, idAfiliado);

        return Ok(new
        {
            items = _repositorioPrestamo.ObtenerPaginado(buscar, idAfiliado, pagina, TamPagina),
            pagina,
            totalPaginas = Paginacion.TotalPaginas(total, TamPagina)
        });
    }

    // GET: /api/prestamos/5
    [HttpGet("{id:int}")]
    public IActionResult Obtener(int id)
    {
        var prestamo = _repositorioPrestamo.ObtenerPorId(id);
        return prestamo == null
            ? NotFound(new { message = "El préstamo no existe." })
            : Ok(prestamo);
    }

    // POST: /api/prestamos  { "idAfiliado": 3, "idEjemplar": 7, "fechaDevolucionPactada": "2026-11-20" }
    [HttpPost]
    public IActionResult Crear(Prestamo prestamo)
    {
        var resultado = _servicioPrestamo.Crear(prestamo, User.ObtenerId());

        return resultado.Estado == StatusCodes.Status200OK
            ? CreatedAtAction(nameof(Obtener), new { id = resultado.Id },
                              new { id = resultado.Id, message = resultado.Mensaje })
            : Responder(resultado);
    }

    // POST: /api/prestamos/5/devolucion (si hay atraso, genera la multa)
    [HttpPost("{id:int}/devolucion")]
    public IActionResult Devolver(int id) => Responder(_servicioPrestamo.Devolver(id, User.ObtenerId()));

    // DELETE: /api/prestamos/5 (anula un préstamo que todavía no se devolvió)
    [HttpDelete("{id:int}")]
    public IActionResult Anular(int id) => Responder(_servicioPrestamo.Anular(id, User.ObtenerId()));

    private IActionResult Responder(ResultadoPrestamo resultado) =>
        StatusCode(resultado.Estado, new { id = resultado.Id, message = resultado.Mensaje });
}
