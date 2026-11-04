using BibliotecaPublica.Models;
using BibliotecaPublica.Repositories;

namespace BibliotecaPublica.Services;

public record ResultadoPrestamo(int Estado, string Mensaje, int? Id = null);

// Reglas de préstamos que comparten la página de Préstamos (Vue) y la Web API
public class ServicioPrestamo
{
    public const decimal MultaPorDia = 100m;

    private readonly RepositorioPrestamo _repositorioPrestamo;
    private readonly RepositorioAfiliado _repositorioAfiliado;
    private readonly RepositorioSuscripcion _repositorioSuscripcion;
    private readonly ILogger<ServicioPrestamo> _logger;

    public ServicioPrestamo(
        RepositorioPrestamo repositorioPrestamo,
        RepositorioAfiliado repositorioAfiliado,
        RepositorioSuscripcion repositorioSuscripcion,
        ILogger<ServicioPrestamo> logger)
    {
        _repositorioPrestamo = repositorioPrestamo;
        _repositorioAfiliado = repositorioAfiliado;
        _repositorioSuscripcion = repositorioSuscripcion;
        _logger = logger;
    }

    // Los ids ya vienen validados por el modelo
    public ResultadoPrestamo Crear(Prestamo prestamo, int userId)
    {
        prestamo.Id = 0;
        prestamo.FechaPrestamo = DateTime.Today;
        prestamo.FechaDevolucionReal = null;
        prestamo.Estado = Prestamo.EstadoEnProgreso;
        prestamo.CreadoPorUserId = userId;
        prestamo.AnuladoPorUserId = null;
        prestamo.Activo = true;

        if (prestamo.FechaDevolucionPactada.Date <= prestamo.FechaPrestamo)
            return new(StatusCodes.Status400BadRequest, "La devolución pactada debe ser posterior a hoy.");

        var afiliado = _repositorioAfiliado.ObtenerPorId(prestamo.IdAfiliado);
        if (afiliado == null)
            return new(StatusCodes.Status400BadRequest, "El afiliado no existe.");

        // Sancionado: tiene préstamos vencidos sin devolver
        if (afiliado.Sancionado)
            return new(StatusCodes.Status400BadRequest, "El afiliado está sancionado: tiene préstamos vencidos sin devolver.");

        if (_repositorioSuscripcion.ObtenerVigente(afiliado.Id) == null)
            return new(StatusCodes.Status400BadRequest, "El afiliado no tiene una suscripción vigente y pagada.");

        try
        {
            int id = _repositorioPrestamo.Crear(prestamo);
            return id == 0
                ? new(StatusCodes.Status409Conflict, "El ejemplar ya no está disponible.")
                : new(StatusCodes.Status200OK, "Préstamo registrado correctamente.", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear préstamo del afiliado {IdAfiliado}", prestamo.IdAfiliado);
            return new(StatusCodes.Status500InternalServerError, "No se pudo registrar el préstamo.");
        }
    }

    // Si se pasó de la fecha pactada, se genera una multa
    public ResultadoPrestamo Devolver(int id, int userId)
    {
        var prestamo = _repositorioPrestamo.ObtenerPorId(id);
        if (prestamo == null)
            return new(StatusCodes.Status404NotFound, "El préstamo no existe.");

        if (prestamo.FechaDevolucionReal != null)
            return new(StatusCodes.Status409Conflict, "El préstamo ya fue devuelto.");

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
                CreadoPorUserId = userId,
                FechaPago = hoy,
                Monto = diasAtraso * MultaPorDia,
                Concepto = $"Multa por {diasAtraso} día(s) de atraso - Préstamo #{prestamo.Id}",
                Saldado = true
            };
        }

        try
        {
            if (!_repositorioPrestamo.Devolver(id, hoy, multa))
                return new(StatusCodes.Status409Conflict, "El préstamo ya fue devuelto.");

            var mensaje = multa == null
                ? "Devolución registrada."
                : $"Devolución registrada. Se generó una multa de ${multa.Monto:N2}.";
            return new(StatusCodes.Status200OK, mensaje, id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al devolver el préstamo {Id}", id);
            return new(StatusCodes.Status500InternalServerError, "No se pudo registrar la devolución.");
        }
    }

    public ResultadoPrestamo Anular(int id, int userId)
    {
        try
        {
            return _repositorioPrestamo.Anular(id, userId)
                ? new(StatusCodes.Status200OK, "Préstamo anulado.", id)
                : new(StatusCodes.Status409Conflict, "Solo se pueden anular préstamos que todavía no se devolvieron.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al anular el préstamo {Id}", id);
            return new(StatusCodes.Status500InternalServerError, "No se pudo anular el préstamo.");
        }
    }
}
