using SistemaSaludGoya.Models;

namespace SistemaSaludGoya.Services
{
    public interface IHorarioService
    {
        Task<List<HorarioAtencion>> ObtenerPorMedicoAsync(int idMedico);
        Task CrearAsync(HorarioAtencion horario);
        Task EliminarAsync(int id);
    }
}
