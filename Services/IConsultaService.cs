using SistemaSaludGoya.Models;

namespace SistemaSaludGoya.Services
{
    public interface IConsultaService
    {
        Task<List<Consulta>> ObtenerTodasAsync();
        Task<Consulta?> ObtenerPorIdAsync(int id);
        Task CrearAsync(Consulta consulta);
        Task ActualizarAsync(Consulta consulta);
        Task EliminarAsync(int id);
    }
}
