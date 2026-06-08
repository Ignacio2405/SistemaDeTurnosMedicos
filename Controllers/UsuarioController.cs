using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaSaludGoya.Models;
using SistemaSaludGoya.Services;

namespace SistemaSaludGoya.Controllers
{
    [Authorize]
    public class UsuarioController : Controller
    {
        private readonly IUsuarioService _usuarioService;

        public UsuarioController(
            IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        public async Task<IActionResult> Index()
        {
            var usuarios =
                await _usuarioService.ObtenerTodosAsync();

            return View(usuarios);
        }

        public async Task<IActionResult> Details(int id)
        {
            var usuario =
                await _usuarioService.ObtenerPorIdAsync(id);

            if (usuario == null)
                return NotFound();

            return View(usuario);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var usuario =
                await _usuarioService.ObtenerPorIdAsync(id);

            if (usuario == null)
                return NotFound();

            return View(usuario);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Usuario usuario)
        {
            if (!ModelState.IsValid)
                return View(usuario);

            await _usuarioService.ActualizarAsync(usuario);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var usuario =
                await _usuarioService.ObtenerPorIdAsync(id);

            if (usuario == null)
                return NotFound();

            return View(usuario);
        }

        [HttpPost]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _usuarioService.EliminarAsync(id);

            return RedirectToAction(nameof(Index));
        }
    }
}
