namespace SistemaSaludGoya.Models;

public class Paciente
{
    public int IdPaciente { get; set; }

    public int IdUsuario { get; set; }

    public string Dni { get; set; } = null!;

    public DateTime FechaNacimiento { get; set; }

    public string? Telefono { get; set; }

    public string? Direccion { get; set; }

    public bool Suspendido { get; set; }

    public DateTime? FechaFinSuspension { get; set; }

    public string? MotivoSuspension { get; set; }

    // Navegación
    public Usuario Usuario { get; set; } = null!;

    public HistorialMedico HistorialMedico { get; set; } = null!;

    public ICollection<Turno> Turnos { get; set; }
        = new List<Turno>();
}
