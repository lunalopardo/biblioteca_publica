using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BibliotecaPublica.Helpers;
using BibliotecaPublica.Repositories;

namespace BibliotecaPublica.Controllers.Api;

[ApiController]
[Route("api/afiliados")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class AfiliadosApiController : ControllerBase
{
    private const int TamPagina = 10;

    private readonly RepositorioAfiliado _repositorioAfiliado;

    public AfiliadosApiController(RepositorioAfiliado repositorioAfiliado)
    {
        _repositorioAfiliado = repositorioAfiliado;
    }

    // GET: /api/afiliados?buscar=perez&pagina=1
    [HttpGet]
    public IActionResult Listar(string? buscar, int pagina = 1)
    {
        pagina = Math.Max(1, pagina);
        int total = _repositorioAfiliado.ObtenerCantidad(buscar);

        return Ok(new
        {
            items = _repositorioAfiliado.ObtenerPaginado(buscar, pagina, TamPagina),
            pagina,
            totalPaginas = Paginacion.TotalPaginas(total, TamPagina)
        });
    }

    // GET: /api/afiliados/3 (incluye si está sancionado)
    [HttpGet("{id:int}")]
    public IActionResult Obtener(int id)
    {
        var afiliado = _repositorioAfiliado.ObtenerPorId(id);
        return afiliado == null
            ? NotFound(new { message = "El afiliado no existe." })
            : Ok(afiliado);
    }
}
