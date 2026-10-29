using MySqlConnector;
using BibliotecaPublica.Models;

namespace BibliotecaPublica.Repositories;

public class RepositorioNacionalidad : RepositorioBase
{
    public RepositorioNacionalidad(IConfiguration configuration) : base(configuration) { }

    public IList<Nacionalidad> ObtenerTodas()
    {
        var listado = new List<Nacionalidad>();
        string sql = "SELECT id, nombre, activo FROM nacionalidad WHERE activo = 1 ORDER BY nombre ASC";

        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        listado.Add(new Nacionalidad
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

    // obtenemos los activos para los select y demás
    public List<Nacionalidad> ObtenerActivas()
    {
        var listado = new List<Nacionalidad>();
        var query = "SELECT * FROM Nacionalidad WHERE activo = 1 ORDER BY nombre";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);

        connection.Open();
        using MySqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            listado.Add(ParseNacionalidad(reader));
        }

        return listado;
    }

    public IList<Nacionalidad> ObtenerPaginado(string? buscar = null, int paginaNro = 1, int tamPagina = 10)
    {
        IList<Nacionalidad> listado = new List<Nacionalidad>();
        int offset = Math.Max(0, (paginaNro - 1) * tamPagina);
        var paramBuscar = string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : $"%{buscar.Trim()}%";

        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            string query = @"SELECT * FROM Nacionalidad
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
                        listado.Add(ParseNacionalidad(reader));
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
            FROM Nacionalidad
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
    public Nacionalidad? ObtenerPorId(int id)
    {
        var query = "SELECT * FROM Nacionalidad WHERE id = @id AND activo = 1";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", id);

        connection.Open();
        using MySqlDataReader reader = command.ExecuteReader();
        if (reader.Read())
        {
            return ParseNacionalidad(reader);
        }
        return null;
    }

    // crear nacionalidad
    public int Create(Nacionalidad Nacionalidad)
    {
        var query = "INSERT INTO Nacionalidad (nombre, activo) VALUES (@nombre, 1)";
        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);

        command.Parameters.AddWithValue("@nombre", Nacionalidad.Nombre);
        connection.Open();
        return command.ExecuteNonQuery();
    }

    // actualizar
    public int Update(Nacionalidad Nacionalidad)
    {
        var query = "UPDATE Nacionalidad SET nombre = @nombre WHERE id = @id AND activo = 1";
        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", Nacionalidad.Id);
        command.Parameters.AddWithValue("@nombre", Nacionalidad.Nombre);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    // borrado lógico
    public int DeleteLogico(int id)
    {
        var query = "UPDATE Nacionalidad SET activo = 0 WHERE id = @id";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", id);

        connection.Open();
        return command.ExecuteNonQuery();
    }


    private static Nacionalidad ParseNacionalidad(MySqlDataReader reader)
    {
        return new Nacionalidad
        {
            Id = reader.GetInt32("id"),
            Nombre = reader.GetString("nombre"),
            Activo = reader.GetBoolean("activo")
        };
    }
}