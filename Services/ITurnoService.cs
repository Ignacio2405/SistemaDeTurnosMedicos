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

        Task<List<Especialidad>> ObtenerEspecialidadesAsync();
        Task<List<Turno>> ObtenerTurnosPacienteAsync(int idPaciente);
        Task<(bool Ok, string Mensaje)> CrearTurnoAsync(int idMedico, int idPaciente, DateTime fecha, TimeSpan hora, string motivo);
        Task<(bool Ok, string Mensaje)> CancelarTurnoPacienteAsync(int idTurno, int idPaciente, string motivo);

        Task<object> ObtenerMedicosPorEspecialidadAsync(int idEspecialidad);
        Task<object> ObtenerHorariosOcupadosAsync(int medicoId, string fecha);
        Task<List<int>> ObtenerDiasHabilesAsync(int medicoId);
    }
}