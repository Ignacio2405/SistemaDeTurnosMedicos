using Microsoft.AspNetCore.Mvc;
using SistemaSaludGoya.Services;
using SistemaSaludGoya.Models;

namespace SistemaSaludGoya.Controllers
{
    public class ConsultaController : Controller
    {
        private readonly IConsultaService _consultaService;

        public ConsultaController(IConsultaService consultaService)
        {
            _consultaService = consultaService;
        }

        // GET: /Consulta
        public async Task<IActionResult> Index()
        {
            var consultas = await _consultaService.ObtenerTodasAsync();
            return View(consultas);
        }

        // GET: /Consulta/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var consulta = await _consultaService.ObtenerPorIdAsync(id);
            if (consulta == null) return NotFound();

            return View(consulta);
        }
    }
}
