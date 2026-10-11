using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BibliotecaPublica.Models;
using BibliotecaPublica.Repositories;

namespace BibliotecaPublica.Controllers
{
    [Authorize]
    public class AfiliadosController(RepositorioAfiliado repo) : Controller
    {
        // GET: Afiliados (con paginado)
        public IActionResult Index(string buscar, int pagina = 1)
        {
            int registrosPorPagina = 5;

            var listado = repo.ObtenerPaginado(buscar, pagina, registrosPorPagina);
            int totalRegistros = repo.ObtenerCantidad(buscar);
            int totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);

            ViewData["FiltroActual"] = buscar;
            ViewData["PaginaActual"] = pagina;
            ViewData["TotalPaginas"] = totalPaginas;

            return View(listado);
        }

        // GET: Afiliados/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();

            var afiliado = repo.GetById(id.Value);
            if (afiliado == null) return NotFound();

            return View(afiliado);
        }

        // GET: Afiliados/Create
        public IActionResult Create() => View();

        // POST: Afiliados/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Afiliado afiliado)
        {
            if (repo.ExistsEmail(afiliado.Email))
            {
                ModelState.AddModelError("Email", "Este correo electrónico ya se encuentra registrado.");
            }

            if (ModelState.IsValid)
            {
                repo.Create(afiliado);
                TempData["Mensaje"] = "Afiliado registrado con éxito.";
                return RedirectToAction(nameof(Index));
            }

            return View(afiliado);
        }

        // GET: Afiliados/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var afiliado = repo.GetById(id.Value);
            if (afiliado == null) return NotFound();

            return View(afiliado);
        }

        // POST: Afiliados/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Afiliado afiliado)
        {
            if (id != afiliado.Id) return NotFound();

            if (repo.ExistsEmail(afiliado.Email, id))
            {
                ModelState.AddModelError("Email", "Este correo electrónico ya pertenece a otro afiliado.");
            }

            if (ModelState.IsValid)
            {
                repo.Update(afiliado);
                TempData["Mensaje"] = "Afiliado actualizado con éxito.";
                return RedirectToAction(nameof(Index));
            }

            return View(afiliado);
        }

        // GET: Afiliados/Delete/5
        [Authorize(Roles = "Administrador")]
        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();

            var afiliado = repo.GetById(id.Value);
            if (afiliado == null) return NotFound();

            return View(afiliado);
        }

        // POST: Afiliados/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public IActionResult DeleteConfirmed(int id)
        {
            repo.DeleteLogico(id);
            TempData["Mensaje"] = "Afiliado dado de baja correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // Endpoint JSON para búsquedas asíncronas por AJAX
        [HttpGet]
        public JsonResult BuscarAsincrona(string term)
        {
            var afiliados = repo.GetParaSelect(term, 10);
            var resultado = afiliados.Select(a => new {
                id = a.Id,
                text = $"{a.Apellido}, {a.Nombre} - Email: {a.Email}"
            });
            return Json(resultado);
        }
    }
}