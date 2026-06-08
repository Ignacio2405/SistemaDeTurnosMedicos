namespace SistemaSaludGoya.Models;

public class Medico
{
    public int IdMedico { get; set; }

    public int IdUsuario { get; set; }

    public string Matricula { get; set; } = null!;

    public string? Telefono { get; set; }

    // Navegación
    public Usuario Usuario { get; set; } = null!;

    public ICollection<MedicoEspecialidad> Especialidades { get; set; }
        = new List<MedicoEspecialidad>();

    public ICollection<HorarioAtencion> Horarios { get; set; }
        = new List<HorarioAtencion>();

    public ICollection<Turno> Turnos { get; set; }
        = new List<Turno>();
}
