using System.ComponentModel.DataAnnotations;

namespace SistemaSaludGoya.ViewModel;

public class RegisterPacienteVM
{
    [Required] public string Nombre { get; set; } = null!;
    [Required] public string Apellido { get; set; } = null!;
    [Required][EmailAddress] public string Email { get; set; } = null!;
    [Required] public string Password { get; set; } = null!;
    [Required] public string Dni { get; set; } = null!;
    [Required] public DateTime FechaNacimiento { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
}
