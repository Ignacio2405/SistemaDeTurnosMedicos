using SistemaSaludGoya.Models;
using SistemaSaludGoya.ViewModel;

namespace SistemaSaludGoya.Services
{
    public interface IMedicoService
    {
        Task<int?> GetIdMedicoAsync( int idUsuario, bool esAdministrador, int? medicoIdQuery);

        Task<MedicoDashboardVM?> ObtenerDashboardAsync( int idMedico, bool esMedico);

        Task<bool> ConfirmarTurnoAsync( int idTurno, int idMedico);

        Task<(bool Ok, string Mensaje)> CancelarTurnoAsync( int idTurno, int idMedico, string motivo);

        Task<ConsultaVM?> ObtenerTurnoParaAtenderAsync( int idTurno, int idMedico);

        Task<bool> GuardarConsultaAsync( ConsultaVM model, int idMedico);

        Task<HistorialPacienteVM?> ObtenerHistorialPacienteAsync( int idPaciente);
    }
}
