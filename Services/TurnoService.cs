using Microsoft.EntityFrameworkCore;
using SistemaSaludGoya.Data;
using SistemaSaludGoya.Models;

namespace SistemaSaludGoya.Services
{
    public class TurnoService : ITurnoService
    {
        private readonly AppDbContext _context;

        public TurnoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Turno>> ObtenerTodosAsync()
        {
            return await _context.Turnos
                // Traemos el médico y su cuenta de usuario asociada
                .Include(t => t.Medico)
                    .ThenInclude(m => m.Usuario)
                // Traemos el paciente y su cuenta de usuario asociada
                .Include(t => t.Paciente)
                    .ThenInclude(p => p.Usuario)
                // Si tenés una entidad EstadoTurno en tu modelo, podés incluirla también:
                // .Include(t => t.EstadoTurno) 
                .ToListAsync();
        }

        public async Task<Turno?> ObtenerPorIdAsync(int id)
        {
            // Ajustá 'IdTurno' si en tu modelo la clave primaria se llama distinto (ej: 'Id')
            return await _context.Turnos
                .Include(t => t.Medico).ThenInclude(m => m.Usuario)
                .Include(t => t.Paciente).ThenInclude(p => p.Usuario)
                .FirstOrDefaultAsync(x => x.IdTurno == id);
        }

        public async Task CrearAsync(Turno turno)
        {
            _context.Turnos.Add(turno);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Turno turno)
        {
            _context.Turnos.Update(turno);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id)
        {
            var turno = await _context.Turnos.FindAsync(id);
            if (turno == null) return;

            _context.Turnos.Remove(turno);
            await _context.SaveChangesAsync();
        }
    }
}
