using SistemaSaludGoya.Models;

namespace SistemaSaludGoya.Services
{
    public interface IEstudioService
    {
        Task<List<Estudio>> ObtenerTodosAsync();
        Task<Estudio?> ObtenerPorIdAsync(int id);
        Task CrearAsync(Estudio estudio);
        Task ActualizarAsync(Estudio estudio);
        Task EliminarAsync(int id);
    }
}
