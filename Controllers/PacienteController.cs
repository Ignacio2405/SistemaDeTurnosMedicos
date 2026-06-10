using Microsoft.AspNetCore.Mvc;
using SistemaSaludGoya.Services;
using SistemaSaludGoya.Models;

namespace SistemaSaludGoya.Controllers
{
    public class PacienteController : Controller
    {
        private readonly IPacienteService _pacienteService;

        // Inyección del servicio
        public PacienteController(IPacienteService pacienteService)
        {
            _pacienteService = pacienteService;
        }

        // GET: /Paciente
        public async Task<IActionResult> Index()
        {
            var pacientes = await _pacienteService.ObtenerTodosAsync();
            return View(pacientes);
        }

        // GET: /Paciente/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var paciente = await _pacienteService.ObtenerPorIdAsync(id);
            if (paciente == null) return NotFound();

            return View(paciente);
        }
    }
}
