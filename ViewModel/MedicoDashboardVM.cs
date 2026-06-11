namespace SistemaSaludGoya.ViewModel;

public class MedicoDashboardVM
{
    public string NombreCompleto { get; set; } = "";
    public string Matricula { get; set; } = "";
    public int IdMedico { get; set; }
    public int TurnosHoy { get; set; }
    public int TurnosPendientes { get; set; }
    public int TurnosConfirmados { get; set; }
    public int TurnosAtendidos { get; set; }
    public List<TurnoResumenVM> TurnosDelDia { get; set; } = new();
    public List<TurnoResumenVM> ProximosTurnos { get; set; } = new();
}

public class TurnoResumenVM
{
    public int IdTurno { get; set; }
    public int IdPaciente { get; set; }
    public DateTime FechaHora { get; set; }
    public string PacienteNombre { get; set; } = "";
    public string PacienteDni { get; set; } = "";
    public string Estado { get; set; } = "";
    public string? Motivo { get; set; }
    public bool TieneConsulta { get; set; }
}
