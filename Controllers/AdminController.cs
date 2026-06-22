using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaSaludGoya.Services;
using SistemaSaludGoya.ViewModel;

namespace SistemaSaludGoya.Controllers
{
    [Authorize(Policy = "ModuloUsuarios")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var vm = await _adminService.ObtenerDashboardAsync();
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> AprobarUsuario(int idUsuario)
        {
            var vm = await _adminService.ObtenerUsuarioParaAprobarAsync(idUsuario);
            if (vm == null) return RedirectToAction("Dashboard");
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> AprobarUsuario(AprobarMedicoVM model)
        {
            if (!ModelState.IsValid && model.RolAsignar == "Medico")
            {
                model.Especialidades = await _adminService.ObtenerEspecialidadesAsync();
                return View(model);
            }

            var resultado = await _adminService.AprobarUsuarioAsync(model);

            if (!resultado.Ok)
            {
                TempData["Error"] = resultado.Mensaje;
                return RedirectToAction("Dashboard");
            }

            TempData["Success"] = resultado.Mensaje;
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> DesactivarUsuario(int idUsuario)
        {
            var resultado = await _adminService.CambiarEstadoUsuarioAsync(idUsuario);
            TempData["Success"] = resultado.Mensaje;
            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public async Task<IActionResult> EditarUsuario(int idUsuario)
        {
            var vm = await _adminService.ObtenerUsuarioParaEditarAsync(idUsuario);
            if (vm == null) return RedirectToAction("Dashboard");
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> EditarUsuario(EditarUsuarioVM model)
        {
            if (string.IsNullOrEmpty(model.NuevaPassword))
                ModelState.Remove(nameof(model.NuevaPassword));

            if (!ModelState.IsValid)
            {
                model.Especialidades = await _adminService.ObtenerEspecialidadesAsync();
                return View(model);
            }

            var resultado = await _adminService.EditarUsuarioAsync(model);

            if (!resultado.Ok)
            {
                ModelState.AddModelError("Email", resultado.Mensaje);
                model.Especialidades = await _adminService.ObtenerEspecialidadesAsync();
                return View(model);
            }

            TempData["Success"] = resultado.Mensaje;
            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public async Task<IActionResult> AnadirPersonal()
        {
            var vm = await _adminService.ObtenerDatosAltaPersonalAsync();
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> AnadirPersonal(AnadirPersonalVM model)
        {
            if (model.RolAsignar == "Medico" && string.IsNullOrWhiteSpace(model.Matricula))
                ModelState.AddModelError("Matricula", "La matrícula es obligatoria para médicos.");

            if (!ModelState.IsValid)
            {
                model.Especialidades = await _adminService.ObtenerEspecialidadesAsync();
                return View(model);
            }

            var resultado = await _adminService.AnadirPersonalAsync(model);

            if (!resultado.Ok)
            {
                ModelState.AddModelError("Email", resultado.Mensaje);
                model.Especialidades = await _adminService.ObtenerEspecialidadesAsync();
                return View(model);
            }

            TempData["Success"] = resultado.Mensaje;
            return RedirectToAction("Dashboard");
        }
    }
}