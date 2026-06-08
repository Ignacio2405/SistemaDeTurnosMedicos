namespace SistemaSaludGoya.Models;

public class MedicoEspecialidad
{
    public int IdMedico { get; set; }

    public int IdEspecialidad { get; set; }

    public Medico Medico { get; set; } = null!;

    public Especialidad Especialidad { get; set; } = null!;
}
