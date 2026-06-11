using System.ComponentModel.DataAnnotations;

namespace SistemaSaludGoya.ViewModel;

public class RegisterMedicoVM
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    public string Nombre { get; set; } = null!;

    [Required(ErrorMessage = "El apellido es obligatorio")]
    public string Apellido { get; set; } = null!;

    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [MinLength(6, ErrorMessage = "Mínimo 6 caracteres")]
    public string Password { get; set; } = null!;

    public string? Telefono { get; set; }

    // Matrícula opcional al registrarse; el admin la puede asignar/confirmar
    public string? Matricula { get; set; }
}
