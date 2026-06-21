using Microsoft.EntityFrameworkCore;
using SistemaSaludGoya.Data;
using SistemaSaludGoya.Models;
using SistemaSaludGoya.ViewModel;

namespace SistemaSaludGoya.Services
{
    public class RecepcionistaService : IRecepcionistaService
    {
        private readonly AppDbContext _context;
        public RecepcionistaService(AppDbContext context) { _context = context; }

        public async Task<RecepcionistaDashboardVM> ObtenerDashboardAsync(DateTime? fecha, string? estado)
        {
            var fechaBusqueda = fecha ?? DateTime.Today;

            var query = _context.Turnos
                .Include(t => t.Paciente).ThenInclude(p => p.Usuario)
                .Include(t => t.Medico).ThenInclude(m => m.Usuario)
                .Where(t => t.FechaHora.Date == fechaBusqueda.Date);

            if (!string.IsNullOrEmpty(estado) && Enum.TryParse<EstadoTurno>(estado, out var estadoEnum))
                query = query.Where(t => t.Estado == estadoEnum);

            var turnos = await query.OrderBy(t => t.FechaHora).ToListAsync();

            return new RecepcionistaDashboardVM
            {
                FechaFiltro = fechaBusqueda,
                FiltroEstado = estado,
                Turnos = turnos.Select(t => new TurnoAdminVM
                {
                    IdTurno = t.IdTurno,
                    FechaHora = t.FechaHora,
                    PacienteNombre = $"{t.Paciente.Usuario.Nombre} {t.Paciente.Usuario.Apellido}",
                    MedicoNombre = $"Dr/a. {t.Medico.Usuario.Nombre} {t.Medico.Usuario.Apellido}",
                    Estado = t.Estado.ToString(),
                    Motivo = t.MotivoConsulta
                }).ToList()
            };
        }

        public async Task<(bool Ok, string Mensaje)> CancelarTurnoAsync(int idTurno, string motivo)
        {
            var turno = await _context.Turnos.FirstOrDefaultAsync(t => t.IdTurno == idTurno);
            if (turno == null) return (false, "Turno no encontrado.");

            turno.Estado = EstadoTurno.Cancelado;
            turno.MotivoConsulta = $"[CANCELADO POR RECEPCIÓN] {motivo}";
            await _context.SaveChangesAsync();

            return (true, "Turno cancelado correctamente.");
        }
    }
}