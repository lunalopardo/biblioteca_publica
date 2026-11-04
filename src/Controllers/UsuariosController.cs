using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BibliotecaPublica.Helpers;
using BibliotecaPublica.Models;
using BibliotecaPublica.Repositories;

namespace BibliotecaPublica.Controllers;

[Authorize]
[AutoValidateAntiforgeryToken]
public class UsuariosController : Controller
{
    private const int TamPagina = 10;
    private const string CarpetaAvatares = "avatars";

    private readonly RepositorioUsuario _repositorioUsuario;
    private readonly IWebHostEnvironment _environment;
    private readonly IPasswordHasher<Usuario> _passwordHasher;
    private readonly ILogger<UsuariosController> _logger;

    public UsuariosController(
        RepositorioUsuario repositorioUsuario,
        IWebHostEnvironment environment,
        IPasswordHasher<Usuario> passwordHasher,
        ILogger<UsuariosController> logger)
    {
        _repositorioUsuario = repositorioUsuario;
        _environment = environment;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    // GET: Login
    // returnUrl lo agrega la cookie cuando alguien sin sesión entra a una página protegida
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity is { IsAuthenticated: true })
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // POST: Login
    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var usuario = _repositorioUsuario.ObtenerActivoPorLogin(model.Email);

        if (usuario != null && ContraseniaValida(usuario, model.Password))
        {
            await IniciarSesionAsync(usuario);

            // Solo se vuelve a URLs del propio sitio, para no redirigir a páginas externas
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, "Credenciales incorrectas o usuario inactivo.");

