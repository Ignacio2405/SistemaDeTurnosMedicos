namespace SistemaSaludGoya.ViewModel;

public class HomeDashboardVM
{
    public string NombreCompleto { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? Dni { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public int IdUsuario { get; set; }
    public int IdPaciente { get; set; }
    public int TurnosPendientes { get; set; }
    public int TurnosConfirmados { get; set; }
    public int TurnosAtendidos { get; set; }
    public int TurnosCancelados { get; set; }
    public int TurnosTotal => TurnosPendientes + TurnosConfirmados + TurnosAtendidos + TurnosCancelados;
    public DateTime? ProximoTurno { get; set; }
    public string? ProximoTurnoMedico { get; set; }
}
