using SistemaSaludGoya.Models;

namespace SistemaSaludGoya.Services
{
    public interface IPacienteService
    {
        Task<List<Paciente>> ObtenerTodosAsync();
        Task<Paciente?> ObtenerPorIdAsync(int id);
        Task CrearAsync(Paciente paciente);
        Task ActualizarAsync(Paciente paciente);
        Task EliminarAsync(int id);
    }
}
