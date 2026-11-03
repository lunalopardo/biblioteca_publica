using MySqlConnector;
using BibliotecaPublica.Models;

namespace BibliotecaPublica.Repositories;

public class RepositorioEditorial : RepositorioBase
{
    public RepositorioEditorial(IConfiguration configuration) : base(configuration) { }

    // obtenemos las activas para los select y demás
    public List<Editorial> ObtenerTodos()
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
        var listado = new List<Editorial>();
        int offset = Math.Max(0, (paginaNro - 1) * tamPagina);
        var paramBuscar = string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : $"%{buscar.Trim()}%";

        string query = @"SELECT * FROM Editorial
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
            listado.Add(ParseEditorial(reader));
        }
        return listado;
    }

    public int ObtenerCantidad(string? buscar = null)
    {
        var paramBuscar = string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : $"%{buscar.Trim()}%";

        string query = @"SELECT COUNT(id)
            FROM Editorial
            WHERE activo = 1
            AND (@buscar IS NULL OR nombre LIKE @buscar)";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@buscar", paramBuscar);

        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar());
    }

    // Busca una editorial activa por nombre exacto
    public Editorial? ObtenerPorNombre(string nombre)
    {
        const string query = "SELECT id, nombre, activo FROM Editorial WHERE activo = 1 AND nombre = @nombre LIMIT 1";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@nombre", nombre);

        connection.Open();
        using MySqlDataReader reader = command.ExecuteReader();
        return reader.Read() ? ParseEditorial(reader) : null;
    }

    // Listado reducido para el Select2: filtra y limita en SQL
    public IList<Editorial> ObtenerParaSelect(string? buscar = null, int limite = 20)
    {
        var listado = new List<Editorial>();
        var paramBuscar = string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : $"%{buscar.Trim()}%";

        const string query = @"SELECT id, nombre, activo FROM Editorial
                               WHERE activo = 1
                                 AND (@buscar IS NULL OR nombre LIKE @buscar)
                               ORDER BY nombre ASC
                               LIMIT @limite";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@buscar", paramBuscar);
        command.Parameters.AddWithValue("@limite", limite);

        connection.Open();
        using MySqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            listado.Add(ParseEditorial(reader));
        }
        return listado;
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

    public int Crear(Editorial editorial)
    {
        var query = "INSERT INTO Editorial (nombre, activo) VALUES (@nombre, 1); SELECT LAST_INSERT_ID();";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@nombre", editorial.Nombre);

        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar());
    }

    // actualizar
    public int Actualizar(Editorial editorial)
    {
        var query = "UPDATE Editorial SET nombre = @nombre WHERE id = @id AND activo = 1";
        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", editorial.Id);
        command.Parameters.AddWithValue("@nombre", editorial.Nombre);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    // borrado lógico
    public int BajaLogica(int id)
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
