using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaSaludGoya.Data;
using SistemaSaludGoya.Models;

namespace SistemaSaludGoya.Controllers
{
    [Authorize(Roles = "Paciente")]
    public class TurnosController : Controller
    {
        private readonly AppDbContext _context;
        public TurnosController(AppDbContext context) { _context = context; }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Especialidades = _context.Especialidades.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult Create(int MedicoId, DateTime Fecha, TimeSpan Hora, string Motivo)
        {
            var pacienteClaim = User.Claims.FirstOrDefault(c => c.Type == "IdPaciente");
            if (pacienteClaim == null || !int.TryParse(pacienteClaim.Value, out int idPaciente) || idPaciente <= 0)
            {
                ModelState.AddModelError("", "No se pudo identificar al paciente.");
                ViewBag.Especialidades = _context.Especialidades.ToList();
                return View();
            }

            // Especificamos DateTimeKind.Local para que Npgsql sepa que es hora de Argentina (UTC-3)
            // y lo convierta correctamente a UTC al guardarlo en PostgreSQL.
            // Al leer, Npgsql convierte UTC → local Argentina, respetando el horario seleccionado.
            var fechaHoraLocal = DateTime.SpecifyKind(Fecha.Date.Add(Hora), DateTimeKind.Local);

            // Verificar si ya hay turno: comparamos en memoria para evitar problemas de timezone en SQL
            bool turnoOcupado = _context.Turnos
                .AsEnumerable()
                .Any(t =>
                    t.IdMedico == MedicoId &&
                    t.FechaHora == fechaHoraLocal &&
                    t.Estado != EstadoTurno.Cancelado &&
                    t.Estado != EstadoTurno.Atendido);

            if (turnoOcupado)
            {
                ModelState.AddModelError("", "El profesional ya tiene un turno en ese horario.");
                ViewBag.Especialidades = _context.Especialidades.ToList();
                return View();
            }

            _context.Turnos.Add(new Turno
            {
                IdMedico       = MedicoId,
                IdPaciente     = idPaciente,
                FechaHora      = fechaHoraLocal,
                MotivoConsulta = Motivo,
                Estado         = EstadoTurno.Solicitado,
                FechaCreacion  = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local)
            });
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Index()
        {
            var pacienteClaim = User.Claims.FirstOrDefault(c => c.Type == "IdPaciente");
            if (pacienteClaim == null || !int.TryParse(pacienteClaim.Value, out int idPaciente) || idPaciente <= 0)
                return View(new List<Turno>());

            var turnos = _context.Turnos
                .Where(t => t.IdPaciente == idPaciente)
                .Include(t => t.Medico).ThenInclude(m => m.Usuario)
                .OrderByDescending(t => t.FechaHora).ToList();

            return View(turnos);
        }

        [HttpPost]
        public IActionResult Cancelar(int id, string motivo)
        {
            var pacienteClaim = User.Claims.FirstOrDefault(c => c.Type == "IdPaciente");
            if (pacienteClaim == null || !int.TryParse(pacienteClaim.Value, out int idPaciente))
                return RedirectToAction("Index");

            var turno = _context.Turnos.FirstOrDefault(t => t.IdTurno == id && t.IdPaciente == idPaciente);
            if (turno == null) return RedirectToAction("Index");

            turno.Estado = EstadoTurno.Cancelado;
            turno.MotivoConsulta = $"[CANCELADO] {motivo}";
            _context.SaveChanges();
            TempData["Success"] = "Turno cancelado correctamente.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult ObtenerMedicosPorEspecialidad(int idEspecialidad)
        {
            var medicos = _context.MedicosEspecialidades
                .Where(me => me.IdEspecialidad == idEspecialidad)
                .Select(me => new { idMedico = me.IdMedico, nombre = me.Medico.Usuario.Nombre + " " + me.Medico.Usuario.Apellido })
                .ToList();
            return Json(medicos);
        }

        [HttpGet]
        public IActionResult ObtenerHorariosOcupados(int medicoId, string fecha)
        {
            try
            {
                if (!DateTime.TryParse(fecha, out DateTime fechaDate))
                    return Json(new { error = "Fecha inválida", slots = new List<object>() });

                var diaSemana = fechaDate.DayOfWeek;
                var todosHorarios = _context.HorariosAtencion.AsEnumerable()
                    .Where(h => h.IdMedico == medicoId && h.DiaSemana == diaSemana).ToList();

                var todosTurnos = _context.Turnos.AsEnumerable()
                    .Where(t => t.IdMedico == medicoId && t.FechaHora.Date == fechaDate.Date &&
                                t.Estado != EstadoTurno.Cancelado && t.Estado != EstadoTurno.Atendido)
                    .Select(t => t.FechaHora.ToString("HH:mm")).ToList();

                List<string> slots;
                if (todosHorarios.Any())
                {
                    slots = new List<string>();
                    foreach (var h in todosHorarios)
                    {
                        var actual = h.HoraDesde;
                        while (actual.Add(TimeSpan.FromMinutes(30)) <= h.HoraHasta)
                        {
                            slots.Add(actual.Hours.ToString("D2") + ":" + actual.Minutes.ToString("D2"));
                            actual = actual.Add(TimeSpan.FromMinutes(30));
                        }
                    }
                    slots = slots.Distinct().OrderBy(s => s).ToList();
                }
                else
                    slots = new List<string> { "08:00","08:30","09:00","09:30","10:00","10:30","11:00","11:30","12:00","14:00","14:30","15:00","15:30","16:00","16:30","17:00" };

                // Mapa de hora → estado para slots ocupados
                var turnosOcupados = _context.Turnos.AsEnumerable()
                    .Where(t => t.IdMedico == medicoId && t.FechaHora.Date == fechaDate.Date &&
                                t.Estado != EstadoTurno.Cancelado && t.Estado != EstadoTurno.Atendido)
                    .ToDictionary(t => t.FechaHora.ToString("HH:mm"), t => t.Estado.ToString());

                var resultado = slots.Select(s => new
                {
                    hora    = s,
                    ocupado = turnosOcupados.ContainsKey(s),
                    estado  = turnosOcupados.ContainsKey(s) ? turnosOcupados[s] : null
                }).ToList();
                return Json(new { disponible = true, slots = resultado });
            }
            catch (Exception ex) { return Json(new { error = ex.Message, slots = new List<object>() }); }
        }
        [HttpGet]
        public IActionResult ObtenerDiasHabiles(int medicoId)
        {
            var dias = _context.HorariosAtencion
                .Where(h => h.IdMedico == medicoId)
                .Select(h => (int)h.DiaSemana)
                .Distinct()
                .ToList();
            return Json(dias);
        }
    }
}
