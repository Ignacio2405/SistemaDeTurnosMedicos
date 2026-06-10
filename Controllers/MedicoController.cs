using Microsoft.AspNetCore.Mvc;
using SistemaSaludGoya.Services;
using SistemaSaludGoya.Models;

namespace SistemaSaludGoya.Controllers
{
   
    public class MedicoController : Controller
    {
              private readonly IMedicoService _medicoService;
              
        public MedicoController(IMedicoService medicoService)
        {
            _medicoService = medicoService;
        }

      
        public async Task<IActionResult> Index()
        {
            var medicos = await _medicoService.ObtenerTodosAsync();

            return View(medicos);
        }

   
        public async Task<IActionResult> Details(int id)
        {
            var medico = await _medicoService.ObtenerPorIdAsync(id);

            
            if (medico == null)
            {
                return NotFound();
            }

      
            return View(medico);
        }
    }
}
