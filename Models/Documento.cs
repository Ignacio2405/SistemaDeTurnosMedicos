namespace SistemaSaludGoya.Models;

public class Documento
{
    public int IdDocumento { get; set; }

    public int IdConsulta { get; set; }

    public string NombreArchivo { get; set; } = null!;

    public string RutaArchivo { get; set; } = null!;

    public DateTime FechaSubida { get; set; }

    public Consulta Consulta { get; set; } = null!;
}
