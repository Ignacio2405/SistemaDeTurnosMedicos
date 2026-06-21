using Microsoft.EntityFrameworkCore;
using SistemaSaludGoya.Data;
using SistemaSaludGoya.ViewModel;
using SistemaSaludGoya.Models;

namespace SistemaSaludGoya.Services
{
    public interface IPerfilService
    {
        Task<EditarPerfilVM?> ObtenerPerfilAsync(int idUsuario, string rol);
        Task<(bool Ok, string Mensaje, bool EmailDuplicado)> GuardarPerfilAsync(int idUsuario, EditarPerfilVM model);
        Task<(bool Ok, string Mensaje)> GuardarHorariosAsync(int idUsuario, List<bool> diasActivos, List<string> horasDesde, List<string> horasHasta);
    }
}