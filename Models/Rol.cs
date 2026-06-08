namespace SistemaSaludGoya.Models;

public class Rol
{
    public int IdRol { get; set; }

    public string Nombre { get; set; } = null!;

    public ICollection<UsuarioRol> UsuarioRoles { get; set; }
        = new List<UsuarioRol>();

    public ICollection<RolPermiso> RolPermisos { get; set; }
        = new List<RolPermiso>();
}
