using MySqlConnector;
using BibliotecaPublica.Models;

namespace BibliotecaPublica.Repositories;

public class RepositorioEjemplar : RepositorioBase
{
    public RepositorioEjemplar(IConfiguration configuration) : base(configuration) { }

    public IList<Ejemplar> ObtenerPaginado(string? buscar = null, int paginaNro = 1, int tamPagina = 10)
    {
        IList<Ejemplar> listado = new List<Ejemplar>();
        int offset = Math.Max(0, (paginaNro - 1) * tamPagina);
        var paramBuscar = string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : $"%{buscar.Trim()}%";

        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            string query = @"SELECT e.*, 
                                    l.id as libro_id, l.titulo as libro_titulo, l.portada as libro_portada,
                                    a.id as autor_id, a.nombre as autor_nombre, a.apellido as autor_apellido
                             FROM Ejemplar e
                             INNER JOIN Libro l ON e.id_libro = l.id
                             INNER JOIN Autor a ON l.id_autor = a.id
                             WHERE e.activo = 1
                             AND (@buscar IS NULL OR e.codigo_inventario LIKE @buscar OR l.titulo LIKE @buscar)
                             ORDER BY e.id DESC
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
                        listado.Add(ParseEjemplarConLibro(reader));
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
            string query = @"SELECT COUNT(e.id)
                             FROM Ejemplar e
                             INNER JOIN Libro l ON e.id_libro = l.id
                             WHERE e.activo = 1
                             AND (@buscar IS NULL OR e.codigo_inventario LIKE @buscar OR l.titulo LIKE @buscar)";

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

    public Ejemplar? ObtenerPorId(int id)
    {
        var query = @"SELECT e.*, 
                             l.id as libro_id, l.titulo as libro_titulo, l.portada as libro_portada,
                             a.id as autor_id, a.nombre as autor_nombre, a.apellido as autor_apellido
                      FROM Ejemplar e
                      INNER JOIN Libro l ON e.id_libro = l.id
                      INNER JOIN Autor a ON l.id_autor = a.id
                      WHERE e.id = @id AND e.activo = 1";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", id);

        connection.Open();
        using MySqlDataReader reader = command.ExecuteReader();
        if (reader.Read())
        {
            return ParseEjemplarConLibro(reader);
        }
        return null;
    }

    public List<Ejemplar> ObtenerPorLibro(int idLibro)
    {
        const string query = @"SELECT id, id_libro, codigo_inventario, estado, ubicacion, activo
                               FROM Ejemplar
                               WHERE id_libro = @idLibro AND activo = 1
                               ORDER BY codigo_inventario";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@idLibro", idLibro);

        connection.Open();
        using MySqlDataReader reader = command.ExecuteReader();

        var listado = new List<Ejemplar>();
        while (reader.Read())
        {
            listado.Add(ParseEjemplar(reader));
        }
        return listado;
    }

    // Ejemplares disponibles para prestar (con el título del libro)
    public List<Ejemplar> ObtenerDisponibles(string? buscar = null, int limite = 10)
    {
        var paramBuscar = string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : $"%{buscar.Trim()}%";

        const string query = @"SELECT e.id, e.id_libro, e.codigo_inventario, e.estado, e.ubicacion, e.activo,
                                      l.titulo AS libro_titulo
                               FROM Ejemplar e
                               INNER JOIN Libro l ON e.id_libro = l.id
                               WHERE e.activo = 1 AND l.activo = 1 AND e.estado = 'Disponible'
                                 AND (@buscar IS NULL OR e.codigo_inventario LIKE @buscar OR l.titulo LIKE @buscar)
                               ORDER BY l.titulo, e.codigo_inventario
                               LIMIT @limite";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@buscar", paramBuscar);
        command.Parameters.AddWithValue("@limite", limite);

        connection.Open();
        using MySqlDataReader reader = command.ExecuteReader();

        var listado = new List<Ejemplar>();
        while (reader.Read())
        {
            var ejemplar = ParseEjemplar(reader);
            ejemplar.Libro = new Libro { Id = ejemplar.IdLibro, Titulo = reader.GetString("libro_titulo") };
            listado.Add(ejemplar);
        }
        return listado;
    }

    public int ObtenerCantidadEjemplares(int idLibro)
    {
        const string query = "SELECT COUNT(id) FROM Ejemplar WHERE id_libro = @idLibro AND activo = 1";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@idLibro", idLibro);

        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public int Create(Ejemplar ejemplar)
    {
        const string query = @"INSERT INTO Ejemplar (id_libro, codigo_inventario, estado, ubicacion, activo)
                               VALUES (@id_libro, @codigo_inventario, @estado, @ubicacion, 1);
                               SELECT LAST_INSERT_ID();";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id_libro", ejemplar.IdLibro);
        command.Parameters.AddWithValue("@codigo_inventario", ejemplar.CodigoInventario);
        command.Parameters.AddWithValue("@estado", ejemplar.Estado);
        command.Parameters.AddWithValue("@ubicacion", (object?)ejemplar.Ubicacion ?? DBNull.Value);

        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public int Update(Ejemplar ejemplar)
    {
        const string query = @"UPDATE Ejemplar SET
                                 codigo_inventario = @codigo_inventario,
                                 ubicacion = @ubicacion
                               WHERE id = @id AND activo = 1";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", ejemplar.Id);
        command.Parameters.AddWithValue("@codigo_inventario", ejemplar.CodigoInventario);
        command.Parameters.AddWithValue("@ubicacion", (object?)ejemplar.Ubicacion ?? DBNull.Value);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    public int AnularLogico(int id)
    {
        const string query = "UPDATE Ejemplar SET activo = 0 WHERE id = @id";
        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", id);
        connection.Open();
        return command.ExecuteNonQuery();
    }

    // Se revisa que figure como Prestado y que tenga un préstamo activo sin devolver.
    public bool EstaPrestado(int idEjemplar)
    {
        const string query = @"SELECT COUNT(e.id)
                               FROM Ejemplar e
                               WHERE e.id = @id
                                 AND (e.estado = 'Prestado'
                                      OR EXISTS(SELECT 1 FROM prestamo pr
                                                WHERE pr.id_ejemplar = e.id
                                                  AND pr.activo = 1
                                                  AND pr.fecha_devolucion_real IS NULL))";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", idEjemplar);

        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    // Ejemplar solo con sus propios datos
    private static Ejemplar ParseEjemplar(MySqlDataReader reader)
    {
        return new Ejemplar
        {
            Id = reader.GetInt32("id"),
            IdLibro = reader.GetInt32("id_libro"),
            CodigoInventario = reader.GetInt32("codigo_inventario"),
            Estado = reader.GetString("estado"),
            Ubicacion = reader.IsDBNull(reader.GetOrdinal("ubicacion")) ? null : reader.GetString("ubicacion"),
            Activo = reader.GetBoolean("activo")
        };
    }

    // Ejemplar + Libro + Autor (solo para el listado general)
    private static Ejemplar ParseEjemplarConLibro(MySqlDataReader reader)
    {
        var ejemplar = ParseEjemplar(reader);
        ejemplar.Libro = new Libro
        {
            Id = reader.GetInt32("libro_id"),
            Titulo = reader.GetString("libro_titulo"),
            Portada = reader.IsDBNull(reader.GetOrdinal("libro_portada")) ? null : reader.GetString("libro_portada"),
            Autor = new Autor
            {
                Id = reader.GetInt32("autor_id"),
                Nombre = reader.GetString("autor_nombre"),
                Apellido = reader.GetString("autor_apellido")
            }
        };
        return ejemplar;
    }
}