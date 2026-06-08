namespace SistemaSaludGoya.Models;

public class HistorialMedico
{
    public int IdHistorialMedico { get; set; }

    public int IdPaciente { get; set; }

    public Paciente Paciente { get; set; } = null!;
}
