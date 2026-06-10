using Microsoft.AspNetCore.Mvc;
using SistemaSaludGoya.Services;
using SistemaSaludGoya.Models;

namespace SistemaSaludGoya.Controllers
{
    public class TurnoController : Controller
    {
        private readonly ITurnoService _turnoService;

        public TurnoController(ITurnoService turnoService)
        {
            _turnoService = turnoService;
        }

        // GET: /Turno
        public async Task<IActionResult> Index()
        {
            var turnos = await _turnoService.ObtenerTodosAsync();
            return View(turnos);
        }

        // GET: /Turno/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var turno = await _turnoService.ObtenerPorIdAsync(id);
            if (turno == null) return NotFound();

            return View(turno);
        }
    }
}
