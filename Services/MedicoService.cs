using Microsoft.EntityFrameworkCore;
using SistemaSaludGoya.Data; // Acá suele estar el AppDbContext
using SistemaSaludGoya.Models;

namespace SistemaSaludGoya.Services
{
    public class MedicoService : IMedicoService
    {
        private readonly AppDbContext _context;

        // El constructor recibe el contexto automáticamente por Inyección de Dependencias
        public MedicoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Medico>> ObtenerTodosAsync()
        {
            return await _context.Medicos
                         .Include(m => m.Usuario)
                         .ToListAsync();
        }

        public async Task<Medico?> ObtenerPorIdAsync(int id)
        {
            return await _context.Medicos.FirstOrDefaultAsync(x => x.IdMedico == id);
        }

        public async Task CrearAsync(Medico medico)
        {
            _context.Medicos.Add(medico);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Medico medico)
        {
            _context.Medicos.Update(medico);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id)
        {
            var medico = await _context.Medicos.FindAsync(id);
            if (medico == null) return;

            _context.Medicos.Remove(medico);
            await _context.SaveChangesAsync();
        }
    }
}
