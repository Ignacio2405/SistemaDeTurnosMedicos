using SistemaSaludGoya.Models;

namespace SistemaSaludGoya.Services
{
    public interface ITurnoService
    {
        Task<List<Turno>> ObtenerTodosAsync();
        Task<Turno?> ObtenerPorIdAsync(int id);
        Task CrearAsync(Turno turno);
        Task ActualizarAsync(Turno turno);
        Task EliminarAsync(int id);
    }
}
