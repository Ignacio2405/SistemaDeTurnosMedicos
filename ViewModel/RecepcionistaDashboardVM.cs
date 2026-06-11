namespace SistemaSaludGoya.ViewModel;

public class RecepcionistaDashboardVM
{
    public List<TurnoAdminVM> Turnos { get; set; } = new();
    public string? FiltroMedico { get; set; }
    public string? FiltroEstado { get; set; }
    public DateTime FechaFiltro { get; set; } = DateTime.Today;
    public int TotalTurnos => Turnos.Count;
    public int TurnosPendientes => Turnos.Count(t => t.Estado == "Solicitado");
    public int TurnosConfirmados => Turnos.Count(t => t.Estado == "Confirmado");
    public int TurnosCancelados => Turnos.Count(t => t.Estado == "Cancelado");
}

public class TurnoAdminVM
{
    public int IdTurno { get; set; }
    public DateTime FechaHora { get; set; }
    public string PacienteNombre { get; set; } = "";
    public string MedicoNombre { get; set; } = "";
    public string Estado { get; set; } = "";
    public string? Motivo { get; set; }
}
