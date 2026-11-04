using MySqlConnector;
using BibliotecaPublica.Models;

namespace BibliotecaPublica.Repositories;

public class RepositorioAuditoria : RepositorioBase
{
    public RepositorioAuditoria(IConfiguration configuration) : base(configuration) { }

    // Cada acción es una fila: alta, devolución y anulación de préstamos; alta y anulación de pagos.
    private const string Movimientos = @"
        SELECT COALESCE(pr.creado_en, pr.fecha_prestamo) AS fecha, pr.creado_en IS NOT NULL AS con_hora,
               pr.creado_por_user_id AS id_usuario, '" + RegistroAuditoria.AccionRegistro + @"' AS accion,
               '" + RegistroAuditoria.OperacionPrestamo + @"' AS operacion, pr.id AS id_operacion,
               CONCAT(l.titulo, ' (Cód. #', e.codigo_inventario, ')') AS detalle,
               CONCAT(a.apellido, ', ', a.nombre) AS afiliado
        FROM prestamo pr
        INNER JOIN afiliado a ON pr.id_afiliado = a.id
        INNER JOIN ejemplar e ON pr.id_ejemplar = e.id
        INNER JOIN libro l ON e.id_libro = l.id

        UNION ALL

        SELECT COALESCE(pr.devuelto_en, pr.fecha_devolucion_real), pr.devuelto_en IS NOT NULL,
               pr.devuelto_por_user_id, '" + RegistroAuditoria.AccionDevolucion + @"',
               '" + RegistroAuditoria.OperacionPrestamo + @"', pr.id,
               CONCAT(l.titulo, ' (Cód. #', e.codigo_inventario, ')'),
               CONCAT(a.apellido, ', ', a.nombre)
        FROM prestamo pr
        INNER JOIN afiliado a ON pr.id_afiliado = a.id
        INNER JOIN ejemplar e ON pr.id_ejemplar = e.id
        INNER JOIN libro l ON e.id_libro = l.id
        WHERE pr.fecha_devolucion_real IS NOT NULL

        UNION ALL

        SELECT pr.anulado_en, pr.anulado_en IS NOT NULL,
               pr.anulado_por_user_id, '" + RegistroAuditoria.AccionAnulacion + @"',
               '" + RegistroAuditoria.OperacionPrestamo + @"', pr.id,
               CONCAT(l.titulo, ' (Cód. #', e.codigo_inventario, ')'),
               CONCAT(a.apellido, ', ', a.nombre)
        FROM prestamo pr
        INNER JOIN afiliado a ON pr.id_afiliado = a.id
        INNER JOIN ejemplar e ON pr.id_ejemplar = e.id
        INNER JOIN libro l ON e.id_libro = l.id
        WHERE pr.activo = 0

        UNION ALL

        SELECT COALESCE(p.creado_en, p.fecha_pago), p.creado_en IS NOT NULL,
               p.creado_por_user_id, '" + RegistroAuditoria.AccionRegistro + @"',
               '" + RegistroAuditoria.OperacionPago + @"', p.id,
               CONCAT(p.concepto, ' ($', FORMAT(p.monto, 2, 'es_AR'), ')'),
               CONCAT(a.apellido, ', ', a.nombre)
        FROM pago p
        INNER JOIN afiliado a ON p.id_afiliado = a.id

        UNION ALL

        SELECT p.anulado_en, p.anulado_en IS NOT NULL,
               p.anulado_por_user_id, '" + RegistroAuditoria.AccionAnulacion + @"',
               '" + RegistroAuditoria.OperacionPago + @"', p.id,
               CONCAT(p.concepto, ' ($', FORMAT(p.monto, 2, 'es_AR'), ')'),
               CONCAT(a.apellido, ', ', a.nombre)
        FROM pago p
        INNER JOIN afiliado a ON p.id_afiliado = a.id
        WHERE p.activo = 0 ";

    private const string Filtro = @"
        WHERE (@idUsuario IS NULL OR m.id_usuario = @idUsuario)
          AND (@operacion IS NULL OR m.operacion = @operacion)
          AND (@desde IS NULL OR m.fecha >= @desde)
          AND (@hasta IS NULL OR m.fecha < DATE_ADD(@hasta, INTERVAL 1 DAY)) ";

    public List<RegistroAuditoria> ObtenerPaginado(int? idUsuario = null, string? operacion = null,
        DateTime? desde = null, DateTime? hasta = null, int paginaNro = 1, int tamPagina = 15)
    {
        int offset = Math.Max(0, (paginaNro - 1) * tamPagina);

        const string query = @"
            SELECT m.*, CONCAT(u.nombre, ' ', u.apellido) AS usuario
            FROM (" + Movimientos + @") m
            LEFT JOIN usuario u ON m.id_usuario = u.id "
            + Filtro + @"
            ORDER BY m.fecha DESC, m.id_operacion DESC, m.operacion, m.accion
            LIMIT @limit OFFSET @offset";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        CargarFiltros(command, idUsuario, operacion, desde, hasta);
        command.Parameters.AddWithValue("@limit", tamPagina);
        command.Parameters.AddWithValue("@offset", offset);

        connection.Open();
        using MySqlDataReader reader = command.ExecuteReader();

        var listado = new List<RegistroAuditoria>();
        while (reader.Read())
        {
            listado.Add(new RegistroAuditoria
            {
                Fecha = reader.IsDBNull(reader.GetOrdinal("fecha")) ? null : reader.GetDateTime("fecha"),
                ConHora = Convert.ToInt32(reader["con_hora"]) == 1,
                Usuario = reader.IsDBNull(reader.GetOrdinal("usuario")) ? null : reader.GetString("usuario"),
                Accion = reader.GetString("accion"),
                Operacion = reader.GetString("operacion"),
                IdOperacion = reader.GetInt32("id_operacion"),
                Detalle = reader.GetString("detalle"),
                Afiliado = reader.GetString("afiliado")
            });
        }
        return listado;
    }

    public int ObtenerCantidad(int? idUsuario = null, string? operacion = null,
        DateTime? desde = null, DateTime? hasta = null)
    {
        const string query = "SELECT COUNT(*) FROM (" + Movimientos + ") m " + Filtro;

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        CargarFiltros(command, idUsuario, operacion, desde, hasta);

        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar());
    }

    private static void CargarFiltros(MySqlCommand command, int? idUsuario, string? operacion,
        DateTime? desde, DateTime? hasta)
    {
        command.Parameters.AddWithValue("@idUsuario", (object?)idUsuario ?? DBNull.Value);
        command.Parameters.AddWithValue("@operacion", string.IsNullOrWhiteSpace(operacion) ? DBNull.Value : operacion);
        command.Parameters.AddWithValue("@desde", (object?)desde?.Date ?? DBNull.Value);
        command.Parameters.AddWithValue("@hasta", (object?)hasta?.Date ?? DBNull.Value);
    }
}
