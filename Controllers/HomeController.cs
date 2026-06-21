using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaSaludGoya.Models;
using SistemaSaludGoya.Services;
using SistemaSaludGoya.ViewModel;

namespace SistemaSaludGoya.Controllers
{
    [Authorize(Roles = "Paciente")]
    public class HomeController : Controller
    {
        private readonly IHomeService _homeService;

        public HomeController(IHomeService homeService) { _homeService = homeService; }

        public async Task<IActionResult> Index()
        {
            var pacienteClaim = User.Claims.FirstOrDefault(c => c.Type == "IdPaciente");
            var usuarioClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);

            if (pacienteClaim == null || !int.TryParse(pacienteClaim.Value, out int idPaciente) || idPaciente <= 0 ||
                usuarioClaim == null || !int.TryParse(usuarioClaim.Value, out int idUsuario))
                return View(new HomeDashboardVM());

            var vm = await _homeService.ObtenerDashboardPacienteAsync(idUsuario, idPaciente);
            return View(vm);
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}