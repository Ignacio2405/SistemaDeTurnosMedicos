using Microsoft.EntityFrameworkCore;
using SistemaSaludGoya.Data;
using SistemaSaludGoya.Models;

namespace SistemaSaludGoya.Services
{
    public class HorarioService : IHorarioService
    {
        private readonly AppDbContext _context;
        public HorarioService(AppDbContext context) => _context = context;

        public async Task<List<HorarioAtencion>> ObtenerPorMedicoAsync(int idMedico)
        {
            return await _context.HorariosAtencion
                .Where(h => h.IdMedico == idMedico)
                .OrderBy(h => h.DiaSemana)
                .ThenBy(h => h.HoraDesde)
                .ToListAsync();
        }

        public async Task CrearAsync(HorarioAtencion horario)
        {
            _context.HorariosAtencion.Add(horario);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id)
        {
            var horario = await _context.HorariosAtencion.FindAsync(id);
            if (horario != null) { _context.HorariosAtencion.Remove(horario); await _context.SaveChangesAsync(); }
        }
    }
}