        return View(model);
    }

    [HttpGet("Usuarios/Logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Restringido()
    {
        return View();
    }

    // GET: /Usuarios/Perfil/5
    // El administrador puede ver cualquier perfil; el empleado solo el suyo
    [HttpGet]
    public IActionResult Perfil(int id)
    {
        int idActual = User.ObtenerId();

        if (!User.EsAdministrador() && id != idActual)
        {
            return RedirectToAction(nameof(Perfil), new { id = idActual });
        }

        var usuario = _repositorioUsuario.ObtenerPorId(id);
        if (usuario == null)
        {
            return NotFound();
        }

        usuario.Password = string.Empty;

        return View(usuario);
    }

    // POST: /Usuarios/Perfil/5
    // El administrador puede editar cualquier perfil y cambiar el rol de los demás, el empleado solo edita sus datos.
    [HttpPost]
    public async Task<IActionResult> Perfil(int id, Usuario modelo)
    {
        int idActual = User.ObtenerId();
        bool esAdmin = User.EsAdministrador();
        bool esMiPerfil = id == idActual;

        if (!esAdmin && !esMiPerfil)
        {
            return Forbid();
        }

        var usuarioExistente = _repositorioUsuario.ObtenerPorId(id);
        if (usuarioExistente == null) return NotFound();

        if (!usuarioExistente.Activo)
        {
            TempData["Error"] = "No se puede editar un usuario inactivo. Reactivalo primero.";
            return RedirectToAction(nameof(Perfil), new { id });
        }

        // El nombre de usuario no se edita y la contraseña puede quedar vacía (se mantiene la actual)
        ModelState.Remove(nameof(Usuario.NombreUsuario));
        ModelState.Remove(nameof(Usuario.Password));

        if (_repositorioUsuario.ExisteEmail(modelo.Email, id))
        {
            ModelState.AddModelError(nameof(Usuario.Email), "El correo electrónico ya se encuentra registrado.");
        }

        if (!ModelState.IsValid)
        {
            TempData["Error"] = ModelState.Values.SelectMany(v => v.Errors).First().ErrorMessage;
            return RedirectToAction(nameof(Perfil), new { id });
        }

        // Preservar avatar si no se cargó uno nuevo
        string? avatarAnterior = usuarioExistente.Avatar;
        if (modelo.AvatarFile != null && modelo.AvatarFile.Length > 0)
        {
            modelo.Avatar = await Archivos.GuardarImagenAsync(modelo.AvatarFile, _environment, CarpetaAvatares);
        }
        else
        {
            modelo.Avatar = avatarAnterior;
        }

        // Manejo de la Contraseña
        modelo.Password = string.IsNullOrWhiteSpace(modelo.Password)
            ? usuarioExistente.Password
            : _passwordHasher.HashPassword(modelo, modelo.Password);

        modelo.Id = id;
        modelo.NombreUsuario = usuarioExistente.NombreUsuario;
        modelo.Activo = usuarioExistente.Activo;

        if (esAdmin)
        {
            bool puedeCambiarRol = !esMiPerfil && Enum.IsDefined(typeof(EnRoles), modelo.Rol);
            modelo.Rol = puedeCambiarRol ? modelo.Rol : usuarioExistente.Rol;
            _repositorioUsuario.ActualizarCompleto(modelo);
        }
        else
        {
            _repositorioUsuario.ActualizarPerfil(modelo);
        }

        // El archivo viejo se borra recién cuando la base ya quedó actualizada
        if (modelo.Avatar != avatarAnterior)
        {
            Archivos.Borrar(avatarAnterior, _environment);
        }

        // Si editó su propio perfil se renueva la cookie para que la navbar muestre los datos nuevos
        if (esMiPerfil)
        {
            var usuarioActualizado = _repositorioUsuario.ObtenerPorId(id);
            if (usuarioActualizado != null)
            {
                await IniciarSesionAsync(usuarioActualizado);
            }
        }

        TempData["Mensaje"] = "Perfil actualizado correctamente.";
        return RedirectToAction(nameof(Perfil), new { id });
    }

    // GET: /Usuarios/Index
    [HttpGet]
    [Authorize(Policy = UsuarioActual.PolicyAdministrador)]
    public IActionResult Index(string? buscar = null, int pagina = 1)
    {
        var usuarios = _repositorioUsuario.ObtenerPaginado(buscar, pagina, TamPagina);
        this.CargarPaginado(buscar, pagina, _repositorioUsuario.ObtenerCantidad(buscar), TamPagina);

        return View(usuarios);
    }

    [HttpPost]
    [Authorize(Policy = UsuarioActual.PolicyAdministrador)]
    public IActionResult Eliminar(int id)
    {
        if (id == User.ObtenerId())
        {
            TempData["Error"] = "No podés dar de baja tu propio usuario.";
            return RedirectToAction(nameof(Index));
        }

        _repositorioUsuario.BajaLogica(id);
        TempData["Mensaje"] = "Usuario eliminado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Policy = UsuarioActual.PolicyAdministrador)]
    public IActionResult Reactivar(int id)
    {
        try
        {
            int resultado = _repositorioUsuario.Reactivar(id);

            if (resultado > 0)
            {
                TempData["Mensaje"] = "El usuario ha sido reactivado correctamente.";
            }
            else
            {
                TempData["Error"] = "No se pudo reactivar el usuario.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al reactivar el usuario {Id}", id);
            TempData["Error"] = "Ocurrió un error al intentar reactivar el usuario.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Policy = UsuarioActual.PolicyAdministrador)]
    public IActionResult Create()
    {
        return View();
    }

    //POST: /Usuarios/Create
    [HttpPost]
    [Authorize(Policy = UsuarioActual.PolicyAdministrador)]
    public async Task<IActionResult> Create(Usuario usuario)
    {
        ModelState.Remove(nameof(Usuario.Avatar));

        if (string.IsNullOrWhiteSpace(usuario.Password))
        {
            ModelState.AddModelError(nameof(Usuario.Password), "La contraseña es obligatoria.");
        }

        if (!Enum.IsDefined(typeof(EnRoles), usuario.Rol))
        {
            ModelState.AddModelError(nameof(Usuario.Rol), "Elegí un rol válido.");
        }

        if (_repositorioUsuario.ExisteNombreUsuario(usuario.NombreUsuario))
        {
            ModelState.AddModelError(nameof(Usuario.NombreUsuario), "El nombre de usuario ya está en uso.");
        }

        if (_repositorioUsuario.ExisteEmail(usuario.Email))
        {
            ModelState.AddModelError(nameof(Usuario.Email), "El correo electrónico ya se encuentra registrado.");
        }

        if (!ModelState.IsValid)
        {
            return View(usuario);
        }

        // Procesar el avatar si subieron uno
        if (usuario.AvatarFile != null && usuario.AvatarFile.Length > 0)
        {
            usuario.Avatar = await Archivos.GuardarImagenAsync(usuario.AvatarFile, _environment, CarpetaAvatares);
        }

        usuario.Password = _passwordHasher.HashPassword(usuario, usuario.Password);
        usuario.Activo = true;

        int resultado = _repositorioUsuario.Crear(usuario);

        if (resultado > 0)
        {
            TempData["Mensaje"] = "¡Usuario creado correctamente!";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, "Ocurrió un error al crear el usuario.");
        return View(usuario);
    }

    private bool ContraseniaValida(Usuario usuario, string password)
    {
        var resultado = _passwordHasher.VerifyHashedPassword(usuario, usuario.Password, password);
        return resultado is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }

    // Crea la cookie de sesión con los datos que usan la navbar y la autorización por rol
    private async Task IniciarSesionAsync(Usuario usuario)
    {
        var claimsIdentity = new ClaimsIdentity(UsuarioActual.CrearClaims(usuario),
                                                CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity));
    }
}
