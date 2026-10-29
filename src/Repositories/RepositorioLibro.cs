using MySqlConnector;
using BibliotecaPublica.Models;

namespace BibliotecaPublica.Repositories;

public class RepositorioLibro : RepositorioBase
{
    public RepositorioLibro(IConfiguration configuration) : base(configuration) { }

    public IList<Libro> ObtenerPaginado(string? buscar = null, int paginaNro = 1, int tamPagina = 10)
    {
        IList<Libro> listado = new List<Libro>();
        int offset = Math.Max(0, (paginaNro - 1) * tamPagina);
        var paramBuscar = string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : $"%{buscar.Trim()}%";

        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            string query = @"SELECT l.*, 
                                    a.id as autor_id, a.nombre as autor_nombre, a.apellido as autor_apellido,
                                    ed.id as editorial_id, ed.nombre as editorial_nombre
                             FROM Libro l
                             INNER JOIN Autor a ON l.id_autor = a.id
                             LEFT JOIN Editorial ed ON l.id_editorial = ed.id
                             WHERE l.activo = 1
                             AND (@buscar IS NULL OR l.titulo LIKE @buscar OR a.nombre LIKE @buscar OR a.apellido LIKE @buscar)
                             ORDER BY l.id DESC
                             LIMIT @limit OFFSET @offset";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@buscar", paramBuscar);
                command.Parameters.AddWithValue("@limit", tamPagina);
                command.Parameters.AddWithValue("@offset", offset);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        listado.Add(ParseLibroConAutor(reader));
                    }
                }
            }
        }
        return listado;
    }

    public int ObtenerCantidad(string? buscar = null)
    {
        int total = 0;
        var paramBuscar = string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : $"%{buscar.Trim()}%";

        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            string query = @"SELECT COUNT(l.id)
                             FROM Libro l
                             INNER JOIN Autor a ON l.id_autor = a.id
                             WHERE l.activo = 1
                             AND (@buscar IS NULL OR l.titulo LIKE @buscar OR a.nombre LIKE @buscar OR a.apellido LIKE @buscar)";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@buscar", paramBuscar);
                connection.Open();
                var result = command.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    total = Convert.ToInt32(result);
                }
            }
        }
        return total;
    }

    public Libro? ObtenerPorId(int id)
    {
        var query = @"SELECT l.*, 
                             a.id as autor_id, a.nombre as autor_nombre, a.apellido as autor_apellido,
                             ed.id as editorial_id, ed.nombre as editorial_nombre
                      FROM Libro l
                      INNER JOIN Autor a ON l.id_autor = a.id
                      LEFT JOIN Editorial ed ON l.id_editorial = ed.id
                      WHERE l.id = @id AND l.activo = 1";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", id);

        connection.Open();
        using MySqlDataReader reader = command.ExecuteReader();
        if (reader.Read())
        {
            return ParseLibroConAutor(reader);
        }
        return null;
    }

    public int Create(Libro libro)
    {
        var query = @"INSERT INTO Libro (titulo, id_autor, isbn, paginas, anio_publicacion, portada, id_editorial, activo)
                      VALUES (@titulo, @id_autor, @isbn, @paginas, @anio_publicacion, @portada, @id_editorial, 1);
                      SELECT LAST_INSERT_ID();";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);

        command.Parameters.AddWithValue("@titulo", libro.Titulo);
        command.Parameters.AddWithValue("@id_autor", libro.IdAutor);
        command.Parameters.AddWithValue("@isbn", (object?)libro.Isbn ?? DBNull.Value);
        command.Parameters.AddWithValue("@paginas", (object?)libro.Paginas ?? DBNull.Value);
        command.Parameters.AddWithValue("@anio_publicacion", (object?)libro.AnioPublicacion ?? DBNull.Value);
        command.Parameters.AddWithValue("@portada", (object?)libro.Portada ?? DBNull.Value);
        command.Parameters.AddWithValue("@id_editorial", (object?)libro.IdEditorial ?? DBNull.Value);

        connection.Open();
        var id = Convert.ToInt32(command.ExecuteScalar());
        return id;
    }

    public int Update(Libro libro)
    {
        var query = @"UPDATE Libro SET 
                        titulo = @titulo, 
                        id_autor = @id_autor, 
                        isbn = @isbn, 
                        paginas = @paginas, 
                        anio_publicacion = @anio_publicacion, 
                        portada = @portada, 
                        id_editorial = @id_editorial
                      WHERE id = @id";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);

        command.Parameters.AddWithValue("@id", libro.Id);
        command.Parameters.AddWithValue("@titulo", libro.Titulo);
        command.Parameters.AddWithValue("@id_autor", libro.IdAutor);
        command.Parameters.AddWithValue("@isbn", (object?)libro.Isbn ?? DBNull.Value);
        command.Parameters.AddWithValue("@paginas", (object?)libro.Paginas ?? DBNull.Value);
        command.Parameters.AddWithValue("@anio_publicacion", (object?)libro.AnioPublicacion ?? DBNull.Value);
        command.Parameters.AddWithValue("@portada", (object?)libro.Portada ?? DBNull.Value);
        command.Parameters.AddWithValue("@id_editorial", (object?)libro.IdEditorial ?? DBNull.Value);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    public int AnularLogico(int id)
    {
        var query = "UPDATE Libro SET activo = 0 WHERE id = @id";
        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", id);
        connection.Open();
        return command.ExecuteNonQuery();
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