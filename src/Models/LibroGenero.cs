using System.ComponentModel.DataAnnotations;

namespace BibliotecaPublica.Models;

public class LibroGenero
{
    [Required]
    public int IdLibro { get; set; }

    [Required]
    public int IdGenero { get; set; }

    // Objetos relacionados para consultas manuales
    public Libro? Libro { get; set; }
    public Genero? Genero { get; set; }
}