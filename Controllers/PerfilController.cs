using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaSaludGoya.Data;
using SistemaSaludGoya.Models;
using SistemaSaludGoya.ViewModel;
using System.Security.Claims;

namespace SistemaSaludGoya.Controllers
{
    [Authorize]
    public class PerfilController : Controller
    {
        private readonly AppDbContext _context;

        public PerfilController(AppDbContext context) { _context = context; }

        private static readonly (DayOfWeek Dia, string Nombre)[] DiasOrdenados =
        {
            (DayOfWeek.Monday,    "Lunes"),
            (DayOfWeek.Tuesday,   "Martes"),
            (DayOfWeek.Wednesday, "Miércoles"),
            (DayOfWeek.Thursday,  "Jueves"),
            (DayOfWeek.Friday,    "Viernes"),
            (DayOfWeek.Saturday,  "Sábado"),
            (DayOfWeek.Sunday,    "Domingo"),
        };

        [HttpGet]
        public async Task<IActionResult> Editar()
        {
            var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var rol = User.FindFirstValue(ClaimTypes.Role) ?? "";

            var usuario = await _context.Usuarios
                .Include(u => u.Paciente)
                .Include(u => u.Medico).ThenInclude(m => m!.Horarios)
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

            if (usuario == null) return RedirectToAction("Login", "Auth");

            var vm = new EditarPerfilVM
            {
                IdUsuario       = idUsuario,
                Rol             = rol,
                Nombre          = usuario.Nombre,
                Apellido        = usuario.Apellido,
                Email           = usuario.Email,
                Dni             = usuario.Paciente?.Dni,
                Telefono        = usuario.Paciente?.Telefono ?? usuario.Medico?.Telefono,
                Direccion       = usuario.Paciente?.Direccion,
                FechaNacimiento = usuario.Paciente?.FechaNacimiento,
                Matricula       = usuario.Medico?.Matricula,
                Horarios = DiasOrdenados.Select(d =>
                {
                    var h = usuario.Medico?.Horarios.FirstOrDefault(x => x.DiaSemana == d.Dia);
                    return new HorarioDiaVM
                    {
                        DiaSemana = d.Dia,
                        NombreDia = d.Nombre,
                        Activo    = h != null,
                        HoraDesde = h != null ? $"{h.HoraDesde.Hours:D2}:{h.HoraDesde.Minutes:D2}" : "08:00",
                        HoraHasta = h != null ? $"{h.HoraHasta.Hours:D2}:{h.HoraHasta.Minutes:D2}" : "13:00",
                    };
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> GuardarPerfil(EditarPerfilVM model)
        {
            if (string.IsNullOrEmpty(model.NuevaPassword))
                ModelState.Remove(nameof(model.NuevaPassword));

            if (!ModelState.IsValid) return View("Editar", model);

            var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var usuario = await _context.Usuarios
                .Include(u => u.Paciente)
                .Include(u => u.Medico)
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);
            if (usuario == null) return RedirectToAction("Login", "Auth");

            if (await _context.Usuarios.AnyAsync(u => u.Email == model.Email && u.IdUsuario != idUsuario))
            {
                ModelState.AddModelError("Email", "Ya existe otro usuario con ese email.");
                model.Rol = User.FindFirstValue(ClaimTypes.Role) ?? "";
                return View("Editar", model);
            }

            usuario.Nombre   = model.Nombre;
            usuario.Apellido = model.Apellido;
            usuario.Email    = model.Email;

            if (!string.IsNullOrWhiteSpace(model.NuevaPassword))
                usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NuevaPassword);

            if (usuario.Paciente != null)
            {
                usuario.Paciente.Telefono  = model.Telefono;
                usuario.Paciente.Direccion = model.Direccion;
                if (model.FechaNacimiento.HasValue)
                    usuario.Paciente.FechaNacimiento = model.FechaNacimiento.Value;
            }

            if (usuario.Medico != null)
            {
                if (!string.IsNullOrEmpty(model.Matricula))
                    usuario.Medico.Matricula = model.Matricula;
                usuario.Medico.Telefono = model.Telefono;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Perfil actualizado correctamente.";
            return RedirectToAction("Editar");
        }

        [HttpPost]
        [Authorize(Roles = "Medico")]
        public async Task<IActionResult> GuardarHorarios(
            List<bool>   diasActivos,
            List<string> horasDesde,
            List<string> horasHasta)
        {
            var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var medico = await _context.Medicos
                .Include(m => m.Horarios)
                .FirstOrDefaultAsync(m => m.IdUsuario == idUsuario);
            if (medico == null) return RedirectToAction("Editar");

            _context.HorariosAtencion.RemoveRange(medico.Horarios);
            await _context.SaveChangesAsync();

            for (int i = 0; i < DiasOrdenados.Length; i++)
            {
                if (i < diasActivos.Count && diasActivos[i])
                {
                    var desdeStr = i < horasDesde.Count ? horasDesde[i] : "08:00";
                    var hastaStr = i < horasHasta.Count ? horasHasta[i] : "13:00";
                    if (TimeSpan.TryParse(desdeStr, out var desde) &&
                        TimeSpan.TryParse(hastaStr, out var hasta) &&
                        desde < hasta)
                    {
                        _context.HorariosAtencion.Add(new HorarioAtencion
                        {
                            IdMedico  = medico.IdMedico,
                            DiaSemana = DiasOrdenados[i].Dia,
                            HoraDesde = desde,
                            HoraHasta = hasta
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Horarios actualizados correctamente.";
            return RedirectToAction("Editar");
        }
    }
}
