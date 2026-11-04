using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using BibliotecaPublica.Helpers;
using BibliotecaPublica.Models;

namespace BibliotecaPublica.Services;

// Sección "Jwt" de appsettings.json
public class OpcionesJwt
{
    public string Clave { get; set; } = string.Empty;
    public string Emisor { get; set; } = string.Empty;
    public string Audiencia { get; set; } = string.Empty;
    public int MinutosValidez { get; set; } = 60;

    // HS256 necesita una clave de al menos 32 caracteres
    public SymmetricSecurityKey ObtenerClave() => new(Encoding.UTF8.GetBytes(Clave));
}

// Arma el JWT que devuelve POST /api/auth/login
public class ServicioToken
{
    private readonly OpcionesJwt _opciones;

    public ServicioToken(OpcionesJwt opciones)
    {
        _opciones = opciones;
    }

    public (string Token, DateTime Expira) Generar(Usuario usuario)
    {
        var expira = DateTime.UtcNow.AddMinutes(_opciones.MinutosValidez);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(UsuarioActual.CrearClaims(usuario)),
            Issuer = _opciones.Emisor,
            Audience = _opciones.Audiencia,
            Expires = expira,
            SigningCredentials = new SigningCredentials(_opciones.ObtenerClave(), SecurityAlgorithms.HmacSha256)
        };

        return (new JsonWebTokenHandler().CreateToken(descriptor), expira);
    }
}
