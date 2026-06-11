using System.ComponentModel.DataAnnotations;

namespace SistemaSaludGoya.ViewModel;

public class ConsultaVM
{
    public int IdTurno { get; set; }
    public string PacienteNombre { get; set; } = "";
    public string PacienteDni { get; set; } = "";
    public DateTime FechaHora { get; set; }
    public string? MotivoConsulta { get; set; }

    [Required(ErrorMessage = "El diagnóstico es obligatorio")]
    public string Diagnostico { get; set; } = null!;

    public string? Observaciones { get; set; }
    public string? Indicaciones { get; set; }
}
