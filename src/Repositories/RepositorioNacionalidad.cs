using MySqlConnector;
using BibliotecaPublica.Models;

namespace BibliotecaPublica.Repositories;

public class RepositorioNacionalidad : RepositorioBase
{
    public RepositorioNacionalidad(IConfiguration configuration) : base(configuration) { }

    public IList<Nacionalidad> ObtenerTodas()
    {
        var listado = new List<Nacionalidad>();
        string sql = "SELECT id, nombre, activo FROM nacionalidad WHERE activo = 1 ORDER BY nombre ASC";

        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        listado.Add(new Nacionalidad
                        {
                            Id = reader.GetInt32("id"),
                            Nombre = reader.GetString("nombre"),
                            Activo = reader.GetBoolean("activo")
                        });
                    }
                }
            }
        }
        return listado;
    }
}