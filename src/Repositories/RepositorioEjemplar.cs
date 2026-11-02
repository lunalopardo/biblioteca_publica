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
        const string query = @"SELECT id, id_libro, codigo_inventario, estado, activo
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

    public int ObtenerCantidadEjemplares(int idLibro)
    {
        int total = 0;
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            string query = "SELECT COUNT(id) FROM Ejemplar WHERE id_libro = @idLibro AND activo = 1";
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@idLibro", idLibro);
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

    public int Create(Ejemplar ejemplar)
    {
        const string query = @"INSERT INTO Ejemplar (id_libro, codigo_inventario, estado, activo)
                           VALUES (@id_libro, @codigo_inventario, @estado, 1);
                           SELECT LAST_INSERT_ID();";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id_libro", ejemplar.IdLibro);
        command.Parameters.AddWithValue("@codigo_inventario", ejemplar.CodigoInventario);
        command.Parameters.AddWithValue("@estado", ejemplar.Estado);

        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public int Update(Ejemplar ejemplar)
    {
        const string query = @"UPDATE Ejemplar SET
                             id_libro = @id_libro,
                             codigo_inventario = @codigo_inventario,
                             estado = @estado
                           WHERE id = @id AND activo = 1";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);

        command.Parameters.AddWithValue("@id", ejemplar.Id);
        command.Parameters.AddWithValue("@id_libro", ejemplar.IdLibro);
        command.Parameters.AddWithValue("@codigo_inventario", ejemplar.CodigoInventario);
        command.Parameters.AddWithValue("@estado", ejemplar.Estado);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    public int AnularLogico(int id)
    {
        var query = "UPDATE Ejemplar SET activo = 0 WHERE id = @id";
        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", id);
        connection.Open();
        return command.ExecuteNonQuery();
    }

    private static Ejemplar ParseEjemplar(MySqlDataReader reader)
    {
        return new Ejemplar
        {
            Id = reader.GetInt32("id"),
            IdLibro = reader.GetInt32("id_libro"),
            CodigoInventario = reader.GetInt32("codigo_inventario"),
            Estado = reader.GetString("estado"),
            Activo = reader.GetBoolean("activo")
        };
    }

    // Ejemplar + Libro + Autor (por si hago listado gral)
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