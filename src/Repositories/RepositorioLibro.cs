using MySqlConnector;
using BibliotecaPublica.Models;

namespace BibliotecaPublica.Repositories;

public class RepositorioLibro : RepositorioBase
{
    private readonly RepositorioEjemplar _repositorioEjemplar;

    public RepositorioLibro(IConfiguration configuration, RepositorioEjemplar repositorioEjemplar)
        : base(configuration)
    {
        _repositorioEjemplar = repositorioEjemplar;
    }

    // Partes comunes: las usan ObtenerPaginado y ObtenerPorId
    private const string ColumnasConAutor = @"
        SELECT l.*,
               a.id AS autor_id, a.nombre AS autor_nombre, a.apellido AS autor_apellido,
               ed.id AS editorial_id, ed.nombre AS editorial_nombre ";

    private const string FromConAutor = @"
        FROM Libro l
        INNER JOIN Autor a ON l.id_autor = a.id
        LEFT JOIN Editorial ed ON l.id_editorial = ed.id ";

    // Préstamos de un libro en los últimos @dias días (los anulados no cuentan)
    private const string PrestamosEnPeriodo = @"
        FROM prestamo pr
        INNER JOIN ejemplar e ON pr.id_ejemplar = e.id
        WHERE e.id_libro = l.id
          AND pr.activo = 1
          AND pr.fecha_prestamo >= DATE_SUB(CURDATE(), INTERVAL @dias DAY) ";

    private const string Filtro = @"
        WHERE l.activo = 1
          AND (@buscar IS NULL OR l.titulo LIKE @buscar OR a.nombre LIKE @buscar OR a.apellido LIKE @buscar
               OR CONCAT(a.nombre, ' ', a.apellido) LIKE @buscar)
          AND (@idAutor IS NULL OR l.id_autor = @idAutor)
          AND (@idioma IS NULL OR l.idioma = @idioma)
          AND (@dias IS NULL OR EXISTS(SELECT 1 " + PrestamosEnPeriodo + ")) ";

    public IList<Libro> ObtenerPaginado(string? buscar = null, int paginaNro = 1, int tamPagina = 10,
        int? idAutor = null, string? idioma = null, int? dias = null)
    {
        var listado = new List<Libro>();
        int offset = Math.Max(0, (paginaNro - 1) * tamPagina);

        const string query = ColumnasConAutor + ", (SELECT COUNT(*) " + PrestamosEnPeriodo + ") AS cantidad_prestamos "
            + FromConAutor + Filtro + @"
            ORDER BY CASE WHEN @dias IS NULL THEN 0 ELSE cantidad_prestamos END DESC, l.id DESC
            LIMIT @limit OFFSET @offset";

        // Bloque using: la conexión se cierra antes de cargar géneros y ejemplares de cada libro
        using (MySqlConnection connection = new(connectionString))
        {
            using MySqlCommand command = new(query, connection);
            CargarFiltros(command, buscar, idAutor, idioma, dias);
            command.Parameters.AddWithValue("@limit", tamPagina);
            command.Parameters.AddWithValue("@offset", offset);

            connection.Open();
            using MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                var libro = ParseLibroConAutor(reader);
                libro.CantidadPrestamos = Convert.ToInt32(reader["cantidad_prestamos"]);
                listado.Add(libro);
            }
        }

