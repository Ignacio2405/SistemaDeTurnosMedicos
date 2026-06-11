using Microsoft.AspNetCore.Mvc;
using SistemaSaludGoya.Services;
using SistemaSaludGoya.Models;

namespace SistemaSaludGoya.Controllers
{
    public class PagoController : Controller
    {
    
        private readonly IPagoService _pagoService;

               public PagoController(IPagoService pagoService)
        {
            _pagoService = pagoService;
        }

          public async Task<IActionResult> Index()
        {
            var pagos = await _pagoService.ObtenerTodosAsync();
            return View(pagos);
        }

        // GET: /Pago/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var pago = await _pagoService.ObtenerPorIdAsync(id);
            if (pago == null) return NotFound();

            return View(pago);
        }
    }
}