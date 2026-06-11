using SistemaSaludGoya.Models;

namespace SistemaSaludGoya.Services
{
    public interface IRecetaService
    {
        Task<List<Receta>> ObtenerTodasAsync();
        Task<Receta?> ObtenerPorIdAsync(int id);
        Task CrearAsync(Receta receta);
        Task ActualizarAsync(Receta receta);
        Task EliminarAsync(int id);
    }
}
