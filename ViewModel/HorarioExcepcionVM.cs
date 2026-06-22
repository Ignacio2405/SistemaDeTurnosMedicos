namespace SistemaSaludGoya.ViewModel;

public class HorarioExcepcionVM
{
    public DateTime Fecha { get; set; }
    public bool Trabaja { get; set; }
    public string? HoraDesde { get; set; }
    public string? HoraHasta { get; set; }
    public int? Capacidad { get; set; }
}