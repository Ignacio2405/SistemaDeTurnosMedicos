using Microsoft.EntityFrameworkCore;
using SistemaSaludGoya.Data;
using SistemaSaludGoya.Models;

namespace SistemaSaludGoya.Services
{
    public class EspecialidadService : IEspecialidadService
    {
        private readonly AppDbContext _context;

        public EspecialidadService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Especialidad>> ObtenerTodasAsync()
        {
            // Trae todas las especialidades de la base de datos
            return await _context.Especialidades.ToListAsync();
        }

        public async Task<Especialidad?> ObtenerPorIdAsync(int id)
        {
           
            return await _context.Especialidades
                .FirstOrDefaultAsync(x => x.IdEspecialidad == id);
        }

        public async Task CrearAsync(Especialidad especialidad)
        {
            _context.Especialidades.Add(especialidad);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Especialidad especialidad)
        {
            _context.Especialidades.Update(especialidad);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id)
        {
            var especialidad = await _context.Especialidades.FindAsync(id);
            if (especialidad == null) return;

            _context.Especialidades.Remove(especialidad);
            await _context.SaveChangesAsync();
        }
    }
}
