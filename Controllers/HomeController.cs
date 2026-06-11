using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaSaludGoya.Data;
using SistemaSaludGoya.Models;
using SistemaSaludGoya.ViewModel;

namespace SistemaSaludGoya.Controllers
{
    [Authorize(Roles = "Paciente")]
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        public HomeController(AppDbContext context) { _context = context; }

        public async Task<IActionResult> Index()
        {
            var pacienteClaim = User.Claims.FirstOrDefault(c => c.Type == "IdPaciente");
            var usuarioClaim  = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);

            if (pacienteClaim == null || !int.TryParse(pacienteClaim.Value, out int idPaciente) || idPaciente <= 0 ||
                usuarioClaim  == null || !int.TryParse(usuarioClaim.Value,  out int idUsuario))
                return View(new HomeDashboardVM());

            var usuario = await _context.Usuarios.Include(u => u.Paciente).FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

            var turnos = await _context.Turnos
                .Where(t => t.IdPaciente == idPaciente)
                .Include(t => t.Medico).ThenInclude(m => m.Usuario)
                .ToListAsync();

            var proximo = turnos
                .Where(t => t.FechaHora > DateTime.Now && t.Estado != EstadoTurno.Cancelado && t.Estado != EstadoTurno.Atendido)
                .OrderBy(t => t.FechaHora).FirstOrDefault();

            var vm = new HomeDashboardVM
            {
                IdUsuario         = idUsuario,
                IdPaciente        = idPaciente,
                NombreCompleto    = $"{usuario?.Nombre} {usuario?.Apellido}",
                Email             = usuario?.Email ?? "",
                Telefono          = usuario?.Paciente?.Telefono,
                Direccion         = usuario?.Paciente?.Direccion,
                Dni               = usuario?.Paciente?.Dni,
                FechaNacimiento   = usuario?.Paciente?.FechaNacimiento,
                TurnosPendientes  = turnos.Count(t => t.Estado == EstadoTurno.Solicitado),
                TurnosConfirmados = turnos.Count(t => t.Estado == EstadoTurno.Confirmado),
                TurnosAtendidos   = turnos.Count(t => t.Estado == EstadoTurno.Atendido),
                TurnosCancelados  = turnos.Count(t => t.Estado == EstadoTurno.Cancelado),
                ProximoTurno        = proximo?.FechaHora,
                ProximoTurnoMedico  = proximo?.Medico?.Usuario != null ? $"Dr/a. {proximo.Medico.Usuario.Nombre} {proximo.Medico.Usuario.Apellido}" : null
            };

            return View(vm);
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
