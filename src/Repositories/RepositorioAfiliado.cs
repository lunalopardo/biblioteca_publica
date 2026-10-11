using MySqlConnector;
using BibliotecaPublica.Models;

namespace BibliotecaPublica.Repositories;

public class RepositorioAfiliado : RepositorioBase
{
    public RepositorioAfiliado(IConfiguration configuration) : base(configuration) { }

    // Listado paginado con búsqueda general
    public IList<Afiliado> ObtenerPaginado(string? buscar = null, int paginaNro = 1, int tamPagina = 5)
    {
        IList<Afiliado> res = new List<Afiliado>();
        int offset = Math.Max(0, (paginaNro - 1) * tamPagina);
        var paramBuscar = string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : $"%{buscar.Trim()}%";

        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            string sql = @"
                SELECT * FROM Afiliado 
                WHERE activo = 1 
                  AND (@buscar IS NULL OR nombre LIKE @buscar 
                  OR apellido LIKE @buscar 
                  OR CONCAT(nombre, ' ', apellido) LIKE @buscar
                  OR email LIKE @buscar)
                ORDER BY id
                LIMIT @limit OFFSET @offset";

            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@buscar", paramBuscar);
                command.Parameters.AddWithValue("@limit", tamPagina);
                command.Parameters.AddWithValue("@offset", offset);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        res.Add(ParseAfiliado(reader));
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
                FROM Afiliado 
                WHERE activo = 1 
                  AND (@buscar IS NULL OR nombre LIKE @buscar 
                  OR apellido LIKE @buscar
                  OR CONCAT(nombre, ' ', apellido) LIKE @buscar
                  OR email LIKE @buscar)";

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

    // Obtener Afiliado por ID
    public Afiliado? GetById(int id)
    {
        var query = "SELECT * FROM Afiliado WHERE id = @id AND activo = 1";
        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", id);

        connection.Open();
        using MySqlDataReader reader = command.ExecuteReader();
        if (reader.Read())
        {
            return ParseAfiliado(reader);
        }

        return null;
    }

    // Validar si el email ya existe en la BD (excluyendo el propio ID si se está editando)
    public bool ExistsEmail(string email, int? idExcluir = null)
    {
        var query = @"SELECT COUNT(*) FROM Afiliado 
                      WHERE email = @email 
                      AND (@idExcluir IS NULL OR id != @idExcluir)";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@email", email);
        command.Parameters.AddWithValue("@idExcluir", (object?)idExcluir ?? DBNull.Value);

        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    // Crear Afiliado
    public int Create(Afiliado afiliado)
    {
        var query = @"INSERT INTO Afiliado (nombre, apellido, email, telefono, fecha_alta, estado, sancionado, activo) 
                      VALUES (@nombre, @apellido, @email, @telefono, @fechaAlta, @estado, @sancionado, 1)";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@nombre", afiliado.Nombre);
        command.Parameters.AddWithValue("@apellido", afiliado.Apellido);
        command.Parameters.AddWithValue("@email", afiliado.Email);
        command.Parameters.AddWithValue("@telefono", (object?)afiliado.Telefono ?? DBNull.Value);
        command.Parameters.AddWithValue("@fechaAlta", afiliado.FechaAlta);
        command.Parameters.AddWithValue("@estado", afiliado.Estado);
        command.Parameters.AddWithValue("@sancionado", afiliado.Sancionado);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    // Editar Afiliado
    public int Update(Afiliado afiliado)
    {
        var query = @"UPDATE Afiliado 
                      SET nombre = @nombre, apellido = @apellido, email = @email, telefono = @telefono, 
                          fecha_alta = @fechaAlta, estado = @estado, sancionado = @sancionado 
                      WHERE id = @id";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", afiliado.Id);
        command.Parameters.AddWithValue("@nombre", afiliado.Nombre);
        command.Parameters.AddWithValue("@apellido", afiliado.Apellido);
        command.Parameters.AddWithValue("@email", afiliado.Email);
        command.Parameters.AddWithValue("@telefono", (object?)afiliado.Telefono ?? DBNull.Value);
        command.Parameters.AddWithValue("@fechaAlta", afiliado.FechaAlta);
        command.Parameters.AddWithValue("@estado", afiliado.Estado);
        command.Parameters.AddWithValue("@sancionado", afiliado.Sancionado);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    // Borrado lógico (activo = 0)
    public int DeleteLogico(int id)
    {
        var query = "UPDATE Afiliado SET activo = 0 WHERE id = @id";
        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", id);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    // Obtener listado reducido para selects / búsquedas asíncronas AJAX
    public IList<Afiliado> GetParaSelect(string? buscar = null, int limite = 20)
    {
        var listado = new List<Afiliado>();

        string sql = @"
            SELECT id, nombre, apellido, email, estado, sancionado 
            FROM Afiliado 
            WHERE activo = 1";

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            sql += " AND (nombre LIKE @buscar OR apellido LIKE @buscar OR email LIKE @buscar)";
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
                        listado.Add(new Afiliado
                        {
                            Id = reader.GetInt32("id"),
                            Nombre = reader.GetString("nombre"),
                            Apellido = reader.GetString("apellido"),
                            Email = reader.GetString("email"),
                            Estado = reader.GetBoolean("estado"),
                            Sancionado = reader.GetBoolean("sancionado")
                        });
                    }
                }
            }
        }

        return listado;
    }

    // Función auxiliar para mapear el registro de la base de datos al modelo
    private static Afiliado ParseAfiliado(MySqlDataReader reader)
    {
        return new Afiliado
        {
            Id = reader.GetInt32("id"),
            Nombre = reader.GetString("nombre"),
            Apellido = reader.GetString("apellido"),
            Email = reader.GetString("email"),
            Telefono = reader.IsDBNull(reader.GetOrdinal("telefono")) ? string.Empty : reader.GetString("telefono"),
            FechaAlta = reader.GetDateTime("fecha_alta"),
            Estado = reader.GetBoolean("estado"),
            Sancionado = reader.GetBoolean("sancionado"),
            Activo = reader.GetBoolean("activo")
        };
    }
}