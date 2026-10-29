using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;
using BibliotecaPublica.Models;
using BibliotecaPublica.Repositories;

namespace BibliotecaPublica.Controllers;

[Authorize]
public class LibrosController : Controller
{
    private readonly RepositorioLibro _repositorioLibro;
    private readonly RepositorioAutor _repositorioAutor;
    private readonly RepositorioGenero _repositorioGenero;
    private readonly RepositorioEditorial _repositorioEditorial;
    private readonly IWebHostEnvironment _environment;

    public LibrosController(
        RepositorioLibro repositorioLibro,
        RepositorioAutor repositorioAutor,
        RepositorioGenero repositorioGenero,
        RepositorioEditorial repositorioEditorial,
        IWebHostEnvironment environment)
    {
        _repositorioLibro = repositorioLibro;
        _repositorioAutor = repositorioAutor;
        _repositorioGenero = repositorioGenero;
        _repositorioEditorial = repositorioEditorial;
        _environment = environment;
    }

    // GET: /Libros
    public IActionResult Index(string? buscar, int pagina = 1)
    {
        int registrosPorPagina = 10;
        var listado = _repositorioLibro.ObtenerPaginado(buscar, pagina, registrosPorPagina);
        int totalRegistros = _repositorioLibro.ObtenerCantidad(buscar);
        int totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);

        ViewData["FiltroActual"] = buscar;
        ViewData["PaginaActual"] = pagina;
        ViewData["TotalPaginas"] = totalPaginas == 0 ? 1 : totalPaginas;

