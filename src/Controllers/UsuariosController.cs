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
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity is { IsAuthenticated: true })
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }

    // POST: Login
    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var usuario = _repositorioUsuario.ValidarLogin(model.Email, model.Password, _passwordHasher);

        if (usuario != null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Email),
                new Claim("FullName", $"{usuario.Nombre} {usuario.Apellido}"),
                new Claim(ClaimTypes.Role, usuario.RolNombre ?? "Usuario"),
                new Claim("Avatar", usuario.Avatar ?? "")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, "Credenciales incorrectas o usuario inactivo.");

        return View(model);
    }

    [HttpGet("Usuarios/Logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Restringido()
    {
        return View();
    }

    // GET: /Usuarios/Perfil/5
    [HttpGet]
    public IActionResult Perfil(int id)
    {
        int currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        bool esAdmin = User.IsInRole("Administrador");

        if (!esAdmin && id != currentUserId)
        {
            return RedirectToAction("Perfil", new { id = currentUserId });
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
    [HttpPost]
    public async Task<IActionResult> Perfil(int id, Usuario modelo)
    {
        int currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        bool esAdmin = User.IsInRole("Administrador");

        if (!esAdmin && id != currentUserId)
        {
            return Forbid();
        }

        var usuarioExistente = _repositorioUsuario.ObtenerPorId(id);
        if (usuarioExistente == null) return NotFound();

        // La imagen se valida con [ImagenValida]; si no es válida no se guarda nada
        string? errorAvatar = ModelState[nameof(Usuario.AvatarFile)]?.Errors.FirstOrDefault()?.ErrorMessage;
        if (errorAvatar != null)
        {
            TempData["Error"] = errorAvatar;
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
        if (string.IsNullOrWhiteSpace(modelo.Password))
        {
            modelo.Password = usuarioExistente.Password;
        }
        else
        {
            modelo.Password = _passwordHasher.HashPassword(modelo, modelo.Password);
        }

        modelo.Id = id;
        modelo.NombreUsuario = usuarioExistente.NombreUsuario;
        modelo.Rol = usuarioExistente.Rol;

        if (esAdmin)
        {
            _repositorioUsuario.ActualizarCompleto(modelo);
        }
        else
        {
            modelo.Activo = usuarioExistente.Activo;
            _repositorioUsuario.ActualizarPerfil(modelo);
        }

        // El archivo viejo se borra recién cuando la base ya quedó actualizada
        if (modelo.Avatar != avatarAnterior)
        {
            Archivos.Borrar(avatarAnterior, _environment);
        }

        var usuarioActualizado = _repositorioUsuario.ObtenerPorId(id);

        if (id == currentUserId && usuarioActualizado != null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuarioActualizado.Id.ToString()),
                new Claim(ClaimTypes.Name, usuarioActualizado.Email),
                new Claim("FullName", $"{usuarioActualizado.Nombre} {usuarioActualizado.Apellido}"),
                new Claim(ClaimTypes.Role, usuarioActualizado.RolNombre),
                new Claim("Avatar", usuarioActualizado.Avatar ?? "")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
        }

        TempData["Mensaje"] = "Perfil actualizado correctamente.";
        return RedirectToAction(nameof(Perfil), new { id });
    }

    // GET: /Usuarios/Index
    [HttpGet]
    [Authorize(Roles = "Administrador")]
    public IActionResult Index(string? buscar = null, int pagina = 1)
    {
        var usuarios = _repositorioUsuario.ObtenerPaginado(buscar, pagina, TamPagina);
        this.CargarPaginado(buscar, pagina, _repositorioUsuario.ObtenerCantidad(buscar), TamPagina);

        return View(usuarios);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public IActionResult Eliminar(int id)
    {
        _repositorioUsuario.BajaLogica(id);
        TempData["Mensaje"] = "Usuario eliminado correctamente.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
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

        return RedirectToAction("Index");
    }

    [HttpGet]
    [Authorize(Roles = "Administrador")]
    public IActionResult Create()
    {
        return View();
    }

    //POST: /Usuarios/Create
    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Create(Usuario usuario)
    {
        ModelState.Remove(nameof(Usuario.Avatar));

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

        if (usuario.Rol == 0)
        {
            usuario.Rol = (int)EnRoles.Empleado;
        }
        usuario.Activo = true;

        int resultado = _repositorioUsuario.Crear(usuario, _passwordHasher);

        if (resultado > 0)
        {
            TempData["Mensaje"] = "¡Usuario creado correctamente!";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, "Ocurrió un error al crear el usuario.");
        return View(usuario);
    }
}