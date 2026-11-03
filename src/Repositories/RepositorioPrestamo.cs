using MySqlConnector;
using BibliotecaPublica.Models;

namespace BibliotecaPublica.Repositories;

public class RepositorioPrestamo : RepositorioBase
{
    public RepositorioPrestamo(IConfiguration configuration) : base(configuration) { }

    private const string SelectBase = @"
        SELECT pr.*,
               a.nombre AS afiliado_nombre, a.apellido AS afiliado_apellido,
               e.codigo_inventario AS ejemplar_codigo,
               l.titulo AS libro_titulo ";

    private const string FromConJoins = @"
        FROM prestamo pr
        INNER JOIN afiliado a ON pr.id_afiliado = a.id
        INNER JOIN ejemplar e ON pr.id_ejemplar = e.id
        INNER JOIN libro l ON e.id_libro = l.id ";

    private const string Filtro = @"
        WHERE pr.activo = 1
          AND (@buscar IS NULL
               OR a.nombre LIKE @buscar OR a.apellido LIKE @buscar
               OR CONCAT(a.nombre, ' ', a.apellido) LIKE @buscar
               OR l.titulo LIKE @buscar
               OR e.codigo_inventario LIKE @buscar) ";

    public List<Prestamo> ObtenerPaginado(string? buscar = null, int paginaNro = 1, int tamPagina = 10)
    {
        int offset = Math.Max(0, (paginaNro - 1) * tamPagina);
        var paramBuscar = string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : $"%{buscar.Trim()}%";

        const string query = SelectBase + FromConJoins + Filtro + @"
            ORDER BY (pr.fecha_devolucion_real IS NULL) DESC, pr.id DESC
            LIMIT @limit OFFSET @offset";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@buscar", paramBuscar);
        command.Parameters.AddWithValue("@limit", tamPagina);
        command.Parameters.AddWithValue("@offset", offset);

        connection.Open();
        using MySqlDataReader reader = command.ExecuteReader();

