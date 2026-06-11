using SistemaSaludGoya.Models;

namespace SistemaSaludGoya.Services
{
    public interface IMedicoService
    {
        Task<List<Medico>> ObtenerTodosAsync();

        Task<Medico?> ObtenerPorIdAsync(int id);

        Task CrearAsync(Medico medico);

        Task ActualizarAsync(Medico medico);

        Task EliminarAsync(int id);
    }
}
