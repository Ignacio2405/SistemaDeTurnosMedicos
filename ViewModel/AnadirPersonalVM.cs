using System.ComponentModel.DataAnnotations;

namespace SistemaSaludGoya.ViewModel;

public class AnadirPersonalVM
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    public string Nombre { get; set; } = "";

    [Required(ErrorMessage = "El apellido es obligatorio")]
    public string Apellido { get; set; } = "";

    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [MinLength(6, ErrorMessage = "Mínimo 6 caracteres")]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "El rol es obligatorio")]
    public string RolAsignar { get; set; } = "Medico";

    public string? Matricula { get; set; }
    public string? Telefono { get; set; }

    public List<int> EspecialidadesSeleccionadas { get; set; } = new();
    public List<EspecialidadCheckVM> Especialidades { get; set; } = new();
}
