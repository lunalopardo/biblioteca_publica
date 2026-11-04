using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BibliotecaPublica.Models;
using BibliotecaPublica.Repositories;
using BibliotecaPublica.Services;

namespace BibliotecaPublica.Controllers.Api;

// Entrada a la Web API: con email (o nombre de usuario) y contraseña devuelve el JWT
[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthApiController : ControllerBase
{
    private readonly RepositorioUsuario _repositorioUsuario;
    private readonly IPasswordHasher<Usuario> _passwordHasher;
    private readonly ServicioToken _servicioToken;

    public AuthApiController(
        RepositorioUsuario repositorioUsuario,
        IPasswordHasher<Usuario> passwordHasher,
        ServicioToken servicioToken)
    {
        _repositorioUsuario = repositorioUsuario;
        _passwordHasher = passwordHasher;
        _servicioToken = servicioToken;
    }

    // POST: /api/auth/login  { "email": "...", "password": "..." }
    [HttpPost("login")]
    public IActionResult Login(LoginViewModel model)
    {
        var usuario = _repositorioUsuario.ObtenerActivoPorLogin(model.Email);

        if (usuario == null || _passwordHasher.VerifyHashedPassword(usuario, usuario.Password, model.Password)
                               == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new { message = "Credenciales incorrectas o usuario inactivo." });
        }

        var (token, expira) = _servicioToken.Generar(usuario);

        return Ok(new
        {
            token,
            expira,
            usuario = new { usuario.Id, nombre = $"{usuario.Nombre} {usuario.Apellido}", rol = usuario.RolNombre }
        });
    }
}
