using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaSaludGoya.Services;
using SistemaSaludGoya.ViewModel;
using System.Security.Claims;

namespace SistemaSaludGoya.Controllers
{
    [Authorize]
    public class PerfilController : Controller
    {
        private readonly IPerfilService _perfilService;

        public PerfilController(IPerfilService perfilService) { _perfilService = perfilService; }

        [HttpGet]
        public async Task<IActionResult> Editar()
        {
            var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var rol = User.FindFirstValue(ClaimTypes.Role) ?? "";

            var vm = await _perfilService.ObtenerPerfilAsync(idUsuario, rol);
            if (vm == null) return RedirectToAction("Login", "Auth");

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> GuardarPerfil(EditarPerfilVM model)
        {
            if (string.IsNullOrWhiteSpace(model.NuevaPassword))
            {
                model.NuevaPassword = null;
                ModelState.Remove(nameof(model.NuevaPassword));
            }

            if (User.FindFirstValue(ClaimTypes.Role) == "paciente")
            {
                ModelState.Remove("Matricula");
            }

            if (!ModelState.IsValid) return View("Editar", model);

            var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            var resultado = await _perfilService.GuardarPerfilAsync(idUsuario, model);

            if (!resultado.Ok)
            {
                if (resultado.EmailDuplicado)
                {
                    ModelState.AddModelError("Email", resultado.Mensaje);
                    model.Rol = User.FindFirstValue(ClaimTypes.Role) ?? "";
                    return View("Editar", model);
                }
                return RedirectToAction("Login", "Auth");
            }

            TempData["Success"] = resultado.Mensaje;
            return RedirectToAction("Editar");
        }

        [HttpPost]
        [Authorize(Policy = "ModuloHorarios")]
        public async Task<IActionResult> GuardarHorarios(List<bool> diasActivos, List<string> horasDesde, List<string> horasHasta)
        {
            var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            var resultado = await _perfilService.GuardarHorariosAsync(idUsuario, diasActivos, horasDesde, horasHasta);

            if (resultado.Ok) TempData["Success"] = resultado.Mensaje;
            else TempData["Error"] = resultado.Mensaje;

            return RedirectToAction("Editar");
        }
    }
}