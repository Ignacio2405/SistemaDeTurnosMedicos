using SistemaSaludGoya.Models;

namespace SistemaSaludGoya.Services
{
    public interface IPagoService
    {
        Task<List<Pago>> ObtenerTodosAsync();
        Task<Pago?> ObtenerPorIdAsync(int id);
        Task RegistrarPagoAsync(Pago pago); // Usamos 'Registrar' para mayor claridad
    }
}
