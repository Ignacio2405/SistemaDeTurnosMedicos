using System.ComponentModel.DataAnnotations;

namespace SistemaSaludGoya.ViewModel;

public class EditarPerfilVM
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

    [MinLength(6, ErrorMessage = "Mínimo 6 caracteres")]
    public string? NuevaPassword { get; set; }

    // Campos paciente
    public string? Dni { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public DateTime? FechaNacimiento { get; set; }

    // Campos médico
    public string? Matricula { get; set; }

    // Horarios del médico — un registro por día seleccionado
    public List<HorarioDiaVM> Horarios { get; set; } = new();
}

public class HorarioDiaVM
{
    public DayOfWeek DiaSemana { get; set; }
    public string NombreDia { get; set; } = "";
    public bool Activo { get; set; }
    public string HoraDesde { get; set; } = "08:00";
    public string HoraHasta { get; set; } = "13:00";
}
