using SistemaSaludGoya.ViewModel;

namespace SistemaSaludGoya.Services
{
    public interface IAdminService
    {
        Task<AdminDashboardVM> ObtenerDashboardAsync();

        Task<AprobarMedicoVM?> ObtenerUsuarioParaAprobarAsync(int idUsuario);

        Task<(bool Ok, string Mensaje)> AprobarUsuarioAsync(AprobarMedicoVM model);

        Task<(bool Ok, string Mensaje)> CambiarEstadoUsuarioAsync(int idUsuario);

        Task<EditarUsuarioVM?> ObtenerUsuarioParaEditarAsync(int idUsuario);

        Task<(bool Ok, string Mensaje)> EditarUsuarioAsync(EditarUsuarioVM model);

        Task<AnadirPersonalVM> ObtenerDatosAltaPersonalAsync();

        Task<(bool Ok, string Mensaje)> AnadirPersonalAsync(AnadirPersonalVM model);

        Task<List<EspecialidadCheckVM>> ObtenerEspecialidadesAsync();
    }
}
