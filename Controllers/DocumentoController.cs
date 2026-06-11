using Microsoft.AspNetCore.Mvc;
using SistemaSaludGoya.Services;
using SistemaSaludGoya.Models;

namespace SistemaSaludGoya.Controllers
{
    public class DocumentoController : Controller
    {
        private readonly IDocumentoService _docService;

        public DocumentoController(IDocumentoService docService)
        {
            _docService = docService;
        }

        // GET: Documento/Index/5 (donde 5 es el IdConsulta)
        public async Task<IActionResult> Index(int idConsulta)
        {
            ViewBag.IdConsulta = idConsulta;
            var documentos = await _docService.ObtenerTodosPorConsultaAsync(idConsulta);
            return View(documentos);
        }

        // Aquí iría el método POST para subir el archivo (IFormFile)
        // Por ahora dejamos la estructura base
    }
}
