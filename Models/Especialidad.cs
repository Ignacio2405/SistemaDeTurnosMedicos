namespace SistemaSaludGoya.Models;

public class Especialidad
{
    public int IdEspecialidad { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public ICollection<MedicoEspecialidad> Medicos { get; set; }
        = new List<MedicoEspecialidad>();
}
