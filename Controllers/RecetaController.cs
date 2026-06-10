using Microsoft.AspNetCore.Mvc;
using SistemaSaludGoya.Services;
using SistemaSaludGoya.Models;

namespace SistemaSaludGoya.Controllers
{
    public class RecetaController : Controller
    {
        private readonly IRecetaService _recetaService;

        public RecetaController(IRecetaService recetaService)
        {
            _recetaService = recetaService;
        }

        // GET: /Receta
        public async Task<IActionResult> Index()
        {
            var recetas = await _recetaService.ObtenerTodasAsync();
            return View(recetas);
        }

        // GET: /Receta/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var receta = await _recetaService.ObtenerPorIdAsync(id);
            if (receta == null) return NotFound();

            return View(receta);
        }
    }
}
