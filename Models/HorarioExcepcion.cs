namespace SistemaSaludGoya.Models;

public class HorarioExcepcion
{
    public int IdHorarioExcepcion { get; set; }
    public int IdMedico { get; set; }
    public DateTime Fecha { get; set; }
    public bool Trabaja { get; set; }
    public TimeSpan? HoraDesde { get; set; }
    public TimeSpan? HoraHasta { get; set; }
    public int? Capacidad { get; set; }

    public Medico Medico { get; set; } = null!;
}