namespace BibliotecaPublica.Helpers;

public static class Archivos
{
    public const long TamanioMaximoImagen = 5 * 1024 * 1024; // 5 MB
    public static readonly string[] ExtensionesImagen = { ".jpg", ".jpeg", ".png", ".webp" };

    private const string CarpetaUploads = "uploads";

    // Devuelve null si la imagen es válida, o el mensaje de error si no lo es
    public static string? ValidarImagen(IFormFile archivo)
    {
        string extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        if (!ExtensionesImagen.Contains(extension))
        {
            return $"Formato de imagen no permitido. Usá {string.Join(", ", ExtensionesImagen)}.";
        }

        if (archivo.Length > TamanioMaximoImagen)
        {
            return $"La imagen no puede superar los {TamanioMaximoImagen / (1024 * 1024)} MB.";
        }

        return null;
    }

    // Guarda la imagen con un nombre único y devuelve la URL relativa (/uploads/carpeta/archivo.ext)
    public static async Task<string> GuardarImagenAsync(IFormFile archivo, IWebHostEnvironment environment, string carpeta)
    {
        string extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        string nombreArchivo = $"{Guid.NewGuid()}{extension}";

        string carpetaFisica = Path.Combine(environment.WebRootPath, CarpetaUploads, carpeta);
        Directory.CreateDirectory(carpetaFisica);

        using var stream = new FileStream(Path.Combine(carpetaFisica, nombreArchivo), FileMode.Create);
        await archivo.CopyToAsync(stream);

        return $"/{CarpetaUploads}/{carpeta}/{nombreArchivo}";
    }

    // Borra el archivo físico. Las URLs externas y cualquier ruta fuera de wwwroot/uploads se ignoran.
    public static void Borrar(string? urlRelativa, IWebHostEnvironment environment)
    {
        if (string.IsNullOrEmpty(urlRelativa) || urlRelativa.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return;

        string carpetaUploads = Path.GetFullPath(Path.Combine(environment.WebRootPath, CarpetaUploads));
        string rutaFisica = Path.GetFullPath(Path.Combine(environment.WebRootPath,
            urlRelativa.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)));

        if (!rutaFisica.StartsWith(carpetaUploads, StringComparison.OrdinalIgnoreCase)) return;

        if (File.Exists(rutaFisica))
        {
            File.Delete(rutaFisica);
        }
    }
}
