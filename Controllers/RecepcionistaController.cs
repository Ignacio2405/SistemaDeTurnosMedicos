using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaSaludGoya.Data;
using SistemaSaludGoya.Models;
using SistemaSaludGoya.ViewModel;

namespace SistemaSaludGoya.Controllers
{
    [Authorize(Roles = "Recepcionista,Administrador")]
    public class RecepcionistaController : Controller
    {
        private readonly AppDbContext _context;
        public RecepcionistaController(AppDbContext context) { _context = context; }

        public async Task<IActionResult> Dashboard(DateTime? fecha, string? estado)
        {
            var fechaBusqueda = fecha ?? DateTime.Today;

            var query = _context.Turnos
                .Include(t => t.Paciente).ThenInclude(p => p.Usuario)
                .Include(t => t.Medico).ThenInclude(m => m.Usuario)
                .Where(t => t.FechaHora.Date == fechaBusqueda.Date);

            if (!string.IsNullOrEmpty(estado) && Enum.TryParse<EstadoTurno>(estado, out var estadoEnum))
                query = query.Where(t => t.Estado == estadoEnum);

            var turnos = await query.OrderBy(t => t.FechaHora).ToListAsync();

            var vm = new RecepcionistaDashboardVM
            {
                FechaFiltro  = fechaBusqueda,
                FiltroEstado = estado,
                Turnos = turnos.Select(t => new TurnoAdminVM
                {
                    IdTurno        = t.IdTurno,
                    FechaHora      = t.FechaHora,
                    PacienteNombre = $"{t.Paciente.Usuario.Nombre} {t.Paciente.Usuario.Apellido}",
                    MedicoNombre   = $"Dr/a. {t.Medico.Usuario.Nombre} {t.Medico.Usuario.Apellido}",
                    Estado         = t.Estado.ToString(),
                    Motivo         = t.MotivoConsulta
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Cancelar(int id, string motivo, DateTime fecha)
        {
            if (string.IsNullOrWhiteSpace(motivo))
            {
                TempData["Error"] = "Debés indicar el motivo de cancelación.";
                return RedirectToAction("Dashboard", new { fecha });
            }

            var turno = await _context.Turnos.FirstOrDefaultAsync(t => t.IdTurno == id);
            if (turno == null) return RedirectToAction("Dashboard");

            turno.Estado         = EstadoTurno.Cancelado;
            turno.MotivoConsulta = $"[CANCELADO POR RECEPCIÓN] {motivo}";
            await _context.SaveChangesAsync();
            TempData["Success"] = "Turno cancelado correctamente.";
            return RedirectToAction("Dashboard", new { fecha });
        }
    }
}
