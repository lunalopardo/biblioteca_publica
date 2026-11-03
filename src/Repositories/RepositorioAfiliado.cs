using MySqlConnector;
using BibliotecaPublica.Models;

namespace BibliotecaPublica.Repositories;

public class RepositorioAfiliado : RepositorioBase
{
    public RepositorioAfiliado(IConfiguration configuration) : base(configuration) { }

    // un afiliado está sancionado mientras tenga préstamos vencidos sin devolver (las multas se cobran al registrar la devolución)
    private const string SancionCalculada = @"
        EXISTS(SELECT 1 FROM prestamo p
               WHERE p.id_afiliado = Afiliado.id
                 AND p.activo = 1
                 AND p.fecha_devolucion_real IS NULL
                 AND p.fecha_devolucion_pactada < CURDATE()) AS sancion_calculada";

    // Listado paginado con búsqueda general
    public IList<Afiliado> ObtenerPaginado(string? buscar = null, int paginaNro = 1, int tamPagina = 5)
    {
        var listado = new List<Afiliado>();
        int offset = Math.Max(0, (paginaNro - 1) * tamPagina);
        var paramBuscar = string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : $"%{buscar.Trim()}%";

        string sql = @"
            SELECT *, " + SancionCalculada + @"
            FROM Afiliado
            WHERE activo = 1
              AND (@buscar IS NULL OR nombre LIKE @buscar
              OR apellido LIKE @buscar
              OR CONCAT(nombre, ' ', apellido) LIKE @buscar
              OR email LIKE @buscar)
            ORDER BY id
            LIMIT @limit OFFSET @offset";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@buscar", paramBuscar);
        command.Parameters.AddWithValue("@limit", tamPagina);
        command.Parameters.AddWithValue("@offset", offset);

        connection.Open();
        using MySqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            listado.Add(ParseAfiliado(reader));
        }
        return listado;
    }

    // Obtener la cantidad total de registros activos filtrados (para la paginación)
    public int ObtenerCantidad(string? buscar = null)
    {
        var paramBuscar = string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : $"%{buscar.Trim()}%";

        string sql = @"
            SELECT COUNT(id)
            FROM Afiliado
            WHERE activo = 1
              AND (@buscar IS NULL OR nombre LIKE @buscar
              OR apellido LIKE @buscar
              OR CONCAT(nombre, ' ', apellido) LIKE @buscar
              OR email LIKE @buscar)";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@buscar", paramBuscar);

        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar());
    }

    // Obtener Afiliado por ID
    public Afiliado? ObtenerPorId(int id)
    {
        var query = "SELECT *, " + SancionCalculada + " FROM Afiliado WHERE id = @id AND activo = 1";
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
    public bool ExisteEmail(string email, int? idExcluir = null)
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
    public int Crear(Afiliado afiliado)
    {
        var query = @"INSERT INTO Afiliado (nombre, apellido, email, telefono, fecha_alta, activo)
                      VALUES (@nombre, @apellido, @email, @telefono, @fechaAlta, 1)";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@nombre", afiliado.Nombre);
        command.Parameters.AddWithValue("@apellido", afiliado.Apellido);
        command.Parameters.AddWithValue("@email", afiliado.Email);
        command.Parameters.AddWithValue("@telefono", (object?)afiliado.Telefono ?? DBNull.Value);
        command.Parameters.AddWithValue("@fechaAlta", afiliado.FechaAlta);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    // Editar Afiliado
    public int Actualizar(Afiliado afiliado)
    {
        var query = @"UPDATE Afiliado
                      SET nombre = @nombre, apellido = @apellido, email = @email, telefono = @telefono,
                          fecha_alta = @fechaAlta
                      WHERE id = @id";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", afiliado.Id);
        command.Parameters.AddWithValue("@nombre", afiliado.Nombre);
        command.Parameters.AddWithValue("@apellido", afiliado.Apellido);
        command.Parameters.AddWithValue("@email", afiliado.Email);
        command.Parameters.AddWithValue("@telefono", (object?)afiliado.Telefono ?? DBNull.Value);
        command.Parameters.AddWithValue("@fechaAlta", afiliado.FechaAlta);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    // Borrado lógico (activo = 0)
    public int BajaLogica(int id)
    {
        var query = "UPDATE Afiliado SET activo = 0 WHERE id = @id";
        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", id);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    // Obtener listado reducido para selects / búsquedas asíncronas AJAX
    public IList<Afiliado> ObtenerParaSelect(string? buscar = null, int limite = 20)
    {
        var listado = new List<Afiliado>();

        string sql = @"
            SELECT id, nombre, apellido, email, " + SancionCalculada + @"
            FROM Afiliado
            WHERE activo = 1";

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            sql += " AND (nombre LIKE @buscar OR apellido LIKE @buscar OR email LIKE @buscar)";
        }

        sql += " ORDER BY apellido, nombre ASC LIMIT @limite";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(sql, connection);
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            command.Parameters.AddWithValue("@buscar", $"%{buscar.Trim()}%");
        }
        command.Parameters.AddWithValue("@limite", limite);

        connection.Open();
        using MySqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            listado.Add(new Afiliado
            {
                Id = reader.GetInt32("id"),
                Nombre = reader.GetString("nombre"),
                Apellido = reader.GetString("apellido"),
                Email = reader.GetString("email"),
                Sancionado = Convert.ToInt32(reader["sancion_calculada"]) == 1
            });
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
            Sancionado = Convert.ToInt32(reader["sancion_calculada"]) == 1,
            Activo = reader.GetBoolean("activo")
        };
    }
}
