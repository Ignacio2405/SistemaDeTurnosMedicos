namespace SistemaSaludGoya.Models;

public class Receta
{
    public int IdReceta { get; set; }

    public int IdConsulta { get; set; }

    public string Descripcion { get; set; } = null!;

    public DateTime FechaEmision { get; set; }

    public Consulta Consulta { get; set; } = null!;
}
