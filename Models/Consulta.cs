namespace SistemaSaludGoya.Models;

public class Consulta
{
    public int IdConsulta { get; set; }

    public int IdTurno { get; set; }

    public string Diagnostico { get; set; } = null!;

    public string? Observaciones { get; set; }

    public string? Indicaciones { get; set; }

    public DateTime FechaConsulta { get; set; }

    public Turno Turno { get; set; } = null!;

    public ICollection<Receta> Recetas { get; set; }
        = new List<Receta>();

    public ICollection<Estudio> Estudios { get; set; }
        = new List<Estudio>();

    public ICollection<Documento> Documentos { get; set; }
        = new List<Documento>();
}
