using Microsoft.EntityFrameworkCore;
using SistemaSaludGoya.Data;
using SistemaSaludGoya.Models;
using SistemaSaludGoya.ViewModel;

namespace SistemaSaludGoya.Services
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _context;

        public AdminService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AdminDashboardVM> ObtenerDashboardAsync()
        {
            var hoy = DateTime.Today;
            var mes = new DateTime(hoy.Year, hoy.Month, 1);

            var usuarios = await _context.Usuarios
                .Include(u => u.UsuarioRoles).ThenInclude(ur => ur.Rol)
                .Include(u => u.Medico)
                .OrderByDescending(u => u.FechaCreacion)
                .ToListAsync();

            var pendientes = usuarios
                .Where(u => !u.UsuarioRoles.Any())
                .Select(u => new UsuarioListaVM
                {
                    IdUsuario = u.IdUsuario,
                    NombreCompleto = $"{u.Nombre} {u.Apellido}",
                    Email = u.Email,
                    Rol = "Sin rol",
                    Activo = u.Activo,
                    FechaCreacion = u.FechaCreacion
                }).ToList();

            return new AdminDashboardVM
            {
                TotalPacientes = await _context.Pacientes.CountAsync(),
                TotalMedicos = await _context.Medicos.CountAsync(),
                TotalRecepcionistas = await _context.UsuariosRoles.CountAsync(ur => ur.Rol.Nombre == "Recepcionista"),
                TurnosHoy = await _context.Turnos.CountAsync(t => t.FechaHora.Date == hoy),
                TurnosMes = await _context.Turnos.CountAsync(t => t.FechaHora >= mes),
                UsuariosSinRol = pendientes.Count,
                UsuariosPendientes = pendientes,
                TodosUsuarios = usuarios.Select(u => new UsuarioListaVM
                {
                    IdUsuario = u.IdUsuario,
                    NombreCompleto = $"{u.Nombre} {u.Apellido}",
                    Email = u.Email,
                    Rol = u.UsuarioRoles.FirstOrDefault()?.Rol?.Nombre ?? "Sin rol",
                    Activo = u.Activo,
                    FechaCreacion = u.FechaCreacion,
                    Matricula = u.Medico?.Matricula
                }).ToList()
            };
        }

        public async Task<AprobarMedicoVM?> ObtenerUsuarioParaAprobarAsync(int idUsuario)
        {
            var usuario = await _context.Usuarios.FindAsync(idUsuario);
            if (usuario == null) return null;

            return new AprobarMedicoVM
            {
                IdUsuario = usuario.IdUsuario,
                NombreCompleto = $"{usuario.Nombre} {usuario.Apellido}",
                Email = usuario.Email,
                Especialidades = await ObtenerEspecialidadesAsync()
            };
        }

        public async Task<(bool Ok, string Mensaje)> AprobarUsuarioAsync(AprobarMedicoVM model)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.UsuarioRoles)
                .FirstOrDefaultAsync(u => u.IdUsuario == model.IdUsuario);

            if (usuario == null) return (false, "Usuario no encontrado.");
            if (usuario.UsuarioRoles.Any()) return (false, "Este usuario ya tiene un rol asignado.");

            var rol = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == model.RolAsignar);
            if (rol == null) return (false, $"El rol '{model.RolAsignar}' no existe en la base de datos.");

            _context.UsuariosRoles.Add(new UsuarioRol { IdUsuario = model.IdUsuario, IdRol = rol.IdRol });

            if (model.RolAsignar == "Medico")
            {
                var medico = new Medico
                {
                    IdUsuario = model.IdUsuario,
                    Matricula = model.Matricula!,
                    Telefono = model.Telefono
                };
                _context.Medicos.Add(medico);
                await _context.SaveChangesAsync();

                foreach (var idEsp in model.EspecialidadesSeleccionadas)
                {
                    _context.MedicosEspecialidades.Add(new MedicoEspecialidad
                    {
                        IdMedico = medico.IdMedico,
                        IdEspecialidad = idEsp
                    });
                }
            }

            await _context.SaveChangesAsync();
            return (true, $"Usuario aprobado como {model.RolAsignar} correctamente.");
        }

        public async Task<(bool Ok, string Mensaje)> CambiarEstadoUsuarioAsync(int idUsuario)
        {
            var usuario = await _context.Usuarios.FindAsync(idUsuario);
            if (usuario == null) return (false, "Usuario no encontrado");

            usuario.Activo = !usuario.Activo;
            await _context.SaveChangesAsync();

            return (true, usuario.Activo ? "Usuario activado." : "Usuario desactivado.");
        }

        public async Task<EditarUsuarioVM?> ObtenerUsuarioParaEditarAsync(int idUsuario)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.UsuarioRoles).ThenInclude(ur => ur.Rol)
                .Include(u => u.Medico).ThenInclude(m => m!.Especialidades)
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

            if (usuario == null) return null;

            return new EditarUsuarioVM
            {
                IdUsuario = usuario.IdUsuario,
                Rol = usuario.UsuarioRoles.FirstOrDefault()?.Rol?.Nombre ?? "",
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Email = usuario.Email,
                Matricula = usuario.Medico?.Matricula,
                Telefono = usuario.Medico?.Telefono,
                EspecialidadesSeleccionadas = usuario.Medico?.Especialidades.Select(e => e.IdEspecialidad).ToList() ?? new(),
                Especialidades = await ObtenerEspecialidadesAsync()
            };
        }

        public async Task<(bool Ok, string Mensaje)> EditarUsuarioAsync(EditarUsuarioVM model)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Medico).ThenInclude(m => m!.Especialidades)
                .FirstOrDefaultAsync(u => u.IdUsuario == model.IdUsuario);

            if (usuario == null) return (false, "Usuario no encontrado.");

            bool emailDuplicado = await _context.Usuarios
                .AnyAsync(u => u.Email == model.Email && u.IdUsuario != model.IdUsuario);

            if (emailDuplicado) return (false, "Ya existe otro usuario con ese email.");

            usuario.Nombre = model.Nombre;
            usuario.Apellido = model.Apellido;
            usuario.Email = model.Email;

            if (!string.IsNullOrWhiteSpace(model.NuevaPassword))
                usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NuevaPassword);

            if (usuario.Medico != null)
            {
                if (!string.IsNullOrEmpty(model.Matricula))
                    usuario.Medico.Matricula = model.Matricula;
                usuario.Medico.Telefono = model.Telefono;

                var actuales = usuario.Medico.Especialidades.Select(e => e.IdEspecialidad).ToList();
                var aQuitar = actuales.Except(model.EspecialidadesSeleccionadas).ToList();
                var aAgregar = model.EspecialidadesSeleccionadas.Except(actuales).ToList();

                foreach (var idEsp in aQuitar)
                    _context.MedicosEspecialidades.Remove(
                        usuario.Medico.Especialidades.First(e => e.IdEspecialidad == idEsp));

                foreach (var idEsp in aAgregar)
                    _context.MedicosEspecialidades.Add(
                        new MedicoEspecialidad { IdMedico = usuario.Medico.IdMedico, IdEspecialidad = idEsp });
            }

            await _context.SaveChangesAsync();
            return (true, $"Usuario {usuario.Nombre} {usuario.Apellido} actualizado correctamente.");
        }

        public async Task<AnadirPersonalVM> ObtenerDatosAltaPersonalAsync()
        {
            return new AnadirPersonalVM
            {
                Especialidades = await ObtenerEspecialidadesAsync()
            };
        }

        public async Task<(bool Ok, string Mensaje)> AnadirPersonalAsync(AnadirPersonalVM model)
        {
            bool existe = await _context.Usuarios.AnyAsync(u => u.Email == model.Email);
            if (existe) return (false, "Ya existe un usuario con ese email.");

            var usuario = new Usuario
            {
                Nombre = model.Nombre,
                Apellido = model.Apellido,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Activo = true
            };
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync(); // Se guarda para generar el IdUsuario

            var rol = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == model.RolAsignar);
            if (rol != null)
                _context.UsuariosRoles.Add(new UsuarioRol { IdUsuario = usuario.IdUsuario, IdRol = rol.IdRol });

            if (model.RolAsignar == "Medico")
            {
                var medico = new Medico
                {
                    IdUsuario = usuario.IdUsuario,
                    Matricula = model.Matricula!,
                    Telefono = model.Telefono
                };
                _context.Medicos.Add(medico);
                await _context.SaveChangesAsync(); // Se guarda para generar el IdMedico

                foreach (var idEsp in model.EspecialidadesSeleccionadas)
                    _context.MedicosEspecialidades.Add(new MedicoEspecialidad { IdMedico = medico.IdMedico, IdEspecialidad = idEsp });
            }

            await _context.SaveChangesAsync();
            return (true, $"{model.RolAsignar} {model.Nombre} {model.Apellido} creado correctamente.");
        }

        public async Task<List<EspecialidadCheckVM>> ObtenerEspecialidadesAsync()
        {
            return await _context.Especialidades
                .OrderBy(e => e.Nombre)
                .Select(e => new EspecialidadCheckVM { IdEspecialidad = e.IdEspecialidad, Nombre = e.Nombre })
                .ToListAsync();
        }
    }
}