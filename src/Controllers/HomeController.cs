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

    public IActionResult Privacy()
    {
        return View();
    }
}