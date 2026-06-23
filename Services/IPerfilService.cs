using SistemaSaludGoya.ViewModel;

namespace SistemaSaludGoya.Services
{
    public interface IPerfilService
    {
        Task<EditarPerfilVM?> ObtenerPerfilAsync(int idUsuario, string rol);
        Task<(bool Ok, string Mensaje, bool EmailDuplicado)> GuardarPerfilAsync(int idUsuario, EditarPerfilVM model);
        Task<(bool Ok, string Mensaje)> GuardarHorariosAsync(int idUsuario, GuardarHorariosDTO request);
    }
}