using System.ComponentModel.DataAnnotations;

namespace SistemaSaludGoya.ViewModel;

public class AprobarMedicoVM
{
    public int IdUsuario { get; set; }
    public string NombreCompleto { get; set; } = "";
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "La matrícula es obligatoria para aprobar al médico")]
    public string Matricula { get; set; } = null!;

    public string? Telefono { get; set; }

    public string RolAsignar { get; set; } = "Medico"; // Medico | Recepcionista

    // Especialidades disponibles para mostrar en el form (se llena desde el controller)
    public List<EspecialidadCheckVM> Especialidades { get; set; } = new();

    // IDs de especialidades seleccionadas por el admin
    public List<int> EspecialidadesSeleccionadas { get; set; } = new();
}

public class EspecialidadCheckVM
{
    public int IdEspecialidad { get; set; }
    public string Nombre { get; set; } = "";
}
