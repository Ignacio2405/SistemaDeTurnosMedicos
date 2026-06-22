using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaSaludGoya.Services;
using SistemaSaludGoya.Models;

namespace SistemaSaludGoya.Controllers
{
    [Authorize]
    public class TurnoController : Controller
    {
        private readonly ITurnoService _turnoService;

        public TurnoController(ITurnoService turnoService) { _turnoService = turnoService; }

        [Authorize(Roles = "Paciente")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var pacienteClaim = User.Claims.FirstOrDefault(c => c.Type == "IdPaciente");
            if (pacienteClaim == null || !int.TryParse(pacienteClaim.Value, out int idPaciente) || idPaciente <= 0)
                return View(new List<Turno>());

            var turnos = await _turnoService.ObtenerTurnosPacienteAsync(idPaciente);
            return View(turnos);
        }

        [Authorize(Roles = "Paciente")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Especialidades = await _turnoService.ObtenerEspecialidadesAsync();
            return View();
        }

        [Authorize(Roles = "Paciente")]
        [HttpPost]
        public async Task<IActionResult> Create(int MedicoId, DateTime Fecha, TimeSpan Hora, string Motivo)
        {
            var pacienteClaim = User.Claims.FirstOrDefault(c => c.Type == "IdPaciente");
            if (pacienteClaim == null || !int.TryParse(pacienteClaim.Value, out int idPaciente) || idPaciente <= 0)
            {
                ModelState.AddModelError("", "No se pudo identificar al paciente.");
                ViewBag.Especialidades = await _turnoService.ObtenerEspecialidadesAsync();
                return View();
            }

            var resultado = await _turnoService.CrearTurnoAsync(MedicoId, idPaciente, Fecha, Hora, Motivo);

            if (!resultado.Ok)
            {
                ModelState.AddModelError("", resultado.Mensaje);
                ViewBag.Especialidades = await _turnoService.ObtenerEspecialidadesAsync();
                return View();
            }

            TempData["Success"] = "Turno solicitado correctamente.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Paciente")]
        [HttpPost]
        public async Task<IActionResult> Cancelar(int id, string motivo)
        {
            var pacienteClaim = User.Claims.FirstOrDefault(c => c.Type == "IdPaciente");
            if (pacienteClaim == null || !int.TryParse(pacienteClaim.Value, out int idPaciente))
                return RedirectToAction("Index");

            var resultado = await _turnoService.CancelarTurnoPacienteAsync(id, idPaciente, motivo);

            if (resultado.Ok) TempData["Success"] = resultado.Mensaje;
            else TempData["Error"] = resultado.Mensaje;

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrador,Recepcionista")]
        [HttpGet]
        public async Task<IActionResult> Todos()
        {
            var turnos = await _turnoService.ObtenerTodosAsync();
            return View(turnos);
        }

        [Authorize(Roles = "Administrador,Recepcionista")]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var turno = await _turnoService.ObtenerPorIdAsync(id);
            if (turno == null) return NotFound();
            return View(turno);
        }

        [Authorize(Roles = "Paciente")]
        [HttpGet]
        public async Task<IActionResult> ObtenerMedicosPorEspecialidad(int idEspecialidad)
        {
            var medicos = await _turnoService.ObtenerMedicosPorEspecialidadAsync(idEspecialidad);
            return Json(medicos);
        }

        [Authorize(Roles = "Paciente")]
        [HttpGet]
        public async Task<IActionResult> ObtenerHorariosOcupados(int medicoId, string fecha)
        {
            var horarios = await _turnoService.ObtenerHorariosOcupadosAsync(medicoId, fecha);
            return Json(horarios);
        }

        [Authorize(Roles = "Paciente")]
        [HttpGet]
        public async Task<IActionResult> ObtenerDiasHabiles(int medicoId)
        {
            var dias = await _turnoService.ObtenerDiasHabilesAsync(medicoId);
            return Json(dias);
        }
    }
}