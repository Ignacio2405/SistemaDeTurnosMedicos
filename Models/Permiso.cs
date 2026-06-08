namespace SistemaSaludGoya.Models;

public class Permiso
{
    public int IdPermiso { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public ICollection<RolPermiso> RolPermisos { get; set; }
        = new List<RolPermiso>();
}
