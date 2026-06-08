using System.ComponentModel.DataAnnotations;

namespace SistemaSaludGoya.ViewModel;

public class RegisterPacienteVM
{
    [Required]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    public string Apellido { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Dni { get; set; } = string.Empty;

    public string? Telefono { get; set; }

    public string? Direccion { get; set; }

    public DateTime FechaNacimiento { get; set; }
}
