namespace SistemaSaludGoya.ViewModel;

public class HistorialPacienteVM
{
    public int IdPaciente { get; set; }
    public string NombreCompleto { get; set; } = "";
    public string Dni { get; set; } = "";
    public string? Telefono { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public List<ConsultaHistorialVM> Consultas { get; set; } = new();
}

public class ConsultaHistorialVM
{
    public int IdConsulta { get; set; }
    public DateTime FechaConsulta { get; set; }
    public string MedicoNombre { get; set; } = "";
    public string Diagnostico { get; set; } = "";
    public string? Observaciones { get; set; }
    public string? Indicaciones { get; set; }
}
