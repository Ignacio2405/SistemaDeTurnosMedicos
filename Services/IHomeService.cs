using Microsoft.EntityFrameworkCore;
using SistemaSaludGoya.Data;
using SistemaSaludGoya.Models;
using SistemaSaludGoya.ViewModel;

namespace SistemaSaludGoya.Services
{
    public interface IHomeService
    {
        Task<HomeDashboardVM> ObtenerDashboardPacienteAsync(int idUsuario, int idPaciente);
    }
}