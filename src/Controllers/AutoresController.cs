using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BibliotecaPublica.Helpers;
using BibliotecaPublica.Models;
using BibliotecaPublica.Repositories;

namespace BibliotecaPublica.Controllers;

[Authorize]
[AutoValidateAntiforgeryToken]
public class AutoresController : Controller
{
    private const int TamPagina = 10;
    private const string CarpetaFotos = "autores";

    private readonly RepositorioAutor _repositorioAutor;
    private readonly RepositorioNacionalidad _repositorioNacionalidad;
    private readonly IWebHostEnvironment _environment;

    public AutoresController(
        RepositorioAutor repositorioAutor,
        RepositorioNacionalidad repositorioNacionalidad,
        IWebHostEnvironment environment)
    {
        _repositorioAutor = repositorioAutor;
        _repositorioNacionalidad = repositorioNacionalidad;
        _environment = environment;
    }

    // GET: Autores (con paginado)
    public IActionResult Index(string? buscar = null, int? idNacionalidad = null, int pagina = 1)
    {
        var listado = _repositorioAutor.ObtenerPaginado(buscar, pagina, TamPagina, idNacionalidad);
        this.CargarPaginado(buscar, pagina, _repositorioAutor.ObtenerCantidad(buscar, idNacionalidad), TamPagina);

        ViewBag.Nacionalidades = _repositorioNacionalidad.ObtenerTodos();
        ViewBag.IdNacionalidad = idNacionalidad;

        return View(listado);
    }

    // GET: Autores/Details/5
    public IActionResult Details(int? id)
    {
        if (id == null) return NotFound();

        var autor = _repositorioAutor.ObtenerPorId(id.Value);
        if (autor == null) return NotFound();

        return View(autor);
    }

    // GET: Autores/Create
    public IActionResult Create()
    {
        ViewBag.Nacionalidades = _repositorioNacionalidad.ObtenerTodos();
        return View();
    }

    // POST: Autores/Create
    [HttpPost]
    public async Task<IActionResult> Create(Autor autor)
    {
        ModelState.Remove(nameof(Autor.Foto));

        if (!ModelState.IsValid)
        {
            ViewBag.Nacionalidades = _repositorioNacionalidad.ObtenerTodos();
            return View(autor);
        }

        if (autor.FotoFile != null && autor.FotoFile.Length > 0)
        {
            autor.Foto = await Archivos.GuardarImagenAsync(autor.FotoFile, _environment, CarpetaFotos);
        }

        autor.Activo = true;
        _repositorioAutor.Crear(autor);
        TempData["Mensaje"] = "Autor registrado con éxito.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Autores/Edit/5
    public IActionResult Edit(int? id)
    {
        if (id == null) return NotFound();

        var autor = _repositorioAutor.ObtenerPorId(id.Value);
        if (autor == null) return NotFound();

        ViewBag.Nacionalidades = _repositorioNacionalidad.ObtenerTodos();
        return View(autor);
    }

    // POST: Autores/Edit/5
    [HttpPost]
    public async Task<IActionResult> Edit(int id, Autor autor)
    {
        if (id != autor.Id) return NotFound();

        ModelState.Remove(nameof(Autor.Foto));

        if (!ModelState.IsValid)
        {
            ViewBag.Nacionalidades = _repositorioNacionalidad.ObtenerTodos();
            return View(autor);
        }

        var autorExistente = _repositorioAutor.ObtenerPorId(id);
        if (autorExistente == null) return NotFound();

        string? fotoAnterior = autorExistente.Foto;

        if (autor.FotoFile != null && autor.FotoFile.Length > 0)
        {
            autor.Foto = await Archivos.GuardarImagenAsync(autor.FotoFile, _environment, CarpetaFotos);
        }
        else
        {
            autor.Foto = fotoAnterior;
        }

        autor.Activo = autorExistente.Activo;
        _repositorioAutor.Actualizar(autor);

        // El archivo viejo se borra recién cuando la base ya quedó actualizada
        if (autor.Foto != fotoAnterior)
        {
            Archivos.Borrar(fotoAnterior, _environment);
        }

        TempData["Mensaje"] = "Autor actualizado con éxito.";
        return RedirectToAction(nameof(Index));
    }

    // POST: Autores/Delete/5
    [HttpPost]
    [Authorize(Policy = UsuarioActual.PolicyAdministrador)]
    public IActionResult Delete(int id)
    {
        _repositorioAutor.BajaLogica(id);
        TempData["Mensaje"] = "Autor dado de baja correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Autores/Buscar (Select2 del formulario de libros; filtra y limita en SQL)
    [HttpGet]
    public IActionResult Buscar(string? q)
    {
        var resultado = _repositorioAutor.ObtenerParaSelect(q).Select(a => new
        {
            id = a.Id,
            text = $"{a.Apellido}, {a.Nombre}"
        });

        return Json(resultado);
    }
}
