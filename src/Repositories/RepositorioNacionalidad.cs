using MySqlConnector;
using BibliotecaPublica.Models;

namespace BibliotecaPublica.Repositories;

public class RepositorioNacionalidad : RepositorioBase
{
    public RepositorioNacionalidad(IConfiguration configuration) : base(configuration) { }

    // obtenemos las activas para los select y demás
    public List<Nacionalidad> ObtenerTodos()
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
        var listado = new List<Nacionalidad>();
        int offset = Math.Max(0, (paginaNro - 1) * tamPagina);
        var paramBuscar = string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : $"%{buscar.Trim()}%";

        string query = @"SELECT * FROM Nacionalidad
            WHERE activo = 1
            AND (@buscar IS NULL OR nombre LIKE @buscar)
            ORDER BY id DESC
            LIMIT @limit OFFSET @offset";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@buscar", paramBuscar);
        command.Parameters.AddWithValue("@limit", tamPagina);
        command.Parameters.AddWithValue("@offset", offset);

        connection.Open();
        using MySqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            listado.Add(ParseNacionalidad(reader));
        }
        return listado;
    }

    public int ObtenerCantidad(string? buscar = null)
    {
        var paramBuscar = string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : $"%{buscar.Trim()}%";

        string query = @"SELECT COUNT(id)
            FROM Nacionalidad
            WHERE activo = 1
            AND (@buscar IS NULL OR nombre LIKE @buscar)";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@buscar", paramBuscar);

        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar());
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
    public int Crear(Nacionalidad nacionalidad)
    {
        var query = "INSERT INTO Nacionalidad (nombre, activo) VALUES (@nombre, 1)";
        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);

        command.Parameters.AddWithValue("@nombre", nacionalidad.Nombre);
        connection.Open();
        return command.ExecuteNonQuery();
    }

    // actualizar
    public int Actualizar(Nacionalidad nacionalidad)
    {
        var query = "UPDATE Nacionalidad SET nombre = @nombre WHERE id = @id AND activo = 1";
        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", nacionalidad.Id);
        command.Parameters.AddWithValue("@nombre", nacionalidad.Nombre);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    // borrado lógico
    public int BajaLogica(int id)
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
