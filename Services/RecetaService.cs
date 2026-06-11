using Microsoft.EntityFrameworkCore;
using SistemaSaludGoya.Data;
using SistemaSaludGoya.Models;

namespace SistemaSaludGoya.Services
{
    public class RecetaService : IRecetaService
    {
        private readonly AppDbContext _context;

        public RecetaService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Receta>> ObtenerTodasAsync()
        {
            return await _context.Recetas
                .Include(r => r.Consulta)
                    .ThenInclude(c => c.Turno)
                        .ThenInclude(t => t.Paciente)
                            .ThenInclude(p => p.Usuario)
                .Include(r => r.Consulta)
                    .ThenInclude(c => c.Turno)
                        .ThenInclude(t => t.Medico)
                            .ThenInclude(m => m.Usuario)
                .ToListAsync();
        }

        public async Task<Receta?> ObtenerPorIdAsync(int id)
        {
            return await _context.Recetas
                .Include(r => r.Consulta).ThenInclude(c => c.Turno).ThenInclude(t => t.Paciente).ThenInclude(p => p.Usuario)
                .Include(r => r.Consulta).ThenInclude(c => c.Turno).ThenInclude(t => t.Medico).ThenInclude(m => m.Usuario)
                .FirstOrDefaultAsync(x => x.IdReceta == id);
        }

        public async Task CrearAsync(Receta receta)
        {
            _context.Recetas.Add(receta);
            await _context.SaveChangesAsync();
        }

            public async Task ActualizarAsync(Receta receta)
        {
            _context.Recetas.Update(receta);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id)
        {
            var receta = await _context.Recetas.FindAsync(id);
            if (receta == null) return;

            _context.Recetas.Remove(receta);
            await _context.SaveChangesAsync();
        }
    }
}
