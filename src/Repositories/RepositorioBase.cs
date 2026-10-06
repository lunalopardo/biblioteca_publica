namespace BibliotecaPublica.Repositories;

public abstract class RepositorioBase
{
    protected readonly string connectionString;

    public RepositorioBase(IConfiguration configuration)
    {
        connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión.");
    }
}