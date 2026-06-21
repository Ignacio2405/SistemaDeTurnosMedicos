using Microsoft.EntityFrameworkCore;
using SistemaSaludGoya.Data;
using SistemaSaludGoya.Models;
using SistemaSaludGoya.ViewModel;

namespace SistemaSaludGoya.Services
{
    public interface IRecepcionistaService
    {
        Task<RecepcionistaDashboardVM> ObtenerDashboardAsync(DateTime? fecha, string? estado);
        Task<(bool Ok, string Mensaje)> CancelarTurnoAsync(int idTurno, string motivo);
    }
}