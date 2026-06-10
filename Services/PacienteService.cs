using Microsoft.EntityFrameworkCore;
using SistemaSaludGoya.Data;
using SistemaSaludGoya.Models;

namespace SistemaSaludGoya.Services
{
    public class PacienteService : IPacienteService
    {
        private readonly AppDbContext _context;

        public PacienteService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Paciente>> ObtenerTodosAsync()
        {
            // Clave: Traemos al Usuario vinculado para tener Nombre y Apellido
            return await _context.Pacientes
                                 .Include(p => p.Usuario)
                                 .ToListAsync();
        }

        public async Task<Paciente?> ObtenerPorIdAsync(int id)
        {
            return await _context.Pacientes
                                 .Include(p => p.Usuario)
                                 .FirstOrDefaultAsync(x => x.IdPaciente == id);
        }

        public async Task CrearAsync(Paciente paciente)
        {
            _context.Pacientes.Add(paciente);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Paciente paciente) // Corregido el nombre según la guía: ActualizarAsync
        {
            _context.Pacientes.Update(paciente);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Paciente paciente)
        {
            _context.Pacientes.Update(paciente);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id)
        {
            var paciente = await _context.Pacientes.FindAsync(id);
            if (paciente == null) return;

            _context.Pacientes.Remove(paciente);
            await _context.SaveChangesAsync();
        }
    }
}
