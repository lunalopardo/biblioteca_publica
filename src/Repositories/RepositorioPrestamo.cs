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

    // @idAfiliado: listado de un solo afiliado (botón "Préstamos" del listado de afiliados)
    private const string Filtro = @"
        WHERE pr.activo = 1
          AND (@idAfiliado IS NULL OR pr.id_afiliado = @idAfiliado)
          AND (@buscar IS NULL
               OR a.nombre LIKE @buscar OR a.apellido LIKE @buscar
               OR CONCAT(a.nombre, ' ', a.apellido) LIKE @buscar
               OR l.titulo LIKE @buscar
               OR e.codigo_inventario LIKE @buscar) ";

    public List<Prestamo> ObtenerPaginado(string? buscar = null, int? idAfiliado = null, int paginaNro = 1, int tamPagina = 10)
    {
        int offset = Math.Max(0, (paginaNro - 1) * tamPagina);
        var paramBuscar = string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : $"%{buscar.Trim()}%";

        const string query = SelectBase + FromConJoins + Filtro + @"
            ORDER BY (pr.fecha_devolucion_real IS NULL) DESC,
                     (pr.fecha_devolucion_real IS NULL AND pr.fecha_devolucion_pactada < CURDATE()) DESC,
                     pr.id DESC
            LIMIT @limit OFFSET @offset";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@buscar", paramBuscar);
        command.Parameters.AddWithValue("@idAfiliado", (object?)idAfiliado ?? DBNull.Value);
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

    public int ObtenerCantidad(string? buscar = null, int? idAfiliado = null)
    {
        var paramBuscar = string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : $"%{buscar.Trim()}%";

        const string query = "SELECT COUNT(pr.id) " + FromConJoins + Filtro;

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@buscar", paramBuscar);
        command.Parameters.AddWithValue("@idAfiliado", (object?)idAfiliado ?? DBNull.Value);

        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar());
    }

    // Préstamos que todavía no se devolvieron (para el panel de inicio)
    public int ObtenerCantidadVigentes()
    {
        const string query = @"SELECT COUNT(id) FROM prestamo
                               WHERE activo = 1 AND fecha_devolucion_real IS NULL";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);

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

    // Crea el préstamo y marca el ejemplar como Prestado.
    // Devuelve el id nuevo, o 0 si el ejemplar ya no estaba disponible.
    public int Crear(Prestamo prestamo)
    {
        using MySqlConnection connection = new(connectionString);
        connection.Open();
        using MySqlTransaction tx = connection.BeginTransaction();

        // Marcar el ejemplar como Prestado SOLO si sigue disponible.
        const string marcarEjemplar = @"UPDATE ejemplar SET estado = 'Prestado'
                                        WHERE id = @id AND estado = 'Disponible' AND activo = 1";
        using var cmdMarcar = new MySqlCommand(marcarEjemplar, connection, tx);
        cmdMarcar.Parameters.AddWithValue("@id", prestamo.IdEjemplar);
        if (cmdMarcar.ExecuteNonQuery() == 0) return 0;

        const string insertar = @"INSERT INTO prestamo
                                    (id_afiliado, id_ejemplar, creado_por_user_id, creado_en, fecha_prestamo,
                                     fecha_devolucion_pactada, estado, activo)
                                  VALUES
                                    (@id_afiliado, @id_ejemplar, @creado_por, NOW(), @fecha_prestamo,
                                     @fecha_pactada, @estado, 1);
                                  SELECT LAST_INSERT_ID();";
        using var cmdInsertar = new MySqlCommand(insertar, connection, tx);
        cmdInsertar.Parameters.AddWithValue("@id_afiliado", prestamo.IdAfiliado);
        cmdInsertar.Parameters.AddWithValue("@id_ejemplar", prestamo.IdEjemplar);
        cmdInsertar.Parameters.AddWithValue("@creado_por", prestamo.CreadoPorUserId);
        cmdInsertar.Parameters.AddWithValue("@fecha_prestamo", prestamo.FechaPrestamo.Date);
        cmdInsertar.Parameters.AddWithValue("@fecha_pactada", prestamo.FechaDevolucionPactada.Date);
        cmdInsertar.Parameters.AddWithValue("@estado", Prestamo.EstadoEnProgreso);
        int id = Convert.ToInt32(cmdInsertar.ExecuteScalar());

        tx.Commit();
        return id;
    }

    // Registra la devolución, libera el ejemplar y (si corresponde) guarda la multa.
    public bool Devolver(int id, DateTime fechaReal, int devueltoPorUserId, Pago? multa)
    {
        using MySqlConnection connection = new(connectionString);
        connection.Open();
        using MySqlTransaction tx = connection.BeginTransaction();

        const string actualizarPrestamo = @"UPDATE prestamo
                                            SET fecha_devolucion_real = @fecha, estado = @estado,
                                                devuelto_por_user_id = @user, devuelto_en = NOW()
                                            WHERE id = @id AND activo = 1 AND fecha_devolucion_real IS NULL";
        using var cmdActualizar = new MySqlCommand(actualizarPrestamo, connection, tx);
        cmdActualizar.Parameters.AddWithValue("@id", id);
        cmdActualizar.Parameters.AddWithValue("@fecha", fechaReal.Date);
        cmdActualizar.Parameters.AddWithValue("@estado", Prestamo.EstadoDevuelto);
        cmdActualizar.Parameters.AddWithValue("@user", devueltoPorUserId);
        if (cmdActualizar.ExecuteNonQuery() == 0) return false;

        const string liberarEjemplar = @"UPDATE ejemplar e
                                         INNER JOIN prestamo pr ON pr.id_ejemplar = e.id
                                         SET e.estado = 'Disponible'
                                         WHERE pr.id = @id";
        using var cmdLiberar = new MySqlCommand(liberarEjemplar, connection, tx);
        cmdLiberar.Parameters.AddWithValue("@id", id);
        cmdLiberar.ExecuteNonQuery();

        if (multa != null)
        {
            const string insertarMulta = @"INSERT INTO pago
                                             (id_afiliado, creado_por_user_id, creado_en, tipo, id_prestamo,
                                              fecha_pago, monto, concepto, saldado, activo)
                                           VALUES
                                             (@id_afiliado, @creado_por, NOW(), @tipo, @id_prestamo,
                                              @fecha_pago, @monto, @concepto, @saldado, 1)";
            using var cmdMulta = new MySqlCommand(insertarMulta, connection, tx);
            cmdMulta.Parameters.AddWithValue("@id_afiliado", multa.IdAfiliado);
            cmdMulta.Parameters.AddWithValue("@creado_por", multa.CreadoPorUserId);
            cmdMulta.Parameters.AddWithValue("@tipo", multa.Tipo.ToString());
            cmdMulta.Parameters.AddWithValue("@id_prestamo", id);
            cmdMulta.Parameters.AddWithValue("@fecha_pago", multa.FechaPago.Date);
            cmdMulta.Parameters.AddWithValue("@monto", multa.Monto);
            cmdMulta.Parameters.AddWithValue("@concepto", multa.Concepto);
            cmdMulta.Parameters.AddWithValue("@saldado", multa.Saldado);
            cmdMulta.ExecuteNonQuery();
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
                                SET activo = 0, anulado_por_user_id = @user, anulado_en = NOW()
                                WHERE id = @id AND activo = 1 AND fecha_devolucion_real IS NULL";
        using var cmdAnular = new MySqlCommand(anular, connection, tx);
        cmdAnular.Parameters.AddWithValue("@id", id);
        cmdAnular.Parameters.AddWithValue("@user", anuladoPorUserId);
        if (cmdAnular.ExecuteNonQuery() == 0) return false;

        const string liberarEjemplar = @"UPDATE ejemplar e
                                         INNER JOIN prestamo pr ON pr.id_ejemplar = e.id
                                         SET e.estado = 'Disponible'
                                         WHERE pr.id = @id";
        using var cmdLiberar = new MySqlCommand(liberarEjemplar, connection, tx);
        cmdLiberar.Parameters.AddWithValue("@id", id);
        cmdLiberar.ExecuteNonQuery();

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
            DevueltoPorUserId = reader.IsDBNull(reader.GetOrdinal("devuelto_por_user_id"))
                ? null : reader.GetInt32("devuelto_por_user_id"),
            CreadoEn = reader.IsDBNull(reader.GetOrdinal("creado_en"))
                ? null : reader.GetDateTime("creado_en"),
            DevueltoEn = reader.IsDBNull(reader.GetOrdinal("devuelto_en"))
                ? null : reader.GetDateTime("devuelto_en"),
            AnuladoEn = reader.IsDBNull(reader.GetOrdinal("anulado_en"))
                ? null : reader.GetDateTime("anulado_en"),
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