        foreach (var libro in listado)
        {
            libro.Generos = ObtenerGenerosDeLibro(libro.Id);
            libro.Ejemplares = _repositorioEjemplar.ObtenerPorLibro(libro.Id);
        }
        return listado;
    }

    public int ObtenerCantidad(string? buscar = null, int? idAutor = null, string? idioma = null, int? dias = null)
    {
        const string query = "SELECT COUNT(l.id) " + FromConAutor + Filtro;

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        CargarFiltros(command, buscar, idAutor, idioma, dias);

        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public Libro? ObtenerPorId(int id)
    {
        Libro? libro;

        const string query = ColumnasConAutor + FromConAutor + " WHERE l.id = @id AND l.activo = 1";

        using (MySqlConnection connection = new(connectionString))
        {
            using MySqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@id", id);

            connection.Open();
            using MySqlDataReader reader = command.ExecuteReader();
            libro = reader.Read() ? ParseLibroConAutor(reader) : null;
        }

        if (libro == null) return null;

        libro.Generos = ObtenerGenerosDeLibro(libro.Id);
        libro.Ejemplares = _repositorioEjemplar.ObtenerPorLibro(libro.Id);
        return libro;
    }

    // Idiomas cargados en el catálogo (para el filtro del listado)
    public List<string> ObtenerIdiomas()
    {
        const string query = @"SELECT DISTINCT idioma FROM Libro
                               WHERE activo = 1 AND idioma IS NOT NULL AND idioma <> ''
                               ORDER BY idioma";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);

        connection.Open();
        using MySqlDataReader reader = command.ExecuteReader();

        var idiomas = new List<string>();
        while (reader.Read())
        {
            idiomas.Add(reader.GetString("idioma"));
        }
        return idiomas;
    }

    public int Crear(Libro libro)
    {
        const string query = @"INSERT INTO Libro
                                 (titulo, id_autor, isbn, paginas, anio_publicacion, idioma, descripcion,
                                  portada, id_editorial, activo)
                               VALUES
                                 (@titulo, @id_autor, @isbn, @paginas, @anio_publicacion, @idioma, @descripcion,
                                  @portada, @id_editorial, 1);
                               SELECT LAST_INSERT_ID();";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        AgregarParametros(command, libro);

        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public int Actualizar(Libro libro)
    {
        const string query = @"UPDATE Libro SET
                                 titulo = @titulo,
                                 id_autor = @id_autor,
                                 isbn = @isbn,
                                 paginas = @paginas,
                                 anio_publicacion = @anio_publicacion,
                                 idioma = @idioma,
                                 descripcion = @descripcion,
                                 portada = @portada,
                                 id_editorial = @id_editorial
                               WHERE id = @id";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        AgregarParametros(command, libro);
        command.Parameters.AddWithValue("@id", libro.Id);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    // Los parámetros que comparten Crear y Actualizar
    private static void AgregarParametros(MySqlCommand command, Libro libro)
    {
        command.Parameters.AddWithValue("@titulo", libro.Titulo);
        command.Parameters.AddWithValue("@id_autor", libro.IdAutor);
        command.Parameters.AddWithValue("@isbn", (object?)libro.Isbn ?? DBNull.Value);
        command.Parameters.AddWithValue("@paginas", (object?)libro.Paginas ?? DBNull.Value);
        command.Parameters.AddWithValue("@anio_publicacion", (object?)libro.AnioPublicacion ?? DBNull.Value);
        command.Parameters.AddWithValue("@idioma", (object?)libro.Idioma ?? DBNull.Value);
        command.Parameters.AddWithValue("@descripcion", (object?)libro.Descripcion ?? DBNull.Value);
        command.Parameters.AddWithValue("@portada", (object?)libro.Portada ?? DBNull.Value);
        command.Parameters.AddWithValue("@id_editorial", (object?)libro.IdEditorial ?? DBNull.Value);
    }

    public int BajaLogica(int id)
    {
        const string query = "UPDATE Libro SET activo = 0 WHERE id = @id";
        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", id);
        connection.Open();
        return command.ExecuteNonQuery();
    }

    // Reemplaza los géneros del libro.
    public void GuardarGeneros(int idLibro, List<int> idsGeneros)
    {
        using MySqlConnection connection = new(connectionString);
        connection.Open();
        using MySqlTransaction tx = connection.BeginTransaction();

        using var borrar = new MySqlCommand("DELETE FROM libro_genero WHERE id_libro = @idLibro", connection, tx);
        borrar.Parameters.AddWithValue("@idLibro", idLibro);
        borrar.ExecuteNonQuery();

        if (idsGeneros != null && idsGeneros.Count > 0)
        {
            using var insertar = new MySqlCommand(
                "INSERT INTO libro_genero (id_libro, id_genero) VALUES (@idLibro, @idGenero)", connection, tx);
            insertar.Parameters.AddWithValue("@idLibro", idLibro);
            var parametroGenero = insertar.Parameters.Add("@idGenero", MySqlDbType.Int32);

            foreach (var idGenero in idsGeneros.Distinct())
            {
                parametroGenero.Value = idGenero;
                insertar.ExecuteNonQuery();
            }
        }

        tx.Commit();
    }

    public List<Genero> ObtenerGenerosDeLibro(int idLibro)
    {
        const string query = @"SELECT g.id, g.nombre, g.activo
                               FROM Genero g
                               INNER JOIN libro_genero lg ON g.id = lg.id_genero
                               WHERE lg.id_libro = @idLibro";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@idLibro", idLibro);
        connection.Open();

        using MySqlDataReader reader = command.ExecuteReader();

        var listado = new List<Genero>();
        while (reader.Read())
        {
            listado.Add(new Genero
            {
                Id = reader.GetInt32("id"),
                Nombre = reader.GetString("nombre"),
                Activo = reader.GetBoolean("activo")
            });
        }
        return listado;
    }

    // ver si un ejemplar de este libro está prestado ahora mismo (préstamo activo sin devolver)
    public bool TienePrestamosEnCurso(int idLibro)
    {
        const string query = @"SELECT COUNT(pr.id)
                               FROM prestamo pr
                               INNER JOIN ejemplar e ON pr.id_ejemplar = e.id
                               WHERE e.id_libro = @idLibro
                                 AND pr.activo = 1
                                 AND pr.fecha_devolucion_real IS NULL";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@idLibro", idLibro);

        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    private static void CargarFiltros(MySqlCommand command, string? buscar, int? idAutor, string? idioma, int? dias)
    {
        command.Parameters.AddWithValue("@buscar", string.IsNullOrWhiteSpace(buscar) ? DBNull.Value : $"%{buscar.Trim()}%");
        command.Parameters.AddWithValue("@idAutor", (object?)idAutor ?? DBNull.Value);
        command.Parameters.AddWithValue("@idioma", string.IsNullOrWhiteSpace(idioma) ? DBNull.Value : idioma);
        command.Parameters.AddWithValue("@dias", (object?)dias ?? DBNull.Value);
    }

    private static Libro ParseLibroConAutor(MySqlDataReader reader)
    {
        return new Libro
        {
            Id = reader.GetInt32("id"),
            Titulo = reader.GetString("titulo"),
            IdAutor = reader.GetInt32("id_autor"),
            IdEditorial = reader.IsDBNull(reader.GetOrdinal("id_editorial")) ? null : reader.GetInt32("id_editorial"),
            Isbn = reader.IsDBNull(reader.GetOrdinal("isbn")) ? null : reader.GetString("isbn"),
            Paginas = reader.IsDBNull(reader.GetOrdinal("paginas")) ? null : reader.GetInt32("paginas"),
            AnioPublicacion = reader.IsDBNull(reader.GetOrdinal("anio_publicacion")) ? null : reader.GetInt32("anio_publicacion"),
            Idioma = reader.IsDBNull(reader.GetOrdinal("idioma")) ? null : reader.GetString("idioma"),
            Descripcion = reader.IsDBNull(reader.GetOrdinal("descripcion")) ? null : reader.GetString("descripcion"),
            Portada = reader.IsDBNull(reader.GetOrdinal("portada")) ? null : reader.GetString("portada"),
            Editorial = reader.IsDBNull(reader.GetOrdinal("editorial_id")) ? null : new Editorial
            {
                Id = reader.GetInt32("editorial_id"),
                Nombre = reader.GetString("editorial_nombre")
            },
            Activo = reader.GetBoolean("activo"),
            Autor = new Autor
            {
                Id = reader.GetInt32("autor_id"),
                Nombre = reader.GetString("autor_nombre"),
                Apellido = reader.GetString("autor_apellido")
            }
        };
    }
}