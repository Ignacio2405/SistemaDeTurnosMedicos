namespace SistemaSaludGoya.Models;

public class Usuario
{
    public int IdUsuario { get; set; }

    public string Nombre { get; set; } = null!;

    public string Apellido { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // Navegación
    public Paciente? Paciente { get; set; }

    public Medico? Medico { get; set; }

    public ICollection<UsuarioRol> UsuarioRoles { get; set; }
        = new List<UsuarioRol>();
}
