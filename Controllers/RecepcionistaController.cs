using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaSaludGoya.Services;

namespace SistemaSaludGoya.Controllers
{
    [Authorize(Roles = "Recepcionista,Administrador")]
    public class RecepcionistaController : Controller
    {
        private readonly IRecepcionistaService _recepcionistaService;

        public RecepcionistaController(IRecepcionistaService recepcionistaService)
        {
            _recepcionistaService = recepcionistaService;
        }

        public async Task<IActionResult> Dashboard(DateTime? fecha, string? estado)
        {
            var vm = await _recepcionistaService.ObtenerDashboardAsync(fecha, estado);
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

            var resultado = await _recepcionistaService.CancelarTurnoAsync(id, motivo);

            if (!resultado.Ok) TempData["Error"] = resultado.Mensaje;
            else TempData["Success"] = resultado.Mensaje;

            return RedirectToAction("Dashboard", new { fecha });
        }
    }
}