using Microsoft.AspNetCore.Mvc;
using SistemaSaludGoya.Services;

public class HorarioController : Controller
{
    private readonly IHorarioService _service;
    public HorarioController(IHorarioService service) => _service = service;

    // GET: Horario/Index/5 (donde 5 es el id del médico)
    public async Task<IActionResult> Index(int idMedico)
    {
        ViewBag.IdMedico = idMedico;
        return View(await _service.ObtenerPorMedicoAsync(idMedico));
    }
}
