using Microsoft.EntityFrameworkCore;
using SistemaSaludGoya.Data;
using SistemaSaludGoya.Models;

namespace SistemaSaludGoya.Services
{
    public class ConsultaService : IConsultaService
    {
        private readonly AppDbContext _context;

        public ConsultaService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Consulta>> ObtenerTodasAsync()
        {
            return await _context.Consultas
                .Include(c => c.Turno)
                    .ThenInclude(t => t.Paciente)
                        .ThenInclude(p => p.Usuario)
                .Include(c => c.Turno)
                    .ThenInclude(t => t.Medico)
                        .ThenInclude(m => m.Usuario)
                .ToListAsync();
        }

        public async Task<Consulta?> ObtenerPorIdAsync(int id)
        {
            // Ajustá 'IdConsulta' si en tu modelo Consulta.cs la clave primaria se llama distinto (ej: 'Id')
            return await _context.Consultas
                .Include(c => c.Turno).ThenInclude(t => t.Paciente).ThenInclude(p => p.Usuario)
                .Include(c => c.Turno).ThenInclude(t => t.Medico).ThenInclude(m => m.Usuario)
                .FirstOrDefaultAsync(x => x.IdConsulta == id);
        }

        public async Task CrearAsync(Consulta consulta)
        {
            _context.Consultas.Add(consulta);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Consulta consulta)
        {
            _context.Consultas.Update(consulta);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id)
        {
            var consulta = await _context.Consultas.FindAsync(id);
            if (consulta == null) return;

            _context.Consultas.Remove(consulta);
            await _context.SaveChangesAsync();
        }
    }
}
