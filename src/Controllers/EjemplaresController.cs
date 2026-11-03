using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using BibliotecaPublica.Models;
using BibliotecaPublica.Repositories;

namespace BibliotecaPublica.Controllers;

[Authorize]
[AutoValidateAntiforgeryToken]
[Route("Ejemplares/[action]")]
public class EjemplaresController : Controller
{
    private readonly RepositorioEjemplar _repoEjemplar;
    private readonly ILogger<EjemplaresController> _logger;

    public EjemplaresController(RepositorioEjemplar repoEjemplar, ILogger<EjemplaresController> logger)
    {
        _repoEjemplar = repoEjemplar;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult ObtenerPorLibro(int idLibro) => Json(_repoEjemplar.ObtenerPorLibro(idLibro));

    [HttpPost]
    public IActionResult CrearAjax([FromBody] Ejemplar ejemplar)
    {
        if (!ModelState.IsValid) return BadRequest(new { message = PrimerError() });

        // Un ejemplar nuevo siempre empieza disponible; después lo cambian los préstamos
        ejemplar.Estado = EstadoEjemplar.Disponible.ToString();

        try
        {
            var id = _repoEjemplar.Create(ejemplar);
            return Ok(new { id, message = "Ejemplar creado exitosamente." });
        }
        catch (MySqlException ex) when (ex.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
        {
            return Conflict(new { message = "Ya existe un ejemplar con ese código de inventario." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear ejemplar para el libro {IdLibro}", ejemplar.IdLibro);
            return StatusCode(500, new { message = "No se pudo crear el ejemplar." });
        }
    }

    [HttpPost]
    public IActionResult EditarAjax([FromBody] Ejemplar ejemplar)
    {
        if (ejemplar.Id <= 0) ModelState.AddModelError(nameof(ejemplar.Id), "Ejemplar inválido.");
        if (!ModelState.IsValid) return BadRequest(new { message = PrimerError() });

        try
        {
            // El repositorio solo actualiza código y ubicación; el estado no se toca
            return _repoEjemplar.Update(ejemplar) > 0
                ? Ok(new { message = "Ejemplar actualizado exitosamente." })
                : NotFound(new { message = "El ejemplar no existe." });
        }
        catch (MySqlException ex) when (ex.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
        {
            return Conflict(new { message = "Ya existe un ejemplar con ese código de inventario." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar ejemplar {Id}", ejemplar.Id);
            return StatusCode(500, new { message = "No se pudo actualizar el ejemplar." });
        }
    }

    [HttpPost]
    public IActionResult EliminarAjax(int id)
    {
        try
        {
            // Un ejemplar prestado no se puede dar de baja: primero hay que registrar la devolución
            if (_repoEjemplar.EstaPrestado(id))
            {
                return Conflict(new
                {
                    message = "No se puede eliminar: el ejemplar está prestado. Registrá primero la devolución."
                });
            }

            return _repoEjemplar.AnularLogico(id) > 0
                ? Ok(new { message = "Ejemplar eliminado." })
                : NotFound(new { message = "El ejemplar no existe." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar ejemplar {Id}", id);
            return StatusCode(500, new { message = "No se pudo eliminar el ejemplar." });
        }
    }

    private string PrimerError() =>
        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                  .FirstOrDefault(m => !string.IsNullOrEmpty(m)) ?? "Datos inválidos.";
}