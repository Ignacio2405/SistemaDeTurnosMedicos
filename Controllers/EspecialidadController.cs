using Microsoft.AspNetCore.Mvc;
using SistemaSaludGoya.Services;
using SistemaSaludGoya.Models;

namespace SistemaSaludGoya.Controllers
{
    public class EspecialidadController : Controller
    {
        private readonly IEspecialidadService _especialidadService;

                public EspecialidadController(IEspecialidadService especialidadService)
        {
            _especialidadService = especialidadService;
        }

                public async Task<IActionResult> Index()
        {
       
            var especialidades = await _especialidadService.ObtenerTodasAsync();

            return View(especialidades);
        }

        public async Task<IActionResult> Details(int id)
        {
           
            var especialidad = await _especialidadService.ObtenerPorIdAsync(id);

            if (especialidad == null)
            {
                return NotFound();
            }

            return View(especialidad);
        }
    }
}
