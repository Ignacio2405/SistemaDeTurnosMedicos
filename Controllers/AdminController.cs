using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaSaludGoya.Data;
using SistemaSaludGoya.Models;
using SistemaSaludGoya.ViewModel;

namespace SistemaSaludGoya.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        public AdminController(AppDbContext context) { _context = context; }

        public async Task<IActionResult> Dashboard()
        {
            var hoy   = DateTime.Today;
            var mes   = new DateTime(hoy.Year, hoy.Month, 1);

            var usuarios = await _context.Usuarios
                .Include(u => u.UsuarioRoles).ThenInclude(ur => ur.Rol)
                .Include(u => u.Medico)
                .OrderByDescending(u => u.FechaCreacion)
                .ToListAsync();

            var pendientes = usuarios
                .Where(u => !u.UsuarioRoles.Any())
                .Select(u => new UsuarioListaVM
                {
                    IdUsuario      = u.IdUsuario,
                    NombreCompleto = $"{u.Nombre} {u.Apellido}",
                    Email          = u.Email,
                    Rol            = "Sin rol",
                    Activo         = u.Activo,
                    FechaCreacion  = u.FechaCreacion
                }).ToList();

            var vm = new AdminDashboardVM
            {
                TotalPacientes       = await _context.Pacientes.CountAsync(),
                TotalMedicos         = await _context.Medicos.CountAsync(),
                TotalRecepcionistas  = await _context.UsuariosRoles.CountAsync(ur => ur.Rol.Nombre == "Recepcionista"),
                TurnosHoy            = await _context.Turnos.CountAsync(t => t.FechaHora.Date == hoy),
                TurnosMes            = await _context.Turnos.CountAsync(t => t.FechaHora >= mes),
                UsuariosSinRol       = pendientes.Count,
                UsuariosPendientes   = pendientes,
                TodosUsuarios = usuarios.Select(u => new UsuarioListaVM
                {
                    IdUsuario      = u.IdUsuario,
                    NombreCompleto = $"{u.Nombre} {u.Apellido}",
                    Email          = u.Email,
                    Rol            = u.UsuarioRoles.FirstOrDefault()?.Rol?.Nombre ?? "Sin rol",
                    Activo         = u.Activo,
                    FechaCreacion  = u.FechaCreacion,
                    Matricula      = u.Medico?.Matricula
                }).ToList()
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> AprobarUsuario(int idUsuario)
        {
            var usuario = await _context.Usuarios.FindAsync(idUsuario);
            if (usuario == null) return RedirectToAction("Dashboard");

            var especialidades = await _context.Especialidades
                .OrderBy(e => e.Nombre)
                .Select(e => new EspecialidadCheckVM { IdEspecialidad = e.IdEspecialidad, Nombre = e.Nombre })
                .ToListAsync();

            var vm = new AprobarMedicoVM
            {
                IdUsuario      = usuario.IdUsuario,
                NombreCompleto = $"{usuario.Nombre} {usuario.Apellido}",
                Email          = usuario.Email,
                Especialidades = especialidades
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> AprobarUsuario(AprobarMedicoVM model)
        {
            // Recargar especialidades si hay error de validación
            if (!ModelState.IsValid && model.RolAsignar == "Medico")
            {
                model.Especialidades = await _context.Especialidades
                    .OrderBy(e => e.Nombre)
                    .Select(e => new EspecialidadCheckVM { IdEspecialidad = e.IdEspecialidad, Nombre = e.Nombre })
                    .ToListAsync();
                return View(model);
            }

            var usuario = await _context.Usuarios
                .Include(u => u.UsuarioRoles)
                .FirstOrDefaultAsync(u => u.IdUsuario == model.IdUsuario);
            if (usuario == null) return RedirectToAction("Dashboard");

            if (usuario.UsuarioRoles.Any())
            {
                TempData["Error"] = "Este usuario ya tiene un rol asignado.";
                return RedirectToAction("Dashboard");
            }

            var rol = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == model.RolAsignar);
            if (rol == null)
            {
                TempData["Error"] = $"El rol '{model.RolAsignar}' no existe en la base de datos.";
                return RedirectToAction("Dashboard");
            }

            _context.UsuariosRoles.Add(new UsuarioRol { IdUsuario = model.IdUsuario, IdRol = rol.IdRol });

            if (model.RolAsignar == "Medico")
            {
                var medico = new Medico
                {
                    IdUsuario = model.IdUsuario,
                    Matricula = model.Matricula!,
                    Telefono  = model.Telefono
                };
                _context.Medicos.Add(medico);
                await _context.SaveChangesAsync(); // necesario para obtener IdMedico

                // Asignar especialidades seleccionadas
                foreach (var idEsp in model.EspecialidadesSeleccionadas)
                {
                    _context.MedicosEspecialidades.Add(new MedicoEspecialidad
                    {
                        IdMedico       = medico.IdMedico,
                        IdEspecialidad = idEsp
                    });
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Usuario aprobado como {model.RolAsignar} correctamente.";
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> DesactivarUsuario(int idUsuario)
        {
            var usuario = await _context.Usuarios.FindAsync(idUsuario);
            if (usuario == null) return RedirectToAction("Dashboard");
            usuario.Activo = !usuario.Activo;
            await _context.SaveChangesAsync();
            TempData["Success"] = usuario.Activo ? "Usuario activado." : "Usuario desactivado.";
            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public async Task<IActionResult> EditarUsuario(int idUsuario)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.UsuarioRoles).ThenInclude(ur => ur.Rol)
                .Include(u => u.Medico).ThenInclude(m => m!.Especialidades)
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);
            if (usuario == null) return RedirectToAction("Dashboard");

            var rol = usuario.UsuarioRoles.FirstOrDefault()?.Rol?.Nombre ?? "";
            var especialidadesActivas = usuario.Medico?.Especialidades.Select(e => e.IdEspecialidad).ToList() ?? new();

            var vm = new EditarUsuarioVM
            {
                IdUsuario    = usuario.IdUsuario,
                Rol          = rol,
                Nombre       = usuario.Nombre,
                Apellido     = usuario.Apellido,
                Email        = usuario.Email,
                Matricula    = usuario.Medico?.Matricula,
                Telefono     = usuario.Medico?.Telefono,
                EspecialidadesSeleccionadas = especialidadesActivas,
                Especialidades = await _context.Especialidades
                    .OrderBy(e => e.Nombre)
                    .Select(e => new EspecialidadCheckVM { IdEspecialidad = e.IdEspecialidad, Nombre = e.Nombre })
                    .ToListAsync()
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> EditarUsuario(EditarUsuarioVM model)
        {
            // Quitar validación de password si está vacío (no se cambia)
            if (string.IsNullOrEmpty(model.NuevaPassword))
                ModelState.Remove(nameof(model.NuevaPassword));

            if (!ModelState.IsValid)
            {
                model.Especialidades = await _context.Especialidades
                    .OrderBy(e => e.Nombre)
                    .Select(e => new EspecialidadCheckVM { IdEspecialidad = e.IdEspecialidad, Nombre = e.Nombre })
                    .ToListAsync();
                return View(model);
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Medico).ThenInclude(m => m!.Especialidades)
                .FirstOrDefaultAsync(u => u.IdUsuario == model.IdUsuario);
            if (usuario == null) return RedirectToAction("Dashboard");

            // Verificar email único
            bool emailDuplicado = await _context.Usuarios
                .AnyAsync(u => u.Email == model.Email && u.IdUsuario != model.IdUsuario);
            if (emailDuplicado)
            {
                ModelState.AddModelError("Email", "Ya existe otro usuario con ese email.");
                model.Especialidades = await _context.Especialidades.OrderBy(e => e.Nombre)
                    .Select(e => new EspecialidadCheckVM { IdEspecialidad = e.IdEspecialidad, Nombre = e.Nombre })
                    .ToListAsync();
                return View(model);
            }

            usuario.Nombre   = model.Nombre;
            usuario.Apellido = model.Apellido;
            usuario.Email    = model.Email;

            if (!string.IsNullOrWhiteSpace(model.NuevaPassword))
                usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NuevaPassword);

            // Actualizar datos del médico
            if (usuario.Medico != null)
            {
                if (!string.IsNullOrEmpty(model.Matricula))
                    usuario.Medico.Matricula = model.Matricula;
                usuario.Medico.Telefono = model.Telefono;

                // Actualizar especialidades: quitar las que no están y agregar las nuevas
                var especialidadesActuales = usuario.Medico.Especialidades.Select(e => e.IdEspecialidad).ToList();
                var aQuitar = especialidadesActuales.Except(model.EspecialidadesSeleccionadas).ToList();
                var aAgregar = model.EspecialidadesSeleccionadas.Except(especialidadesActuales).ToList();

                foreach (var idEsp in aQuitar)
                    _context.MedicosEspecialidades.Remove(
                        usuario.Medico.Especialidades.First(e => e.IdEspecialidad == idEsp));

                foreach (var idEsp in aAgregar)
                    _context.MedicosEspecialidades.Add(
                        new MedicoEspecialidad { IdMedico = usuario.Medico.IdMedico, IdEspecialidad = idEsp });
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Usuario {usuario.Nombre} {usuario.Apellido} actualizado correctamente.";
            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public async Task<IActionResult> AnadirPersonal()
        {
            var vm = new AnadirPersonalVM
            {
                Especialidades = await _context.Especialidades
                    .OrderBy(e => e.Nombre)
                    .Select(e => new EspecialidadCheckVM { IdEspecialidad = e.IdEspecialidad, Nombre = e.Nombre })
                    .ToListAsync()
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> AnadirPersonal(AnadirPersonalVM model)
        {
            // Validar matrícula solo si es médico
            if (model.RolAsignar == "Medico" && string.IsNullOrWhiteSpace(model.Matricula))
                ModelState.AddModelError("Matricula", "La matrícula es obligatoria para médicos.");

            if (!ModelState.IsValid)
            {
                model.Especialidades = await _context.Especialidades
                    .OrderBy(e => e.Nombre)
                    .Select(e => new EspecialidadCheckVM { IdEspecialidad = e.IdEspecialidad, Nombre = e.Nombre })
                    .ToListAsync();
                return View(model);
            }

            // Verificar email único
            bool existe = await _context.Usuarios.AnyAsync(u => u.Email == model.Email);
            if (existe)
            {
                ModelState.AddModelError("Email", "Ya existe un usuario con ese email.");
                model.Especialidades = await _context.Especialidades
                    .OrderBy(e => e.Nombre)
                    .Select(e => new EspecialidadCheckVM { IdEspecialidad = e.IdEspecialidad, Nombre = e.Nombre })
                    .ToListAsync();
                return View(model);
            }

            // Crear usuario
            var usuario = new Usuario
            {
                Nombre       = model.Nombre,
                Apellido     = model.Apellido,
                Email        = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Activo       = true
            };
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            // Asignar rol
            var rol = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == model.RolAsignar);
            if (rol != null)
                _context.UsuariosRoles.Add(new UsuarioRol { IdUsuario = usuario.IdUsuario, IdRol = rol.IdRol });

            // Si es médico, crear registro Medico + especialidades
            if (model.RolAsignar == "Medico")
            {
                var medico = new Medico
                {
                    IdUsuario = usuario.IdUsuario,
                    Matricula = model.Matricula!,
                    Telefono  = model.Telefono
                };
                _context.Medicos.Add(medico);
                await _context.SaveChangesAsync();

                foreach (var idEsp in model.EspecialidadesSeleccionadas)
                    _context.MedicosEspecialidades.Add(new MedicoEspecialidad { IdMedico = medico.IdMedico, IdEspecialidad = idEsp });
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"{model.RolAsignar} {model.Nombre} {model.Apellido} creado correctamente.";
            return RedirectToAction("Dashboard");
        }
    }
}
