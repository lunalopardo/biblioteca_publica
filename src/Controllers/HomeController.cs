using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BibliotecaPublica.Models;
using BibliotecaPublica.Repositories;

namespace BibliotecaPublica.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly RepositorioLibro _repositorioLibro;
    private readonly RepositorioAfiliado _repositorioAfiliado;
    private readonly RepositorioAutor _repositorioAutor;
    private readonly RepositorioPrestamo _repositorioPrestamo;

    public HomeController(RepositorioLibro repositorioLibro, RepositorioAfiliado repositorioAfiliado,
        RepositorioAutor repositorioAutor, RepositorioPrestamo repositorioPrestamo)
    {
        _repositorioLibro = repositorioLibro;
        _repositorioAfiliado = repositorioAfiliado;
        _repositorioAutor = repositorioAutor;
        _repositorioPrestamo = repositorioPrestamo;
    }

    public IActionResult Index()
    {
        var viewModel = new HomeViewModel
        {
            TotalLibros = _repositorioLibro.ObtenerCantidad(),
            TotalAfiliados = _repositorioAfiliado.ObtenerCantidad(),
            TotalAutores = _repositorioAutor.ObtenerCantidad(),
            TotalPrestamosVigentes = _repositorioPrestamo.ObtenerCantidadVigentes(),
            UltimosLibros = _repositorioLibro.ObtenerPaginado(null, 1, 5)
        };

        return View(viewModel);
    }

    // Página de error HTTP (404 y otros)
    [AllowAnonymous]
    public IActionResult Estado(int codigo)
    {
        // Las llamadas AJAX esperan JSON, no una página
        if (Request.Headers.XRequestedWith == "XMLHttpRequest")
        {
            string mensaje = codigo == StatusCodes.Status404NotFound
                ? "Lo que buscás no existe."
                : "Ocurrió un error inesperado.";
            return Json(new { message = mensaje });
        }

        return View(codigo);
    }

    // Página a la que manda UseExceptionHandler en producción
    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}