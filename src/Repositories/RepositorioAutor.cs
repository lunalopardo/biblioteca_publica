using MySqlConnector;
using BibliotecaPublica.Models;

namespace BibliotecaPublica.Repositories;

public class RepositorioAutor : RepositorioBase
{
    public RepositorioAutor(IConfiguration configuration) : base(configuration) { }

    // Obtener todos los autores activos (sin paginado)
    public IList<Autor> ObtenerTodos()
    {
        var listado = new List<Autor>();
        string sql = @"
            SELECT a.id, a.nombre, a.apellido, a.id_nacionalidad, n.nombre AS nacionalidad_nombre, a.biografia, a.foto, a.activo 
            FROM autor a
            LEFT JOIN nacionalidad n ON a.id_nacionalidad = n.id
            WHERE a.activo = 1
            ORDER BY a.apellido, a.nombre ASC";

        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        listado.Add(ParseAutor(reader));
                    }
                }
            }
        }
        return listado;
    }

    // Listado paginado
    public IList<Autor> ObtenerPaginado(string? buscar = null, int paginaNro = 1, int tamPagina = 5)
    {
        IList<Autor> res = new List<Autor>();
        int offset = Math.Max(0, (paginaNro - 1) * tamPagina);
        var paramBuscar = string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : $"%{buscar.Trim()}%";

        string sql = @"
            SELECT a.id, a.nombre, a.apellido, a.id_nacionalidad, n.nombre AS nacionalidad_nombre, a.biografia, a.foto, a.activo 
            FROM autor a
            LEFT JOIN nacionalidad n ON a.id_nacionalidad = n.id
            WHERE a.activo = 1 
              AND (@buscar IS NULL OR a.nombre LIKE @buscar 
              OR a.apellido LIKE @buscar 
              OR CONCAT(a.nombre, ' ', a.apellido) LIKE @buscar)
            ORDER BY a.id
            LIMIT @limit OFFSET @offset";

        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@buscar", paramBuscar);
                command.Parameters.AddWithValue("@limit", tamPagina);
                command.Parameters.AddWithValue("@offset", offset);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        res.Add(ParseAutor(reader));
                    }
                }
            }
        }
        return res;
    }

    // Obtener la cantidad total de registros activos filtrados (para la paginación)
    public int ObtenerCantidad(string? buscar = null)
    {
        int total = 0;
        var paramBuscar = string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : $"%{buscar.Trim()}%";

        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            string sql = @"
                SELECT COUNT(id) 
                FROM autor 
                WHERE activo = 1 
                  AND (@buscar IS NULL OR nombre LIKE @buscar 
                  OR apellido LIKE @buscar
                  OR CONCAT(nombre, ' ', apellido) LIKE @buscar)";

            using (MySqlCommand command = new MySqlCommand(sql, connection))
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

    // Obtener Autor por ID
    public Autor? GetById(int id)
    {
        var query = @"
            SELECT a.id, a.nombre, a.apellido, a.id_nacionalidad, n.nombre AS nacionalidad_nombre, a.biografia, a.foto, a.activo 
            FROM autor a
            LEFT JOIN nacionalidad n ON a.id_nacionalidad = n.id
            WHERE a.id = @id AND a.activo = 1";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", id);

        connection.Open();
        using MySqlDataReader reader = command.ExecuteReader();
        if (reader.Read())
        {
            return ParseAutor(reader);
        }

        return null;
    }

    // Crear Autor
    public int Create(Autor autor)
    {
        var query = @"INSERT INTO autor (nombre, apellido, id_nacionalidad, biografia, foto, activo) 
                  VALUES (@nombre, @apellido, @id_nacionalidad, @biografia, @foto, 1);
                  SELECT LAST_INSERT_ID();";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@nombre", autor.Nombre);
        command.Parameters.AddWithValue("@apellido", autor.Apellido);

        command.Parameters.AddWithValue("@id_nacionalidad", autor.IdNacionalidad <= 0 ? DBNull.Value : autor.IdNacionalidad);

        command.Parameters.AddWithValue("@biografia", (object?)autor.Biografia ?? DBNull.Value);
        command.Parameters.AddWithValue("@foto", (object?)autor.Foto ?? DBNull.Value);

        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar());
    }

    // Editar autor
    public int Update(Autor autor)
    {
        var query = @"UPDATE autor 
                      SET nombre = @nombre, apellido = @apellido, id_nacionalidad = @id_nacionalidad, 
                          biografia = @biografia, foto = @foto 
                      WHERE id = @id";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", autor.Id);
        command.Parameters.AddWithValue("@nombre", autor.Nombre);
        command.Parameters.AddWithValue("@apellido", autor.Apellido);
        command.Parameters.AddWithValue("@id_nacionalidad", autor.IdNacionalidad);
        command.Parameters.AddWithValue("@biografia", (object?)autor.Biografia ?? DBNull.Value);
        command.Parameters.AddWithValue("@foto", (object?)autor.Foto ?? DBNull.Value);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    // Borrado lógico (activo = 0)
    public int DeleteLogico(int id)
    {
        var query = "UPDATE autor SET activo = 0 WHERE id = @id";
        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", id);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    // Obtener listado reducido para selects
    public IList<Autor> GetParaSelect(string? buscar = null, int limite = 20)
    {
        var listado = new List<Autor>();

        string sql = @"
            SELECT id, nombre, apellido 
            FROM autor 
            WHERE activo = 1";

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            sql += " AND (nombre LIKE @buscar OR apellido LIKE @buscar)";
        }

        sql += " ORDER BY apellido, nombre ASC LIMIT @limite";

        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                if (!string.IsNullOrWhiteSpace(buscar))
                {
                    command.Parameters.AddWithValue("@buscar", $"%{buscar.Trim()}%");
                }
                command.Parameters.AddWithValue("@limite", limite);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        listado.Add(new Autor
                        {
                            Id = reader.GetInt32("id"),
                            Nombre = reader.GetString("nombre"),
                            Apellido = reader.GetString("apellido")
                        });
                    }
                }
            }
        }

        return listado;
    }

    // Función auxiliar para mapear el registro de la base de datos al modelo
    private static Autor ParseAutor(MySqlDataReader reader)
    {
        return new Autor
        {
            Id = reader.GetInt32("id"),
            Nombre = reader.GetString("nombre"),
            Apellido = reader.GetString("apellido"),
            IdNacionalidad = reader.IsDBNull(reader.GetOrdinal("id_nacionalidad")) ? 0 : reader.GetInt32("id_nacionalidad"),
            NacionalidadNombre = reader.IsDBNull(reader.GetOrdinal("nacionalidad_nombre")) ? string.Empty : reader.GetString("nacionalidad_nombre"),
            Biografia = reader.IsDBNull(reader.GetOrdinal("biografia")) ? string.Empty : reader.GetString("biografia"),
            Foto = reader.IsDBNull(reader.GetOrdinal("foto")) ? null : reader.GetString("foto"),
            Activo = reader.GetBoolean("activo")
        };
    }
}