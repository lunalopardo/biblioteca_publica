using MySqlConnector;
using BibliotecaPublica.Models;

namespace BibliotecaPublica.Repositories;

public class RepositorioUsuario : RepositorioBase
{
    public RepositorioUsuario(IConfiguration configuration) : base(configuration) { }

    // Usuario activo por nombre de usuario o email (para el login). La contraseña se verifica en el controlador.
    public Usuario? ObtenerActivoPorLogin(string login)
    {
        string sql = @"
                SELECT * FROM usuario
                WHERE (nombre_usuario = @login OR email = @login)
                AND activo = 1";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@login", login);

        connection.Open();
        using MySqlDataReader reader = command.ExecuteReader();
        return reader.Read() ? ParseUsuario(reader) : null;
    }

    // Listado de usuarios (empleados) paginado
    public IList<Usuario> ObtenerPaginado(string? buscar = null, int paginaNro = 1, int tamPagina = 10)
    {
        var listado = new List<Usuario>();
        int offset = Math.Max(0, (paginaNro - 1) * tamPagina);
        var paramBuscar = string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : $"%{buscar.Trim()}%";

        string sql = @"
            SELECT * FROM usuario
            WHERE (@buscar IS NULL OR nombre_usuario LIKE @buscar
                                   OR nombre LIKE @buscar
                                   OR apellido LIKE @buscar
                                   OR CONCAT(Nombre, ' ', Apellido) LIKE @buscar
                                   OR email LIKE @buscar)
            ORDER BY id DESC
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
            listado.Add(ParseUsuario(reader));
        }
        return listado;
    }

    public int ObtenerCantidad(string? buscar = null)
    {
        var paramBuscar = string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : $"%{buscar.Trim()}%";

        string sql = @"
            SELECT COUNT(id) FROM usuario
            WHERE (@buscar IS NULL OR nombre_usuario LIKE @buscar
                                   OR nombre LIKE @buscar
                                   OR apellido LIKE @buscar
                                   OR CONCAT(Nombre, ' ', Apellido) LIKE @buscar
                                   OR email LIKE @buscar)";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@buscar", paramBuscar);

        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar());
    }

    // Traer un usuario por id
    public Usuario? ObtenerPorId(int id)
    {
        string sql = "SELECT * FROM usuario WHERE id = @id";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        connection.Open();
        using MySqlDataReader reader = command.ExecuteReader();
        if (reader.Read())
        {
            return ParseUsuario(reader);
        }
        return null;
    }

    // Crear usuario (no usamos registro). La contraseña tiene que llegar ya hasheada.
    public int Crear(Usuario usuario)
    {
        string sql = @"INSERT INTO usuario
                      (nombre_usuario, nombre, apellido, email, password, avatar, rol, activo)
                      VALUES
                      (@nombreUsuario, @nombre, @apellido, @email, @password, @avatar, @rol, 1)";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(sql, connection);

        command.Parameters.AddWithValue("@nombreUsuario", usuario.NombreUsuario);
        command.Parameters.AddWithValue("@nombre", usuario.Nombre);
        command.Parameters.AddWithValue("@apellido", usuario.Apellido);
        command.Parameters.AddWithValue("@email", usuario.Email);
        command.Parameters.AddWithValue("@password", usuario.Password);
        command.Parameters.AddWithValue("@avatar", (object?)usuario.Avatar ?? DBNull.Value);
        command.Parameters.AddWithValue("@rol", usuario.Rol);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    // Actualizar perfil
    public int ActualizarPerfil(Usuario usuario)
    {
        string sql = @"UPDATE usuario SET
                      nombre = @nombre,
                      apellido = @apellido,
                      email = @email,
                      password = @password,
                      avatar = @avatar
                      WHERE id = @id";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(sql, connection);

        command.Parameters.AddWithValue("@id", usuario.Id);
        command.Parameters.AddWithValue("@nombre", usuario.Nombre);
        command.Parameters.AddWithValue("@apellido", usuario.Apellido);
        command.Parameters.AddWithValue("@email", usuario.Email);
        command.Parameters.AddWithValue("@password", usuario.Password);
        command.Parameters.AddWithValue("@avatar", (object?)usuario.Avatar ?? DBNull.Value);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    // Actualización solo para admin
    public int ActualizarCompleto(Usuario usuario)
    {
        string sql = @"UPDATE usuario SET
                  nombre = @nombre,
                  apellido = @apellido,
                  email = @email,
                  password = @password,
                  avatar = @avatar,
                  rol = @rol,
                  activo = @activo
                  WHERE id = @id";

        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(sql, connection);

        command.Parameters.AddWithValue("@id", usuario.Id);
        command.Parameters.AddWithValue("@nombre", usuario.Nombre);
        command.Parameters.AddWithValue("@apellido", usuario.Apellido);
        command.Parameters.AddWithValue("@email", usuario.Email);
        command.Parameters.AddWithValue("@password", usuario.Password);
        command.Parameters.AddWithValue("@avatar", (object?)usuario.Avatar ?? DBNull.Value);
        command.Parameters.AddWithValue("@rol", usuario.Rol);
        command.Parameters.AddWithValue("@activo", usuario.Activo);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    // Borrado lógico
    public int BajaLogica(int id)
    {
        string sql = "UPDATE usuario SET activo = 0 WHERE id = @id";
        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        connection.Open();
        return command.ExecuteNonQuery();
    }

    // Reactivar usuarios (solo admin)
    public int Reactivar(int id)
    {
        string sql = "UPDATE usuario SET Activo = 1 WHERE Id = @id";
        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(sql, connection);

        command.Parameters.AddWithValue("@id", id);
        connection.Open();
        return command.ExecuteNonQuery();
    }

    // Chequear si existe el nombre de usuario y correo para no guardar dos iguales.
    public bool ExisteNombreUsuario(string nombreUsuario, int? idExcluir = null)
    {
        string sql = "SELECT COUNT(*) FROM usuario WHERE nombre_usuario = @nombreUsuario AND (@id IS NULL OR id != @id)";
        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@nombreUsuario", nombreUsuario);
        command.Parameters.AddWithValue("@id", (object?)idExcluir ?? DBNull.Value);

        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    public bool ExisteEmail(string email, int? idExcluir = null)
    {
        string sql = "SELECT COUNT(*) FROM usuario WHERE email = @email AND (@id IS NULL OR id != @id)";
        using MySqlConnection connection = new(connectionString);
        using MySqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@email", email);
        command.Parameters.AddWithValue("@id", (object?)idExcluir ?? DBNull.Value);

        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }


    internal static Usuario ParseUsuario(MySqlDataReader reader)
    {
        return new Usuario
        {
            Id = reader.GetInt32("id"),
            NombreUsuario = reader.GetString("nombre_usuario"),
            Nombre = reader.GetString("nombre"),
            Apellido = reader.GetString("apellido"),
            Email = reader.GetString("email"),
            Password = reader.GetString("password"),
            Avatar = reader.IsDBNull(reader.GetOrdinal("avatar")) ? null : reader.GetString("avatar"),
            Rol = Convert.ToInt32(reader["rol"]),
            Activo = reader.GetBoolean("activo")
        };
    }
}