namespace BibliotecaPublica.Models;

public class HomeViewModel
{
    public int TotalLibros { get; set; }
    public int TotalAfiliados { get; set; }
    public int TotalAutores { get; set; }
    public int TotalPrestamosVigentes { get; set; } = 0;
    public IList<Libro> UltimosLibros { get; set; } = new List<Libro>();
}