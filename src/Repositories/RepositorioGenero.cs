using MySqlConnector;
using BibliotecaPublica.Models;

namespace BibliotecaPublica.Repositories;

public class RepositorioGenero : RepositorioBase
{
    public RepositorioGenero(IConfiguration configuration) : base(configuration) { }

    // obtenemos todos
    public List<Genero> ObtenerTodos()
    {
        var listado = new List<Genero>();
        var query = "SELECT id, nombre FROM Genero";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        connection.Open();

        using MySqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            listado.Add(new Genero
            {
                Id = reader.GetInt32("id"),
                Nombre = reader.GetString("nombre")
            });
        }
        return listado;
    }

    // obtenemos los activos para los select y demás
    public List<Genero> ObtenerActivos()
    {
        var listado = new List<Genero>();
        var query = "SELECT * FROM Genero WHERE activo = 1 ORDER BY nombre";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);

        connection.Open();
        using MySqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            listado.Add(ParseGenero(reader));
        }

        return listado;
    }

    public IList<Genero> ObtenerPaginado(string? buscar = null, int paginaNro = 1, int tamPagina = 10)
    {
        IList<Genero> listado = new List<Genero>();
        int offset = Math.Max(0, (paginaNro - 1) * tamPagina);
        var paramBuscar = string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : $"%{buscar.Trim()}%";

        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            string query = @"SELECT * FROM Genero
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
                        listado.Add(ParseGenero(reader));
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
            FROM Genero
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
    public Genero? ObtenerPorId(int id)
    {
        var query = "SELECT * FROM Genero WHERE id = @id AND activo = 1";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", id);

        connection.Open();
        using MySqlDataReader reader = command.ExecuteReader();
        if (reader.Read())
        {
            return ParseGenero(reader);
        }
        return null;
    }

    // crear género
    public int Create(Genero genero)
    {
        var query = "INSERT INTO Genero (nombre, activo) VALUES (@nombre, 1)";
        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);

        command.Parameters.AddWithValue("@nombre", genero.Nombre);
        connection.Open();
        return command.ExecuteNonQuery();
    }

    // actualizar
    public int Update(Genero genero)
    {
        var query = "UPDATE Genero SET nombre = @nombre WHERE id = @id AND activo = 1";
        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", genero.Id);
        command.Parameters.AddWithValue("@nombre", genero.Nombre);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    // borrado lógico
    public int DeleteLogico(int id)
    {
        var query = "UPDATE Genero SET activo = 0 WHERE id = @id";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", id);

        connection.Open();
        return command.ExecuteNonQuery();
    }


    private static Genero ParseGenero(MySqlDataReader reader)
    {
        return new Genero
        {
            Id = reader.GetInt32("id"),
            Nombre = reader.GetString("nombre"),
            Activo = reader.GetBoolean("activo")
        };
    }
}