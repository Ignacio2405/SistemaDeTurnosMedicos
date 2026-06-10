using SistemaSaludGoya.Models;

namespace SistemaSaludGoya.Services
{
    public interface IEspecialidadService
    {
        Task<List<Especialidad>> ObtenerTodasAsync();
        Task<Especialidad?> ObtenerPorIdAsync(int id);
        Task CrearAsync(Especialidad especialidad);
        Task ActualizarAsync(Especialidad especialidad);
        Task EliminarAsync(int id);
    }
}
