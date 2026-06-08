namespace SistemaSaludGoya.Models;

public class Estudio
{
    public int IdEstudio { get; set; }

    public int IdConsulta { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public Consulta Consulta { get; set; } = null!;
}
