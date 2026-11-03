using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using BibliotecaPublica.Helpers;
using BibliotecaPublica.Models;
using BibliotecaPublica.Repositories;

namespace BibliotecaPublica.Controllers;

[Authorize]
[AutoValidateAntiforgeryToken]
[Route("Ejemplares/[action]")]
public class EjemplaresController : Controller
{
    private readonly RepositorioEjemplar _repositorioEjemplar;
    private readonly ILogger<EjemplaresController> _logger;

    public EjemplaresController(RepositorioEjemplar repositorioEjemplar, ILogger<EjemplaresController> logger)
    {
        _repositorioEjemplar = repositorioEjemplar;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult ObtenerPorLibro(int idLibro) => Json(_repositorioEjemplar.ObtenerPorLibro(idLibro));

    [HttpPost]
    public IActionResult CrearAjax([FromBody] Ejemplar ejemplar)
    {
        if (!ModelState.IsValid) return BadRequest(new { message = PrimerError() });

        // Un ejemplar nuevo siempre empieza disponible; después lo cambian los préstamos
        ejemplar.Estado = EstadoEjemplar.Disponible.ToString();

        try
        {
            var id = _repositorioEjemplar.Crear(ejemplar);
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
            return _repositorioEjemplar.Actualizar(ejemplar) > 0
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

    // Solo administradores
    [HttpPost]
    [Authorize(Policy = UsuarioActual.PolicyAdministrador)]
    public IActionResult EliminarAjax(int id)
    {
        try
        {
            // Un ejemplar prestado no se puede dar de baja: primero hay que registrar la devolución
            if (_repositorioEjemplar.EstaPrestado(id))
            {
                return Conflict(new
                {
                    message = "No se puede eliminar: el ejemplar está prestado. Registrá primero la devolución."
                });
            }

            return _repositorioEjemplar.BajaLogica(id) > 0
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