        return View(listado);
    }

    // GET: /Libros/Details/5
    public IActionResult Details(int id)
    {
        var libro = _repositorioLibro.ObtenerPorId(id);
        if (libro == null) return NotFound();
        return View(libro);
    }

    // GET: /Libros/Create
    public IActionResult Create()
    {
        ViewBag.Autores = _repositorioAutor.ObtenerTodos();
        ViewBag.Generos = _repositorioGenero.ObtenerTodos();
        ViewBag.Editoriales = _repositorioEditorial.ObtenerTodas();
        return View();
    }

    // POST: /Libros/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Libro libro)
    {
        ModelState.Remove(nameof(Libro.Portada));
        ModelState.Remove(nameof(Libro.Autor));
        ModelState.Remove(nameof(Libro.Editorial));
        ModelState.Remove(nameof(Libro.Generos));
        ModelState.Remove(nameof(Libro.Ejemplares));

        if (!ModelState.IsValid)
        {
            ViewBag.Autores = _repositorioAutor.ObtenerTodos();
            ViewBag.Generos = _repositorioGenero.ObtenerTodos();
            ViewBag.Editoriales = _repositorioEditorial.ObtenerTodas();
            return View(libro);
        }

        if (libro.PortadaFile != null && libro.PortadaFile.Length > 0)
        {
            libro.Portada = await ImagenesController.GuardarArchivoAsync(libro.PortadaFile, _environment, "libros");
        }

        _repositorioLibro.Create(libro);
        TempData["Mensaje"] = "Libro registrado exitosamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Libros/Edit/5
    public IActionResult Edit(int id)
    {
        var libro = _repositorioLibro.ObtenerPorId(id);
        if (libro == null) return NotFound();

        ViewBag.Autores = _repositorioAutor.ObtenerTodos();
        ViewBag.Generos = _repositorioGenero.ObtenerTodos();
        ViewBag.Editoriales = _repositorioEditorial.ObtenerTodas();
        return View(libro);
    }

    // POST: /Libros/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Libro libro)
    {
        if (id != libro.Id) return NotFound();

        ModelState.Remove(nameof(Libro.Portada));
        ModelState.Remove(nameof(Libro.Autor));
        ModelState.Remove(nameof(Libro.Editorial));
        ModelState.Remove(nameof(Libro.Generos));
        ModelState.Remove(nameof(Libro.Ejemplares));

        var libroExistente = _repositorioLibro.ObtenerPorId(id);
        if (libroExistente == null) return NotFound();

        if (!ModelState.IsValid)
        {
            ViewBag.Autores = _repositorioAutor.ObtenerTodos();
            ViewBag.Generos = _repositorioGenero.ObtenerTodos();
            ViewBag.Editoriales = _repositorioEditorial.ObtenerTodas();
            return View(libro);
        }

        if (libro.PortadaFile != null && libro.PortadaFile.Length > 0)
        {
            ImagenesController.BorrarArchivoFisico(libroExistente.Portada, _environment);
            libro.Portada = await ImagenesController.GuardarArchivoAsync(libro.PortadaFile, _environment, "libros");
        }
        else
        {
            libro.Portada = libroExistente.Portada;
        }

        _repositorioLibro.Update(libro);
        TempData["Mensaje"] = "Libro actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Libros/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        _repositorioLibro.AnularLogico(id);
        TempData["Mensaje"] = "Libro eliminado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Libros/BuscarExterno
    [HttpGet]
    public async Task<IActionResult> BuscarExterno(string? q, int pagina = 1)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return View(new List<JsonObject>());
        }

        int resultadosPorPagina = 10;
        using var client = new HttpClient();
        string url = $"https://openlibrary.org/search.json?q={Uri.EscapeDataString(q)}&page={pagina}&limit={resultadosPorPagina}";

        try
        {
            var response = await client.GetFromJsonAsync<JsonObject>(url);
            var docs = response?["docs"]?.AsArray();
            int numFound = response?["num_found"]?.GetValue<int>() ?? 0;

            var resultados = new List<JsonObject>();
            if (docs != null)
            {
                foreach (var doc in docs)
                {
                    if (doc is JsonObject obj)
                    {
                        resultados.Add(obj);
                    }
                }
            }

            int totalPaginas = (int)Math.Ceiling((double)numFound / resultadosPorPagina);
            if (totalPaginas > 20) totalPaginas = 20;

            ViewData["FiltroExterno"] = q;
            ViewData["PaginaActual"] = pagina;
            ViewData["TotalPaginas"] = totalPaginas;

            return View(resultados);
        }
        catch (Exception)
        {
            TempData["Error"] = "No se pudo establecer conexión con Open Library.";
            return View(new List<JsonObject>());
        }
    }

    // POST: /Libros/ImportarExterno
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ImportarExterno(string titulo, string? autorNombre, int? anio, string? isbn, int? paginas, string? portadaUrl, string? editorial)
    {
        int idAutorFinal;

        // Manejo del autor
        if (!string.IsNullOrWhiteSpace(autorNombre))
        {
            string nombreAutor = autorNombre.Trim();
            string apellidoAutor = "Desconocido";
            int espacioIdx = nombreAutor.LastIndexOf(' ');
            if (espacioIdx > 0)
            {
                apellidoAutor = nombreAutor.Substring(espacioIdx + 1);
                nombreAutor = nombreAutor.Substring(0, espacioIdx);
            }

            var autorExistente = _repositorioAutor.ObtenerTodos()
                .FirstOrDefault(a => (a.Nombre + " " + a.Apellido).Contains(autorNombre, StringComparison.OrdinalIgnoreCase));

            if (autorExistente != null)
            {
                idAutorFinal = autorExistente.Id;
            }
            else
            {
                var nuevoAutor = new Autor
                {
                    Nombre = nombreAutor,
                    Apellido = apellidoAutor,
                    IdNacionalidad = 0
                };
                idAutorFinal = _repositorioAutor.Create(nuevoAutor);
            }
        }
        else
        {
            TempData["Error"] = "El libro seleccionado no posee información de autor válida.";
            return RedirectToAction(nameof(BuscarExterno));
        }

        // Manejo de la editorial
        int? idEditorialFinal = null;
        if (!string.IsNullOrWhiteSpace(editorial))
        {
            string nombreEditorial = editorial.Trim();
            var editorialExistente = _repositorioEditorial.ObtenerTodas()
                .FirstOrDefault(e => e.Nombre.Equals(nombreEditorial, StringComparison.OrdinalIgnoreCase));

            if (editorialExistente != null)
            {
                idEditorialFinal = editorialExistente.Id;
            }
            else
            {
                var nuevaEditorial = new Editorial
                {
                    Nombre = nombreEditorial.Length > 100 ? nombreEditorial.Substring(0, 100) : nombreEditorial
                };
                idEditorialFinal = _repositorioEditorial.Create(nuevaEditorial);
            }
        }

        // Crear el libro vinculando las entidades
        var nuevoLibro = new Libro
        {
            Titulo = titulo ?? "Sin Título",
            IdAutor = idAutorFinal,
            IdEditorial = idEditorialFinal,
            AnioPublicacion = anio,
            Isbn = string.IsNullOrEmpty(isbn) ? null : (isbn.Length > 20 ? isbn.Substring(0, 20) : isbn),
            Paginas = paginas,
            Portada = portadaUrl
        };

        _repositorioLibro.Create(nuevoLibro);
        TempData["Mensaje"] = $"¡Libro '{nuevoLibro.Titulo}' importado correctamente!";
        return RedirectToAction(nameof(Index));
    }
}