        var listado = new List<Prestamo>();
        while (reader.Read())
        {
            listado.Add(Parse(reader));
        }
        return listado;
    }

    public int ObtenerCantidad(string? buscar = null)
    {
        var paramBuscar = string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : $"%{buscar.Trim()}%";

        const string query = "SELECT COUNT(pr.id) " + FromConJoins + Filtro;

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@buscar", paramBuscar);

        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public Prestamo? ObtenerPorId(int id)
    {
        const string query = SelectBase + FromConJoins + " WHERE pr.id = @id AND pr.activo = 1";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", id);

        connection.Open();
        using MySqlDataReader reader = command.ExecuteReader();
        return reader.Read() ? Parse(reader) : null;
    }

    // chequear si el afiliado tiene algún préstamo en progreso con la fecha pactada ya pasada
    public bool TieneVencidos(int idAfiliado)
    {
        const string query = @"SELECT COUNT(id) FROM prestamo
                               WHERE id_afiliado = @idAfiliado
                                 AND activo = 1
                                 AND fecha_devolucion_real IS NULL
                                 AND fecha_devolucion_pactada < CURDATE()";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@idAfiliado", idAfiliado);

        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    // Crea el préstamo y marca el ejemplar como Prestado.
    // Devuelve el id nuevo, o 0 si el ejemplar ya no estaba disponible.
    public int Create(Prestamo prestamo)
    {
        using MySqlConnection connection = new(connectionString);
        connection.Open();
        using MySqlTransaction tx = connection.BeginTransaction();

        // Marcar el ejemplar como Prestado SOLO si sigue disponible.
        const string marcarEjemplar = @"UPDATE ejemplar SET estado = 'Prestado'
                                        WHERE id = @id AND estado = 'Disponible' AND activo = 1";
        using (var cmd = new MySqlCommand(marcarEjemplar, connection, tx))
        {
            cmd.Parameters.AddWithValue("@id", prestamo.IdEjemplar);
            if (cmd.ExecuteNonQuery() == 0) return 0;
        }

        const string insertar = @"INSERT INTO prestamo
                                    (id_afiliado, id_ejemplar, creado_por_user_id, fecha_prestamo,
                                     fecha_devolucion_pactada, estado, activo)
                                  VALUES
                                    (@id_afiliado, @id_ejemplar, @creado_por, @fecha_prestamo,
                                     @fecha_pactada, @estado, 1);
                                  SELECT LAST_INSERT_ID();";
        int id;
        using (var cmd = new MySqlCommand(insertar, connection, tx))
        {
            cmd.Parameters.AddWithValue("@id_afiliado", prestamo.IdAfiliado);
            cmd.Parameters.AddWithValue("@id_ejemplar", prestamo.IdEjemplar);
            cmd.Parameters.AddWithValue("@creado_por", prestamo.CreadoPorUserId);
            cmd.Parameters.AddWithValue("@fecha_prestamo", prestamo.FechaPrestamo.Date);
            cmd.Parameters.AddWithValue("@fecha_pactada", prestamo.FechaDevolucionPactada.Date);
            cmd.Parameters.AddWithValue("@estado", Prestamo.EstadoEnProgreso);
            id = Convert.ToInt32(cmd.ExecuteScalar());
        }

        tx.Commit();
        return id;
    }

    // Registra la devolución, libera el ejemplar y (si corresponde) guarda la multa.
    public bool Devolver(int id, DateTime fechaReal, Pago? multa)
    {
        using MySqlConnection connection = new(connectionString);
        connection.Open();
        using MySqlTransaction tx = connection.BeginTransaction();

        const string actualizarPrestamo = @"UPDATE prestamo
                                            SET fecha_devolucion_real = @fecha, estado = @estado
                                            WHERE id = @id AND activo = 1 AND fecha_devolucion_real IS NULL";
        using (var cmd = new MySqlCommand(actualizarPrestamo, connection, tx))
        {
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@fecha", fechaReal.Date);
            cmd.Parameters.AddWithValue("@estado", Prestamo.EstadoDevuelto);
            if (cmd.ExecuteNonQuery() == 0) return false;
        }

        const string liberarEjemplar = @"UPDATE ejemplar e
                                         INNER JOIN prestamo pr ON pr.id_ejemplar = e.id
                                         SET e.estado = 'Disponible'
                                         WHERE pr.id = @id";
        using (var cmd = new MySqlCommand(liberarEjemplar, connection, tx))
        {
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        if (multa != null)
        {
            const string insertarMulta = @"INSERT INTO pago
                                             (id_afiliado, creado_por_user_id, tipo, id_prestamo,
                                              fecha_pago, monto, concepto, saldado, activo)
                                           VALUES
                                             (@id_afiliado, @creado_por, @tipo, @id_prestamo,
                                              @fecha_pago, @monto, @concepto, @saldado, 1)";
            using var cmd = new MySqlCommand(insertarMulta, connection, tx);
            cmd.Parameters.AddWithValue("@id_afiliado", multa.IdAfiliado);
            cmd.Parameters.AddWithValue("@creado_por", multa.CreadoPorUserId);
            cmd.Parameters.AddWithValue("@tipo", multa.Tipo.ToString());
            cmd.Parameters.AddWithValue("@id_prestamo", id);
            cmd.Parameters.AddWithValue("@fecha_pago", multa.FechaPago.Date);
            cmd.Parameters.AddWithValue("@monto", multa.Monto);
            cmd.Parameters.AddWithValue("@concepto", multa.Concepto);
            cmd.Parameters.AddWithValue("@saldado", multa.Saldado);
            cmd.ExecuteNonQuery();
        }

        tx.Commit();
        return true;
    }

    // Anula un préstamo cargado por error. Solo si todavía no se devolvió.
    public bool Anular(int id, int anuladoPorUserId)
    {
        using MySqlConnection connection = new(connectionString);
        connection.Open();
        using MySqlTransaction tx = connection.BeginTransaction();

        const string anular = @"UPDATE prestamo
                                SET activo = 0, anulado_por_user_id = @user
                                WHERE id = @id AND activo = 1 AND fecha_devolucion_real IS NULL";
        using (var cmd = new MySqlCommand(anular, connection, tx))
        {
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@user", anuladoPorUserId);
            if (cmd.ExecuteNonQuery() == 0) return false;
        }

        const string liberarEjemplar = @"UPDATE ejemplar e
                                         INNER JOIN prestamo pr ON pr.id_ejemplar = e.id
                                         SET e.estado = 'Disponible'
                                         WHERE pr.id = @id";
        using (var cmd = new MySqlCommand(liberarEjemplar, connection, tx))
        {
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        tx.Commit();
        return true;
    }

    private static Prestamo Parse(MySqlDataReader reader)
    {
        return new Prestamo
        {
            Id = reader.GetInt32("id"),
            IdAfiliado = reader.GetInt32("id_afiliado"),
            IdEjemplar = reader.GetInt32("id_ejemplar"),
            CreadoPorUserId = reader.GetInt32("creado_por_user_id"),
            AnuladoPorUserId = reader.IsDBNull(reader.GetOrdinal("anulado_por_user_id"))
                ? null : reader.GetInt32("anulado_por_user_id"),
            FechaPrestamo = reader.GetDateTime("fecha_prestamo"),
            FechaDevolucionPactada = reader.GetDateTime("fecha_devolucion_pactada"),
            FechaDevolucionReal = reader.IsDBNull(reader.GetOrdinal("fecha_devolucion_real"))
                ? null : reader.GetDateTime("fecha_devolucion_real"),
            Estado = reader.GetString("estado"),
            Activo = reader.GetBoolean("activo"),
            Afiliado = new Afiliado
            {
                Id = reader.GetInt32("id_afiliado"),
                Nombre = reader.GetString("afiliado_nombre"),
                Apellido = reader.GetString("afiliado_apellido")
            },
            Ejemplar = new Ejemplar
            {
                Id = reader.GetInt32("id_ejemplar"),
                CodigoInventario = reader.GetInt32("ejemplar_codigo"),
                Libro = new Libro { Titulo = reader.GetString("libro_titulo") }
            }
        };
    }
}