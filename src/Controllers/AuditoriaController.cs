using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BibliotecaPublica.Helpers;
using BibliotecaPublica.Repositories;

namespace BibliotecaPublica.Controllers;

[Authorize(Policy = UsuarioActual.PolicyAdministrador)]
public class AuditoriaController : Controller
{
    private const int TamPagina = 15;

    private readonly RepositorioAuditoria _repositorioAuditoria;
    private readonly RepositorioUsuario _repositorioUsuario;

    public AuditoriaController(RepositorioAuditoria repositorioAuditoria, RepositorioUsuario repositorioUsuario)
    {
        _repositorioAuditoria = repositorioAuditoria;
        _repositorioUsuario = repositorioUsuario;
    }

    // GET: /Auditoria?idUsuario=2&operacion=Pago&desde=2026-11-01&hasta=2026-11-30&pagina=1
    public IActionResult Index(int? idUsuario, string? operacion, DateTime? desde, DateTime? hasta, int pagina = 1)
    {
        if (desde > hasta)
        {
            (desde, hasta) = (hasta, desde);
        }

        var listado = _repositorioAuditoria.ObtenerPaginado(idUsuario, operacion, desde, hasta, pagina, TamPagina);
        this.CargarPaginado(null, pagina, _repositorioAuditoria.ObtenerCantidad(idUsuario, operacion, desde, hasta), TamPagina);

        ViewBag.Usuarios = _repositorioUsuario.ObtenerParaFiltro();
        ViewBag.IdUsuario = idUsuario;
        ViewBag.Operacion = operacion;
        ViewBag.Desde = desde?.ToString("yyyy-MM-dd");
        ViewBag.Hasta = hasta?.ToString("yyyy-MM-dd");

        return View(listado);
    }
}
