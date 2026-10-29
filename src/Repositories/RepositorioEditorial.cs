using MySqlConnector;
using BibliotecaPublica.Models;

namespace BibliotecaPublica.Repositories;

public class RepositorioEditorial : RepositorioBase
{
    public RepositorioEditorial(IConfiguration configuration) : base(configuration) { }


    //obtener todas
    public IList<Editorial> ObtenerTodas()
    {
        var listado = new List<Editorial>();
        string sql = "SELECT id, nombre, activo FROM Editorial WHERE activo = 1 ORDER BY nombre ASC";

        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        listado.Add(new Editorial
                        {
                            Id = reader.GetInt32("id"),
                            Nombre = reader.GetString("nombre"),
                            Activo = reader.GetBoolean("activo")
                        });
                    }
                }
            }
        }
        return listado;
    }

    // obtenemos las activas para los select y demás
    public List<Editorial> ObtenerActivos()
    {
        var listado = new List<Editorial>();
        var query = "SELECT * FROM Editorial WHERE activo = 1 ORDER BY nombre";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);

        connection.Open();
        using MySqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            listado.Add(ParseEditorial(reader));
        }

        return listado;
    }

    public IList<Editorial> ObtenerPaginado(string? buscar = null, int paginaNro = 1, int tamPagina = 10)
    {
        IList<Editorial> listado = new List<Editorial>();
        int offset = Math.Max(0, (paginaNro - 1) * tamPagina);
        var paramBuscar = string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : $"%{buscar.Trim()}%";

        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            string query = @"SELECT * FROM Editorial
            WHERE activo = 1
            AND (@buscar IS NULL OR nombre LIKE @buscar)
            ORDER BY id DESC
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
                        listado.Add(ParseEditorial(reader));
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
            string query = @"SELECT COUNT(id)
            FROM Editorial
            WHERE activo = 1
            AND (@buscar IS NULL OR nombre LIKE @buscar)";

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

    // obtener por id
    public Editorial? ObtenerPorId(int id)
    {
        var query = "SELECT * FROM Editorial WHERE id = @id AND activo = 1";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", id);

        connection.Open();
        using MySqlDataReader reader = command.ExecuteReader();
        if (reader.Read())
        {
            return ParseEditorial(reader);
        }
        return null;
    }

    public int Create(Editorial editorial)
    {
        var query = "INSERT INTO Editorial (nombre, activo) VALUES (@nombre, 1); SELECT LAST_INSERT_ID();";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@nombre", editorial.Nombre);

        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar());
    }

    // actualizar
    public int Update(Editorial Editorial)
    {
        var query = "UPDATE Editorial SET nombre = @nombre WHERE id = @id AND activo = 1";
        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", Editorial.Id);
        command.Parameters.AddWithValue("@nombre", Editorial.Nombre);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    // borrado lógico
    public int DeleteLogico(int id)
    {
        var query = "UPDATE Editorial SET activo = 0 WHERE id = @id";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", id);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    private static Editorial ParseEditorial(MySqlDataReader reader)
    {
        return new Editorial
        {
            Id = reader.GetInt32("id"),
            Nombre = reader.GetString("nombre"),
            Activo = reader.GetBoolean("activo")
        };
    }
}