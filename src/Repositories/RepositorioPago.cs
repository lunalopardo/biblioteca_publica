using MySqlConnector;
using BibliotecaPublica.Models;

namespace BibliotecaPublica.Repositories;

public class RepositorioPago : RepositorioBase
{
    public RepositorioPago(IConfiguration configuration) : base(configuration) { }

    // idAfiliado = null > pagos de todos los afiliados
    public List<Pago> ObtenerPaginado(int? idAfiliado = null, string? buscar = null, int paginaNro = 1, int tamPagina = 10)
    {
        int offset = Math.Max(0, (paginaNro - 1) * tamPagina);
        var paramBuscar = string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : $"%{buscar.Trim()}%";

        const string query = @"SELECT p.*, a.nombre AS afiliado_nombre, a.apellido AS afiliado_apellido
                               FROM pago p
                               INNER JOIN afiliado a ON p.id_afiliado = a.id
                               WHERE p.activo = 1
                               AND (@idAfiliado IS NULL OR p.id_afiliado = @idAfiliado)
                               AND (@buscar IS NULL OR p.concepto LIKE @buscar
                                    OR a.nombre LIKE @buscar OR a.apellido LIKE @buscar)
                               ORDER BY p.fecha_pago DESC, p.id DESC
                               LIMIT @limit OFFSET @offset";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@idAfiliado", (object?)idAfiliado ?? DBNull.Value);
        command.Parameters.AddWithValue("@buscar", paramBuscar);
        command.Parameters.AddWithValue("@limit", tamPagina);
        command.Parameters.AddWithValue("@offset", offset);

        connection.Open();
        using MySqlDataReader reader = command.ExecuteReader();

        var listado = new List<Pago>();
        while (reader.Read())
        {
            listado.Add(Parse(reader));
        }
        return listado;
    }

    public int ObtenerCantidad(int? idAfiliado = null, string? buscar = null)
    {
        var paramBuscar = string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : $"%{buscar.Trim()}%";

        const string query = @"SELECT COUNT(p.id)
                               FROM pago p
                               INNER JOIN afiliado a ON p.id_afiliado = a.id
                               WHERE p.activo = 1
                               AND (@idAfiliado IS NULL OR p.id_afiliado = @idAfiliado)
                               AND (@buscar IS NULL OR p.concepto LIKE @buscar
                                    OR a.nombre LIKE @buscar OR a.apellido LIKE @buscar)";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@idAfiliado", (object?)idAfiliado ?? DBNull.Value);
        command.Parameters.AddWithValue("@buscar", paramBuscar);

        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public Pago? ObtenerPorId(int id)
    {
        const string query = @"SELECT p.*, a.nombre AS afiliado_nombre, a.apellido AS afiliado_apellido
                               FROM pago p
                               INNER JOIN afiliado a ON p.id_afiliado = a.id
                               WHERE p.id = @id AND p.activo = 1";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", id);

        connection.Open();
        using MySqlDataReader reader = command.ExecuteReader();
        return reader.Read() ? Parse(reader) : null;
    }

    // tiene una cuota pagada?
    public bool ExisteCuotaActiva(int idSuscripcion)
    {
        const string query = @"SELECT COUNT(id) FROM pago
                               WHERE id_suscripcion = @idSuscripcion AND activo = 1";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@idSuscripcion", idSuscripcion);

        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    public int Create(Pago pago)
    {
        const string query = @"INSERT INTO pago
                                 (id_afiliado, creado_por_user_id, tipo, id_suscripcion, id_prestamo,
                                  fecha_pago, monto, concepto, saldado, activo)
                               VALUES
                                 (@id_afiliado, @creado_por, @tipo, @id_suscripcion, @id_prestamo,
                                  @fecha_pago, @monto, @concepto, @saldado, 1);
                               SELECT LAST_INSERT_ID();";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id_afiliado", pago.IdAfiliado);
        command.Parameters.AddWithValue("@creado_por", pago.CreadoPorUserId);
        command.Parameters.AddWithValue("@tipo", pago.Tipo.ToString());
        command.Parameters.AddWithValue("@id_suscripcion", (object?)pago.IdSuscripcion ?? DBNull.Value);
        command.Parameters.AddWithValue("@id_prestamo", (object?)pago.IdPrestamo ?? DBNull.Value);
        command.Parameters.AddWithValue("@fecha_pago", pago.FechaPago.Date);
        command.Parameters.AddWithValue("@monto", pago.Monto);
        command.Parameters.AddWithValue("@concepto", pago.Concepto);
        command.Parameters.AddWithValue("@saldado", pago.Saldado);

        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar());
    }

    // Anular = baja lógica
    public int Anular(int id, int anuladoPorUserId)
    {
        const string query = @"UPDATE pago
                               SET activo = 0, anulado_por_user_id = @user
                               WHERE id = @id AND activo = 1";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@user", anuladoPorUserId);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    private static Pago Parse(MySqlDataReader reader)
    {
        return new Pago
        {
            Id = reader.GetInt32("id"),
            IdAfiliado = reader.GetInt32("id_afiliado"),
            CreadoPorUserId = reader.GetInt32("creado_por_user_id"),
            AnuladoPorUserId = reader.IsDBNull(reader.GetOrdinal("anulado_por_user_id"))
                ? null : reader.GetInt32("anulado_por_user_id"),
            Tipo = Enum.Parse<TipoPago>(reader.GetString("tipo")),
            IdSuscripcion = reader.IsDBNull(reader.GetOrdinal("id_suscripcion"))
                ? null : reader.GetInt32("id_suscripcion"),
            IdPrestamo = reader.IsDBNull(reader.GetOrdinal("id_prestamo"))
                ? null : reader.GetInt32("id_prestamo"),
            FechaPago = reader.GetDateTime("fecha_pago"),
            Monto = reader.GetDecimal("monto"),
            Concepto = reader.GetString("concepto"),
            Saldado = reader.GetBoolean("saldado"),
            Activo = reader.GetBoolean("activo"),
            Afiliado = new Afiliado
            {
                Id = reader.GetInt32("id_afiliado"),
                Nombre = reader.GetString("afiliado_nombre"),
                Apellido = reader.GetString("afiliado_apellido")
            }
        };
    }
}