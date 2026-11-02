using MySqlConnector;
using BibliotecaPublica.Models;

namespace BibliotecaPublica.Repositories;

public class RepositorioSuscripcion : RepositorioBase
{
    public RepositorioSuscripcion(IConfiguration configuration) : base(configuration) { }

    private const string SelectBase = @"
        SELECT s.id, s.id_afiliado, s.fecha_inicio, s.fecha_fin, s.monto, s.activo,
               EXISTS(SELECT 1 FROM pago p
                      WHERE p.id_suscripcion = s.id AND p.activo = 1) AS pagada
        FROM suscripcion s";

    public List<Suscripcion> ObtenerPorAfiliado(int idAfiliado)
    {
        const string query = SelectBase + @"
            WHERE s.id_afiliado = @idAfiliado AND s.activo = 1
            ORDER BY s.fecha_fin DESC";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@idAfiliado", idAfiliado);

        connection.Open();
        using MySqlDataReader reader = command.ExecuteReader();

        var listado = new List<Suscripcion>();
        while (reader.Read())
        {
            listado.Add(Parse(reader));
        }
        return listado;
    }

    public Suscripcion? ObtenerPorId(int id)
    {
        const string query = SelectBase + @"
            WHERE s.id = @id AND s.activo = 1";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", id);

        connection.Open();
        using MySqlDataReader reader = command.ExecuteReader();
        return reader.Read() ? Parse(reader) : null;
    }

    // Suscripción vigente de hoy Y pagada, o null si no tiene
    public Suscripcion? ObtenerVigente(int idAfiliado)
    {
        const string query = SelectBase + @"
            WHERE s.id_afiliado = @idAfiliado
              AND s.activo = 1
              AND CURDATE() BETWEEN s.fecha_inicio AND s.fecha_fin
              AND EXISTS(SELECT 1 FROM pago p
                         WHERE p.id_suscripcion = s.id AND p.activo = 1)
            ORDER BY s.fecha_fin DESC
            LIMIT 1";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@idAfiliado", idAfiliado);

        connection.Open();
        using MySqlDataReader reader = command.ExecuteReader();
        return reader.Read() ? Parse(reader) : null;
    }

    // ver si se superpone con otra suscripción del mismo afiliado
    public bool ExisteSuperposicion(int idAfiliado, DateTime inicio, DateTime fin, int idExcluir = 0)
    {
        const string query = @"SELECT COUNT(id) FROM suscripcion
                               WHERE id_afiliado = @idAfiliado
                                 AND activo = 1
                                 AND id <> @idExcluir
                                 AND fecha_inicio <= @fin
                                 AND fecha_fin >= @inicio";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@idAfiliado", idAfiliado);
        command.Parameters.AddWithValue("@idExcluir", idExcluir);
        command.Parameters.AddWithValue("@inicio", inicio.Date);
        command.Parameters.AddWithValue("@fin", fin.Date);

        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    public int Create(Suscripcion s)
    {
        const string query = @"INSERT INTO suscripcion (id_afiliado, fecha_inicio, fecha_fin, monto, activo)
                               VALUES (@id_afiliado, @fecha_inicio, @fecha_fin, @monto, 1);
                               SELECT LAST_INSERT_ID();";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id_afiliado", s.IdAfiliado);
        command.Parameters.AddWithValue("@fecha_inicio", s.FechaInicio.Date);
        command.Parameters.AddWithValue("@fecha_fin", s.FechaFin.Date);
        command.Parameters.AddWithValue("@monto", s.Monto);

        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public int Update(Suscripcion s)
    {
        const string query = @"UPDATE suscripcion SET
                                 fecha_inicio = @fecha_inicio,
                                 fecha_fin = @fecha_fin,
                                 monto = @monto
                               WHERE id = @id AND activo = 1";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", s.Id);
        command.Parameters.AddWithValue("@fecha_inicio", s.FechaInicio.Date);
        command.Parameters.AddWithValue("@fecha_fin", s.FechaFin.Date);
        command.Parameters.AddWithValue("@monto", s.Monto);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    public int AnularLogico(int id)
    {
        const string query = "UPDATE suscripcion SET activo = 0 WHERE id = @id";
        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", id);
        connection.Open();
        return command.ExecuteNonQuery();
    }

    public Dictionary<int, bool> ObtenerEstadoActual(IEnumerable<int> idsAfiliados)
    {
        var resultado = new Dictionary<int, bool>();
        var ids = idsAfiliados.Distinct().ToList();
        if (ids.Count == 0) return resultado;

        var nombres = ids.Select((_, i) => $"@id{i}").ToList();

        string query = $@"SELECT s.id_afiliado,
                             EXISTS(SELECT 1 FROM pago p
                                    WHERE p.id_suscripcion = s.id AND p.activo = 1) AS pagada
                      FROM suscripcion s
                      WHERE s.activo = 1
                        AND CURDATE() BETWEEN s.fecha_inicio AND s.fecha_fin
                        AND s.id_afiliado IN ({string.Join(",", nombres)})";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(query, connection);
        for (int i = 0; i < ids.Count; i++)
        {
            command.Parameters.AddWithValue(nombres[i], ids[i]);
        }

        connection.Open();
        using MySqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            int idAfiliado = reader.GetInt32("id_afiliado");
            bool pagada = Convert.ToInt32(reader["pagada"]) == 1;
            resultado[idAfiliado] = resultado.GetValueOrDefault(idAfiliado) || pagada;
        }
        return resultado;
    }

    private static Suscripcion Parse(MySqlDataReader reader)
    {
        return new Suscripcion
        {
            Id = reader.GetInt32("id"),
            IdAfiliado = reader.GetInt32("id_afiliado"),
            FechaInicio = reader.GetDateTime("fecha_inicio"),
            FechaFin = reader.GetDateTime("fecha_fin"),
            Monto = reader.GetDecimal("monto"),
            Activo = reader.GetBoolean("activo"),
            Pagada = Convert.ToInt32(reader["pagada"]) == 1
        };
    }
}