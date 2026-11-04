using Microsoft.AspNetCore.Mvc;

namespace BibliotecaPublica.Helpers;

public static class Paginacion
{
    // Carga en ViewData lo que necesita el parcial _Paginacion (y el buscador de cada vista)
    public static void CargarPaginado(this Controller controller, string? buscar, int pagina, int totalRegistros, int tamPagina)
    {
        controller.ViewData["FiltroActual"] = buscar;
        controller.ViewData["PaginaActual"] = Math.Max(1, pagina);
        controller.ViewData["TotalPaginas"] = TotalPaginas(totalRegistros, tamPagina);
    }
    public static int TotalPaginas(int totalRegistros, int tamPagina) =>
        Math.Max(1, (int)Math.Ceiling((double)totalRegistros / tamPagina));
}
