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

    public HomeController(RepositorioLibro repositorioLibro, RepositorioAfiliado repositorioAfiliado, RepositorioAutor repositorioAutor)
    {
        _repositorioLibro = repositorioLibro;
        _repositorioAfiliado = repositorioAfiliado;
        _repositorioAutor = repositorioAutor;
    }

    public IActionResult Index()
    {
        var totalLibros = _repositorioLibro.ObtenerCantidad();
        var totalAfiliados = _repositorioAfiliado.ObtenerCantidad();
        var totalAutores = _repositorioAutor.ObtenerCantidad();
        
        var ultimosLibros = _repositorioLibro.ObtenerPaginado(null, 1, 5);

        var viewModel = new HomeViewModel
        {
            TotalLibros = totalLibros,
            TotalAfiliados = totalAfiliados,
            UltimosLibros = ultimosLibros,
            TotalAutores = totalAutores
        };

        return View(viewModel);
    }

    // Página a la que manda UseExceptionHandler en producción
    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}