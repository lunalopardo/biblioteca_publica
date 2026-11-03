using System.ComponentModel.DataAnnotations;

namespace BibliotecaPublica.Helpers;

// Valida extensión y tamaño de una imagen subida (IFormFile). Si no se subió nada, es válido.
public class ImagenValidaAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not IFormFile archivo || archivo.Length == 0) return ValidationResult.Success;

        string? error = Archivos.ValidarImagen(archivo);
        return error == null ? ValidationResult.Success : new ValidationResult(error);
    }
}
