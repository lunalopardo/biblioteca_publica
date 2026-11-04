namespace BibliotecaPublica.Models;

public class RegistroAuditoria
{
    public const string OperacionPrestamo = "Préstamo";
    public const string OperacionPago = "Pago";

    public const string AccionRegistro = "Registró";
    public const string AccionDevolucion = "Devolvió";
    public const string AccionAnulacion = "Anuló";

    public DateTime? Fecha { get; set; }

    // Los registros anteriores a la auditoría solo tienen el día, no la hora
    public bool ConHora { get; set; }

    public string? Usuario { get; set; }
    public string Accion { get; set; } = string.Empty;
    public string Operacion { get; set; } = string.Empty;
    public int IdOperacion { get; set; }
    public string Detalle { get; set; } = string.Empty;
    public string Afiliado { get; set; } = string.Empty;

    public string FechaTexto => Fecha == null
        ? "Sin registro"
        : Fecha.Value.ToString(ConHora ? "dd/MM/yyyy HH:mm" : "dd/MM/yyyy");
}
