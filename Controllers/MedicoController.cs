using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaSaludGoya.Services;
using SistemaSaludGoya.ViewModel;
using System.Security.Claims;

namespace SistemaSaludGoya.Controllers
{
    [Authorize]
    public class MedicoController : Controller
    {
        private readonly IMedicoService _medicoService;

        public MedicoController(IMedicoService medicoService)
        {
            _medicoService = medicoService;
        }

        private async Task<int?> GetIdMedico()
        {
            var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            int? medicoIdQuery = null;

            bool puedeVerCualquierAgenda = User.HasClaim("Permiso", "turnos.gestionar.all");

            if (puedeVerCualquierAgenda &&
                Request.Query.TryGetValue("medicoId", out var valor) &&
                int.TryParse(valor, out int medicoId))
            {
                medicoIdQuery = medicoId;
            }

            return await _medicoService.GetIdMedicoAsync(idUsuario, puedeVerCualquierAgenda, medicoIdQuery);
        }

        [Authorize(Policy = "ModuloAgendaMedica")]
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var idMedico = await GetIdMedico();

            if (idMedico == null) return RedirectToAction("Login", "Auth");

            bool esSuPropiaAgenda = User.HasClaim("Permiso", "turnos.ver.propio");

            var vm = await _medicoService.ObtenerDashboardAsync(idMedico.Value, esSuPropiaAgenda);

            if (vm == null) return RedirectToAction("Login", "Auth");

            return View(vm);
        }

        [Authorize(Policy = "ModuloAgendaMedica")]
        [HttpPost]
        public async Task<IActionResult> ConfirmarTurno(int id)
        {
            var idMedico = await GetIdMedico();
            if (idMedico == null) return RedirectToAction("Dashboard");

            var resultado = await _medicoService.ConfirmarTurnoAsync(id, idMedico.Value);

            if (resultado) TempData["Success"] = "Turno confirmado correctamente.";

            return RedirectToAction("Dashboard");
        }

        [Authorize(Policy = "ModuloAgendaMedica")]
        [HttpPost]
        public async Task<IActionResult> CancelarTurno(int id, string motivo)
        {
            var idMedico = await GetIdMedico();
            if (idMedico == null) return RedirectToAction("Dashboard");

            var resultado = await _medicoService.CancelarTurnoAsync(id, idMedico.Value, motivo);

            if (resultado.Ok) TempData["Success"] = resultado.Mensaje;
            else TempData["Error"] = resultado.Mensaje;

            return RedirectToAction("Dashboard");
        }

        [Authorize(Policy = "ModuloConsultas")]
        [HttpGet]
        public async Task<IActionResult> AtenderTurno(int id)
        {
            var idMedico = await GetIdMedico();
            if (idMedico == null) return RedirectToAction("Dashboard");

            var vm = await _medicoService.ObtenerTurnoParaAtenderAsync(id, idMedico.Value);

            if (vm == null) return RedirectToAction("Dashboard");

            return View(vm);
        }

        [Authorize(Policy = "ModuloConsultas")]
        [HttpPost]
        public async Task<IActionResult> GuardarConsulta(ConsultaVM model)
        {
            if (!ModelState.IsValid) return View("AtenderTurno", model);

            var idMedico = await GetIdMedico();
            if (idMedico == null) return RedirectToAction("Dashboard");

            var resultado = await _medicoService.GuardarConsultaAsync(model, idMedico.Value);

            if (resultado) TempData["Success"] = "Consulta guardada correctamente.";

            return RedirectToAction("Dashboard");
        }

        [Authorize(Policy = "ModuloHistorial")]
        [HttpGet]
        public async Task<IActionResult> HistorialPaciente(int idPaciente)
        {
            var vm = await _medicoService.ObtenerHistorialPacienteAsync(idPaciente);

            if (vm == null) return RedirectToAction("Dashboard");

            return View(vm);
        }
    }
}