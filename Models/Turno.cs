namespace SistemaSaludGoya.Models;

public class Turno
{
    public int IdTurno { get; set; }

    public int IdPaciente { get; set; }

    public int IdMedico { get; set; }

    public DateTime FechaHora { get; set; }

    public EstadoTurno Estado { get; set; }

    public string? MotivoConsulta { get; set; }

    public DateTime FechaCreacion { get; set; }

    public Paciente Paciente { get; set; } = null!;

    public Medico Medico { get; set; } = null!;

    public Consulta? Consulta { get; set; }

    public Pago? Pago { get; set; }
}
