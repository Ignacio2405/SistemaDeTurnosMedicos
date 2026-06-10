using Microsoft.AspNetCore.Mvc;
using SistemaSaludGoya.Services;

public class EstudioController : Controller
{
    private readonly IEstudioService _service;
    public EstudioController(IEstudioService service) => _service = service;

    public async Task<IActionResult> Index() => View(await _service.ObtenerTodosAsync());
    public async Task<IActionResult> Details(int id)
    {
        var estudio = await _service.ObtenerPorIdAsync(id);
        return estudio == null ? NotFound() : View(estudio);
    }
}
