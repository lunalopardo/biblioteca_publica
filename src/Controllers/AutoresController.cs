using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BibliotecaPublica.Models;
using BibliotecaPublica.Repositories;

namespace BibliotecaPublica.Controllers;

[Authorize]
public class AutoresController : Controller
{
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
    public IActionResult Index(string? buscar = null, int pagina = 1)
    {
        int registrosPorPagina = 5;

        var listado = _repositorioAutor.ObtenerPaginado(buscar, pagina, registrosPorPagina);
        int totalRegistros = _repositorioAutor.ObtenerCantidad(buscar);
        int totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);

        ViewData["FiltroActual"] = buscar;
        ViewData["PaginaActual"] = pagina;
        ViewData["TotalPaginas"] = totalPaginas;

        return View(listado);
    }

    // GET: Autores/Details/5
    public IActionResult Details(int? id)
    {
        if (id == null) return NotFound();

        var autor = _repositorioAutor.GetById(id.Value);
        if (autor == null) return NotFound();

        return View(autor);
    }

    // GET: Autores/Create
    public IActionResult Create()
    {
        ViewBag.Nacionalidades = _repositorioNacionalidad.ObtenerTodas();
        return View();
    }

    // POST: Autores/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Autor autor)
    {
        ModelState.Remove(nameof(Autor.Foto));

        if (!ModelState.IsValid)
        {
            ViewBag.Nacionalidades = _repositorioNacionalidad.ObtenerTodas();
            return View(autor);
        }

        if (autor.FotoFile != null && autor.FotoFile.Length > 0)
        {
            string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "autores");
            Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(autor.FotoFile.FileName)}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await autor.FotoFile.CopyToAsync(fileStream);
            }

            autor.Foto = $"/uploads/autores/{uniqueFileName}";
        }

        autor.Activo = true;
        _repositorioAutor.Create(autor);
        TempData["Mensaje"] = "Autor registrado con éxito.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Autores/Edit/5
    public IActionResult Edit(int? id)
    {
        if (id == null) return NotFound();

        var autor = _repositorioAutor.GetById(id.Value);
        if (autor == null) return NotFound();

        ViewBag.Nacionalidades = _repositorioNacionalidad.ObtenerTodas();
        return View(autor);
    }

    // POST: Autores/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Autor autor)
    {
        if (id != autor.Id) return NotFound();

        ModelState.Remove(nameof(Autor.Foto));

        if (!ModelState.IsValid)
        {
            ViewBag.Nacionalidades = _repositorioNacionalidad.ObtenerTodas();
            return View(autor);
        }

        var autorExistente = _repositorioAutor.GetById(id);
        if (autorExistente == null) return NotFound();

        if (autor.FotoFile != null && autor.FotoFile.Length > 0)
        {
            // Borrar foto anterior si existía
            ImagenesController.BorrarArchivoFisico(autorExistente.Foto, _environment);

            string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "autores");
            Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(autor.FotoFile.FileName)}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await autor.FotoFile.CopyToAsync(fileStream);
            }

            autor.Foto = $"/uploads/autores/{uniqueFileName}";
        }
        else
        {
            autor.Foto = autorExistente.Foto;
        }

        autor.Activo = autorExistente.Activo;
        _repositorioAutor.Update(autor);
        TempData["Mensaje"] = "Autor actualizado con éxito.";
        return RedirectToAction(nameof(Index));
    }

    // POST: Autores/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public IActionResult Delete(int id)
    {
        _repositorioAutor.DeleteLogico(id);
        TempData["Mensaje"] = "Autor dado de baja correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Autores/Buscar
    [HttpGet]
    public IActionResult Buscar(string? q)
    {
        var autores = _repositorioAutor.ObtenerTodos();
        if (!string.IsNullOrWhiteSpace(q))
        {
            autores = autores.Where(a =>
                (a.Nombre + " " + a.Apellido).Contains(q, StringComparison.OrdinalIgnoreCase) ||
                a.Apellido.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                a.Nombre.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var resultado = autores.Select(a => new
        {
            id = a.Id,
            text = $"{a.Apellido}, {a.Nombre}"
        });

        return Json(resultado);
    }

}
