using Microsoft.EntityFrameworkCore;
using SistemaSaludGoya.Data;
using SistemaSaludGoya.Models;

namespace SistemaSaludGoya.Services
{
    public class PagoService : IPagoService
    {
        private readonly AppDbContext _context;
        public PagoService(AppDbContext context) => _context = context;

        public async Task<List<Pago>> ObtenerTodosAsync()
        {
            return await _context.Pagos
                .Include(p => p.Turno)
                    .ThenInclude(t => t.Paciente)
                        .ThenInclude(p => p.Usuario)
                .ToListAsync();
        }

        public async Task<Pago?> ObtenerPorIdAsync(int id)
        {
            return await _context.Pagos
                .Include(p => p.Turno).ThenInclude(t => t.Paciente).ThenInclude(p => p.Usuario)
                .FirstOrDefaultAsync(x => x.IdPago == id);
        }

        public async Task RegistrarPagoAsync(Pago pago)
        {
            _context.Pagos.Add(pago);
            await _context.SaveChangesAsync();
        }
    }
}
