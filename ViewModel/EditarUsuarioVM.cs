using System.ComponentModel.DataAnnotations;

namespace SistemaSaludGoya.ViewModel;

public class EditarUsuarioVM
{
    public int IdUsuario { get; set; }
    public string Rol { get; set; } = "";

    [Required(ErrorMessage = "El nombre es obligatorio")]
    public string Nombre { get; set; } = null!;

    [Required(ErrorMessage = "El apellido es obligatorio")]
    public string Apellido { get; set; } = null!;

    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; } = null!;

    // Si se deja vacío, no se cambia la contraseña
    [MinLength(6, ErrorMessage = "Mínimo 6 caracteres")]
    public string? NuevaPassword { get; set; }

    // Solo para médicos
    public string? Matricula { get; set; }
    public string? Telefono { get; set; }

    // Especialidades (solo para médicos)
    public List<EspecialidadCheckVM> Especialidades { get; set; } = new();
    public List<int> EspecialidadesSeleccionadas { get; set; } = new();
}
