using SistemaSaludGoya.Models;

namespace SistemaSaludGoya.Services;

public interface IUsuarioService
{
    
    Task<List<Usuario>> ObtenerTodosAsync();

    Task<Usuario?> ObtenerPorIdAsync(int id);

    Task ActualizarAsync(Usuario usuario);

    Task EliminarAsync(int id);
}
