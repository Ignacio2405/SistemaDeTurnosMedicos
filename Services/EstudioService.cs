using Microsoft.EntityFrameworkCore;
using SistemaSaludGoya.Data;
using SistemaSaludGoya.Models;

namespace SistemaSaludGoya.Services
{
    public class EstudioService : IEstudioService
    {
        private readonly AppDbContext _context;
        public EstudioService(AppDbContext context) => _context = context;

        public async Task<List<Estudio>> ObtenerTodosAsync()
        {
            return await _context.Estudios
                .Include(e => e.Consulta).ThenInclude(c => c.Turno).ThenInclude(t => t.Paciente).ThenInclude(p => p.Usuario)
                .Include(e => e.Consulta).ThenInclude(c => c.Turno).ThenInclude(t => t.Medico).ThenInclude(m => m.Usuario)
                .ToListAsync();
        }

        public async Task<Estudio?> ObtenerPorIdAsync(int id)
        {
            return await _context.Estudios
                .Include(e => e.Consulta).ThenInclude(c => c.Turno).ThenInclude(t => t.Paciente).ThenInclude(p => p.Usuario)
                .FirstOrDefaultAsync(x => x.IdEstudio == id);
        }

        public async Task CrearAsync(Estudio estudio) { _context.Estudios.Add(estudio); await _context.SaveChangesAsync(); }
        public async Task ActualizarAsync(Estudio estudio) { _context.Estudios.Update(estudio); await _context.SaveChangesAsync(); }
        public async Task EliminarAsync(int id)
        {
            var estudio = await _context.Estudios.FindAsync(id);
            if (estudio != null) { _context.Estudios.Remove(estudio); await _context.SaveChangesAsync(); }
        }
    }
}
