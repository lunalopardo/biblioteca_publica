using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BibliotecaPublica.Models;
using BibliotecaPublica.Repositories;

namespace BibliotecaPublica.Controllers;

[Authorize]
public class UsuariosController : Controller
{
    private readonly RepositorioUsuario _repositorioUsuario;
    private readonly IWebHostEnvironment _environment;
    private readonly IPasswordHasher<Usuario> _passwordHasher;

    public UsuariosController(
        RepositorioUsuario repositorioUsuario,
        IWebHostEnvironment environment,
        IPasswordHasher<Usuario> passwordHasher)
    {
        _repositorioUsuario = repositorioUsuario;
        _environment = environment;
        _passwordHasher = passwordHasher;
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
    [ValidateAntiForgeryToken]
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

    // GET: /Perfil/5
    [HttpGet("Perfil/{id:int}")]
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

    // POST: /Perfil/5
    [HttpPost("Perfil/{id:int}")]
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

        // Preservar avatar si no se cargó uno nuevo
        if (modelo.AvatarFile != null && modelo.AvatarFile.Length > 0)
        {
            string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "avatars");
            Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(modelo.AvatarFile.FileName)}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await modelo.AvatarFile.CopyToAsync(fileStream);
            }

            modelo.Avatar = $"/uploads/avatars/{uniqueFileName}";
        }
        else
        {
            modelo.Avatar = usuarioExistente.Avatar;
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
            _repositorioUsuario.UpdateCompleto(modelo);
        }
        else
        {
            modelo.Activo = usuarioExistente.Activo;
            _repositorioUsuario.UpdatePerfil(modelo);
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

        ViewBag.Mensaje = "Perfil actualizado correctamente.";
        if (usuarioActualizado != null) usuarioActualizado.Password = string.Empty;

        return View(usuarioActualizado ?? modelo);
    }

    // GET: /Usuarios/Index
    [HttpGet]
    [Authorize(Roles = "Administrador")]
    public IActionResult Index(string? buscar = null, int pagina = 1)
    {
        int cantidadPorPagina = 10;

        int totalRegistros = _repositorioUsuario.ObtenerCantidad(buscar);

        var usuarios = _repositorioUsuario.ObtenerPaginado(buscar, pagina, cantidadPorPagina);

        ViewData["FiltroActual"] = buscar;
        ViewData["PaginaActual"] = pagina;
        ViewData["TotalPaginas"] = (int)Math.Ceiling((double)totalRegistros / cantidadPorPagina);

        return View(usuarios);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public IActionResult Eliminar(int id)
    {
        _repositorioUsuario.AnularLogico(id);
        TempData["Mensaje"] = "Usuario eliminado correctamente.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Edit(
        Usuario usuario,
        IFormFile? avatarFile,
        [FromServices] IWebHostEnvironment environment)
    {
        if (ModelState.IsValid)
        {
            var usuarioExistente = _repositorioUsuario.ObtenerPorId(usuario.Id);
            if (usuarioExistente == null) return NotFound();

            if (avatarFile != null && avatarFile.Length > 0)
            {
                ImagenesController.BorrarArchivoFisico(usuarioExistente.Avatar, environment);
                usuario.Avatar = await ImagenesController.GuardarArchivoAsync(avatarFile, environment, "Avatares");
            }
            else
            {
                usuario.Avatar = usuarioExistente.Avatar;
            }
            usuario.Rol = usuarioExistente.Rol;

            // Si cambió la contraseña, la hasheamos
            if (!string.IsNullOrWhiteSpace(usuario.Password))
            {
                usuario.Password = _passwordHasher.HashPassword(usuario, usuario.Password);
            }
            else
            {
                usuario.Password = usuarioExistente.Password;
            }

            _repositorioUsuario.UpdateCompleto(usuario);
            return RedirectToAction(nameof(Index));
        }
        return View(usuario);
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
                TempData["Mensaje"] = "No se pudo reactivar el usuario.";
            }
        }
        catch (Exception ex)
        {
            TempData["Mensaje"] = "Ocurrió un error al intentar reactivar el usuario.";
            Console.Write(ex.Message);
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
    [ValidateAntiForgeryToken]
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
            string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "avatars");
            Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(usuario.AvatarFile.FileName)}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await usuario.AvatarFile.CopyToAsync(fileStream);
            }

            usuario.Avatar = $"/uploads/avatars/{uniqueFileName}";
        }

        if (usuario.Rol == 0)
        {
            usuario.Rol = (int)EnRoles.Empleado;
        }
        usuario.Activo = true;

        int resultado = _repositorioUsuario.Create(usuario, _passwordHasher);

        if (resultado > 0)
        {
            TempData["Mensaje"] = "¡Usuario creado correctamente!";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, "Ocurrió un error al crear el usuario.");
        return View(usuario);
    }
}