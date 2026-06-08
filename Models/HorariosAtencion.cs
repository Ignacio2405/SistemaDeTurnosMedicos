namespace SistemaSaludGoya.Models;

public class HorarioAtencion
{
    public int IdHorarioAtencion { get; set; }

    public int IdMedico { get; set; }

    public DayOfWeek DiaSemana { get; set; }

    public TimeSpan HoraDesde { get; set; }

    public TimeSpan HoraHasta { get; set; }

    public Medico Medico { get; set; } = null!;
}
