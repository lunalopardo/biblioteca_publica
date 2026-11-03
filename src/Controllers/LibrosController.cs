using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;
using BibliotecaPublica.Helpers;
using BibliotecaPublica.Models;
using BibliotecaPublica.Repositories;

namespace BibliotecaPublica.Controllers;

[Authorize]
[AutoValidateAntiforgeryToken]
public class LibrosController : Controller
{
    private const int TamPagina = 10;
    private const int ResultadosExternosPorPagina = 10;
    private const int MaxPaginasExternas = 20;
    private const string CarpetaPortadas = "libros";

    private readonly RepositorioLibro _repositorioLibro;
    private readonly RepositorioAutor _repositorioAutor;
    private readonly RepositorioGenero _repositorioGenero;
    private readonly RepositorioEditorial _repositorioEditorial;
    private readonly IWebHostEnvironment _environment;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<LibrosController> _logger;

    public LibrosController(
        RepositorioLibro repositorioLibro,
        RepositorioAutor repositorioAutor,
        RepositorioGenero repositorioGenero,
        RepositorioEditorial repositorioEditorial,
        IWebHostEnvironment environment,
        IHttpClientFactory httpClientFactory,
        ILogger<LibrosController> logger)
    {
        _repositorioLibro = repositorioLibro;
        _repositorioAutor = repositorioAutor;
        _repositorioGenero = repositorioGenero;
        _repositorioEditorial = repositorioEditorial;
        _environment = environment;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    // Autores y editoriales se buscan por AJAX (Select2), así que acá solo
    // hace falta la lista de géneros, que es una tabla chica.
    private void CargarGeneros() => ViewBag.Generos = _repositorioGenero.ObtenerTodos();

    private void CargarRelacionesElegidas(Libro libro)
    {
        libro.Autor = libro.IdAutor > 0 ? _repositorioAutor.ObtenerPorId(libro.IdAutor) : null;
        libro.Editorial = libro.IdEditorial.HasValue
            ? _repositorioEditorial.ObtenerPorId(libro.IdEditorial.Value)
            : null;
    }

    private static string Recortar(string texto, int maximo) =>
        texto.Length > maximo ? texto[..maximo] : texto;

    // GET: /Libros
    public IActionResult Index(string? buscar, int pagina = 1)
    {
        var listado = _repositorioLibro.ObtenerPaginado(buscar, pagina, TamPagina);
        this.CargarPaginado(buscar, pagina, _repositorioLibro.ObtenerCantidad(buscar), TamPagina);

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
        CargarGeneros();
        return View();
    }

    // POST: /Libros/Create
    [HttpPost]
    public async Task<IActionResult> Create(Libro libro)
    {
        if (!ModelState.IsValid)
        {
            CargarGeneros();
            return View(libro);
        }

        if (libro.PortadaFile != null && libro.PortadaFile.Length > 0)
        {
            libro.Portada = await Archivos.GuardarImagenAsync(libro.PortadaFile, _environment, CarpetaPortadas);
        }

        int idLibroCreado = _repositorioLibro.Crear(libro);

        if (idLibroCreado > 0 && libro.GenerosSeleccionados.Count > 0)
        {
            _repositorioLibro.GuardarGeneros(idLibroCreado, libro.GenerosSeleccionados);
        }

        TempData["Mensaje"] = "Libro registrado exitosamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Libros/Edit/5
    public IActionResult Edit(int id)
    {
        var libro = _repositorioLibro.ObtenerPorId(id);
        if (libro == null) return NotFound();

        if (libro.Generos != null)
        {
            libro.GenerosSeleccionados = libro.Generos.Select(g => g.Id).ToList();
        }

        CargarGeneros();
        return View(libro);
    }

    // POST: /Libros/Edit/5
    [HttpPost]
    public async Task<IActionResult> Edit(int id, Libro libro)
    {
        if (id != libro.Id) return NotFound();

        var libroExistente = _repositorioLibro.ObtenerPorId(id);
        if (libroExistente == null) return NotFound();

        if (!ModelState.IsValid)
        {
            libro.Portada = libroExistente.Portada;
            CargarRelacionesElegidas(libro);
            CargarGeneros();
            return View(libro);
        }
        string? portadaAnterior = libroExistente.Portada;

        if (libro.PortadaFile != null && libro.PortadaFile.Length > 0)
        {
            libro.Portada = await Archivos.GuardarImagenAsync(libro.PortadaFile, _environment, CarpetaPortadas);
        }
        else if (libro.EliminarPortada)
        {
            libro.Portada = null;
        }
        else
        {
            libro.Portada = portadaAnterior;
        }

        _repositorioLibro.Actualizar(libro);
        _repositorioLibro.GuardarGeneros(libro.Id, libro.GenerosSeleccionados);

        // El archivo viejo se borra recién cuando la base ya quedó actualizada
        if (libro.Portada != portadaAnterior)
        {
            Archivos.Borrar(portadaAnterior, _environment);
        }

        TempData["Mensaje"] = "Libro actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Libros/Delete/5 (solo administradores)
    [HttpPost]
    [Authorize(Policy = UsuarioActual.PolicyAdministrador)]
    public IActionResult Delete(int id)
    {
        if (_repositorioLibro.TienePrestamosEnCurso(id))
        {
            TempData["Error"] = "No se puede dar de baja el libro: tiene ejemplares prestados. Registrá primero las devoluciones.";
            return RedirectToAction(nameof(Index));
        }

        _repositorioLibro.BajaLogica(id);
        TempData["Mensaje"] = "Libro eliminado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // Open Library

    private HttpClient CrearClienteOpenLibrary()
    {
        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(10);
        // Open Library pide que las aplicaciones se identifiquen
        client.DefaultRequestHeaders.UserAgent.ParseAdd("BibliotecaPublica/1.0 (proyecto academico)");
        return client;
    }

    // GET: /Libros/BuscarExterno
    [HttpGet]
    public async Task<IActionResult> BuscarExterno(string? buscar, int pagina = 1)
    {
        if (string.IsNullOrWhiteSpace(buscar))
        {
            return View(new List<LibroExterno>());
        }

        // Se normaliza acá porque la página se manda a la API de Open Library
        pagina = Math.Max(1, pagina);
        var client = CrearClienteOpenLibrary();
        string url = $"{OpenLibrary.UrlBase}/search.json?q={Uri.EscapeDataString(buscar)}" +
                     $"&page={pagina}&limit={ResultadosExternosPorPagina}&fields={OpenLibrary.CamposBusqueda}";

        try
        {
            var response = await client.GetFromJsonAsync<JsonObject>(url);
            int numFound = response?["num_found"]?.GetValue<int>() ?? 0;

            var resultados = (response?["docs"] as JsonArray)?
                .OfType<JsonObject>()
                .Select(OpenLibrary.Parsear)
                .ToList() ?? new List<LibroExterno>();

            // Open Library puede devolver miles de resultados: se muestran como mucho MaxPaginasExternas páginas
            int totalMostrable = Math.Min(numFound, MaxPaginasExternas * ResultadosExternosPorPagina);
            this.CargarPaginado(buscar, pagina, totalMostrable, ResultadosExternosPorPagina);

            return View(resultados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar Open Library con la búsqueda {Busqueda}", buscar);
            TempData["Error"] = "No se pudo establecer conexión con Open Library.";
            return View(new List<LibroExterno>());
        }
    }

    // POST: /Libros/ImportarExterno
    [HttpPost]
    public async Task<IActionResult> ImportarExterno(LibroExterno externo)
    {
        if (string.IsNullOrWhiteSpace(externo.AutorNombre))
        {
            TempData["Error"] = "El libro seleccionado no posee información de autor válida.";
            return RedirectToAction(nameof(BuscarExterno));
        }

        int idAutor = ObtenerOCrearAutor(externo.AutorNombre);
        int? idEditorial = ObtenerOCrearEditorial(externo.Editorial);
        string? descripcion = await ObtenerDescripcionAsync(externo.WorkKey);

        string titulo = string.IsNullOrWhiteSpace(externo.Titulo) ? "Sin título" : Recortar(externo.Titulo.Trim(), 255);

        string? portada = !string.IsNullOrWhiteSpace(externo.PortadaUrl)
                          && externo.PortadaUrl.StartsWith(OpenLibrary.PrefijoPortadas, StringComparison.OrdinalIgnoreCase)
            ? externo.PortadaUrl
            : null;

        var nuevoLibro = new Libro
        {
            Titulo = titulo,
            IdAutor = idAutor,
            IdEditorial = idEditorial,
            AnioPublicacion = externo.Anio is >= 1000 and <= 2100 ? externo.Anio : null,
            Paginas = externo.Paginas is >= 1 and <= 10000 ? externo.Paginas : null,
            Isbn = string.IsNullOrWhiteSpace(externo.Isbn) ? null : Recortar(externo.Isbn.Trim(), 20),
            Idioma = OpenLibrary.EsIdiomaConocido(externo.Idioma) ? externo.Idioma : null,
            Descripcion = descripcion,
            Portada = portada
        };

        _repositorioLibro.Crear(nuevoLibro);
        TempData["Mensaje"] = $"¡Libro '{nuevoLibro.Titulo}' importado correctamente!";
        return RedirectToAction(nameof(Index));
    }

    // Busca el autor por nombre y apellido; si no existe, lo crea
    private int ObtenerOCrearAutor(string autorNombre)
    {
        string nombre = autorNombre.Trim();
        string apellido = "Desconocido";

        int ultimoEspacio = nombre.LastIndexOf(' ');
        if (ultimoEspacio > 0)
        {
            apellido = nombre[(ultimoEspacio + 1)..];
            nombre = nombre[..ultimoEspacio];
        }

        nombre = Recortar(nombre, 100);
        apellido = Recortar(apellido, 100);

        var existente = _repositorioAutor.ObtenerPorNombre(nombre, apellido);
        if (existente != null) return existente.Id;

        return _repositorioAutor.Crear(new Autor
        {
            Nombre = nombre,
            Apellido = apellido,
            IdNacionalidad = 0 // "Desconocida"
        });
    }

    // Busca la editorial por nombre; si no existe, la crea
    private int? ObtenerOCrearEditorial(string? editorial)
    {
        if (string.IsNullOrWhiteSpace(editorial)) return null;

        string nombre = Recortar(editorial.Trim(), 100);

        var existente = _repositorioEditorial.ObtenerPorNombre(nombre);
        if (existente != null) return existente.Id;

        return _repositorioEditorial.Crear(new Editorial { Nombre = nombre });
    }

    // La búsqueda de Open Library no trae la descripción: hay que pedirla a la página de la obra.
    // Si falla, el libro se importa igual, solo que sin descripción.
    private async Task<string?> ObtenerDescripcionAsync(string? workKey)
    {
        if (!OpenLibrary.EsClaveValida(workKey)) return null;

        try
        {
            var client = CrearClienteOpenLibrary();
            var obra = await client.GetFromJsonAsync<JsonObject>($"{OpenLibrary.UrlBase}{workKey}.json");
            var texto = obra == null ? null : OpenLibrary.ExtraerDescripcion(obra);
            return texto == null ? null : Recortar(texto, 5000);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo obtener la descripción de {Obra}", workKey);
            return null;
        }
    }
}