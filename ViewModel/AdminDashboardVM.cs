namespace SistemaSaludGoya.ViewModel;

public class AdminDashboardVM
{
    public int TotalPacientes { get; set; }
    public int TotalMedicos { get; set; }
    public int TotalRecepcionistas { get; set; }
    public int TurnosHoy { get; set; }
    public int TurnosMes { get; set; }
    public int UsuariosSinRol { get; set; }
    public List<UsuarioListaVM> UsuariosPendientes { get; set; } = new();
    public List<UsuarioListaVM> TodosUsuarios { get; set; } = new();
}

public class UsuarioListaVM
{
    public int IdUsuario { get; set; }
    public string NombreCompleto { get; set; } = "";
    public string Email { get; set; } = "";
    public string Rol { get; set; } = "Sin rol";
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string? Matricula { get; set; }
}
