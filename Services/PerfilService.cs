using Microsoft.EntityFrameworkCore;
using SistemaSaludGoya.Data;
using SistemaSaludGoya.ViewModel;
using SistemaSaludGoya.Models;

namespace SistemaSaludGoya.Services
{
    public class PerfilService : IPerfilService
    {
        private readonly AppDbContext _context;

        public PerfilService(AppDbContext context) { _context = context; }

        private static readonly (DayOfWeek Dia, string Nombre)[] DiasOrdenados =
        {
            (DayOfWeek.Monday,    "Lunes"), (DayOfWeek.Tuesday,   "Martes"),
            (DayOfWeek.Wednesday, "Miércoles"), (DayOfWeek.Thursday,  "Jueves"),
            (DayOfWeek.Friday,    "Viernes"), (DayOfWeek.Saturday,  "Sábado"),
            (DayOfWeek.Sunday,    "Domingo"),
        };

        public async Task<EditarPerfilVM?> ObtenerPerfilAsync(int idUsuario, string rol)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Paciente)
                .Include(u => u.Medico).ThenInclude(m => m!.Horarios)
                .Include(u => u.Medico).ThenInclude(m => m!.ExcepcionesHorarias)
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

            if (usuario == null) return null;

            return new EditarPerfilVM
            {
                IdUsuario = idUsuario,
                Rol = rol,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Email = usuario.Email,
                Dni = usuario.Paciente?.Dni,
                Telefono = usuario.Paciente?.Telefono ?? usuario.Medico?.Telefono,
                Direccion = usuario.Paciente?.Direccion,
                FechaNacimiento = usuario.Paciente?.FechaNacimiento,
                Matricula = usuario.Medico?.Matricula,
                Horarios = DiasOrdenados.Select(d =>
                {
                    var hs = usuario.Medico?.Horarios.Where(x => x.DiaSemana == d.Dia).OrderBy(x => x.HoraDesde).ToList() ?? new List<HorarioAtencion>();

                    // Formateo ESTRICTO 24hs (evita cualquier bug de AM/PM)
                    var bloques = hs.Select(h => new HorarioBloqueVM
                    {
                        HoraDesde = $"{h.HoraDesde.Hours:D2}:{h.HoraDesde.Minutes:D2}",
                        HoraHasta = $"{h.HoraHasta.Hours:D2}:{h.HoraHasta.Minutes:D2}",
                        Capacidad = h.Capacidad
                    }).ToList();

                    // Si no tiene bloques guardados, le damos uno vacío por defecto para la UI
                    if (!bloques.Any()) bloques.Add(new HorarioBloqueVM());

                    return new HorarioDiaVM
                    {
                        DiaSemana = d.Dia,
                        NombreDia = d.Nombre,
                        Activo = hs.Any(),
                        Bloques = bloques
                    };
                }).ToList(),
                Excepciones = usuario.Medico?.ExcepcionesHorarias.Select(e => new HorarioExcepcionVM
                {
                    Fecha = e.Fecha,
                    Trabaja = e.Trabaja,
                    Capacidad = e.Capacidad,
                    HoraDesde = e.HoraDesde.HasValue ? $"{e.HoraDesde.Value.Hours:D2}:{e.HoraDesde.Value.Minutes:D2}" : null,
                    HoraHasta = e.HoraHasta.HasValue ? $"{e.HoraHasta.Value.Hours:D2}:{e.HoraHasta.Value.Minutes:D2}" : null
                }).ToList() ?? new List<HorarioExcepcionVM>()
            };
        }

        public async Task<(bool Ok, string Mensaje, bool EmailDuplicado)> GuardarPerfilAsync(int idUsuario, EditarPerfilVM model)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Paciente)
                .Include(u => u.Medico)
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

            if (usuario == null) return (false, "Usuario no encontrado", false);

            if (await _context.Usuarios.AnyAsync(u => u.Email == model.Email && u.IdUsuario != idUsuario))
                return (false, "Ya existe otro usuario con ese email.", true);

            usuario.Nombre = model.Nombre;
            usuario.Apellido = model.Apellido;
            usuario.Email = model.Email;

            if (!string.IsNullOrWhiteSpace(model.NuevaPassword))
                usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NuevaPassword);

            if (usuario.Paciente != null)
            {
                if (!string.IsNullOrEmpty(model.Dni))
                    usuario.Paciente.Dni = model.Dni;

                usuario.Paciente.Telefono = model.Telefono;
                usuario.Paciente.Direccion = model.Direccion;

                if (model.FechaNacimiento.HasValue)
                    usuario.Paciente.FechaNacimiento = model.FechaNacimiento.Value.ToUniversalTime();
            }

            if (usuario.Medico != null)
            {
                if (!string.IsNullOrEmpty(model.Matricula)) usuario.Medico.Matricula = model.Matricula;
                usuario.Medico.Telefono = model.Telefono;
            }

            await _context.SaveChangesAsync();
            return (true, "Perfil actualizado correctamente.", false);
        }

        public async Task<(bool Ok, string Mensaje)> GuardarHorariosAsync(int idUsuario, GuardarHorariosDTO request)
        {
            var medico = await _context.Medicos
                .Include(m => m.Horarios)
                .Include(m => m.ExcepcionesHorarias)
                .FirstOrDefaultAsync(m => m.IdUsuario == idUsuario);

            if (medico == null) return (false, "Médico no encontrado.");

            _context.HorariosAtencion.RemoveRange(medico.Horarios);

            // MAGIA ACÁ: Usamos el DiaSemana correcto mandado desde JS resolviendo el bug.
            foreach (var dia in request.HorariosDefecto.Where(x => x.Activo))
            {
                foreach (var b in dia.Bloques)
                {
                    if (TimeSpan.TryParse(b.HoraDesde, out var desde) && TimeSpan.TryParse(b.HoraHasta, out var hasta))
                    {
                        _context.HorariosAtencion.Add(new HorarioAtencion
                        {
                            IdMedico = medico.IdMedico,
                            DiaSemana = dia.DiaSemana, // Ya no usa un índice roto
                            HoraDesde = desde,
                            HoraHasta = hasta,
                            Capacidad = b.Capacidad
                        });
                    }
                }
            }

            _context.HorariosExcepciones.RemoveRange(medico.ExcepcionesHorarias);
            foreach (var exc in request.Excepciones)
            {
                var excepcion = new HorarioExcepcion { IdMedico = medico.IdMedico, Fecha = DateTime.SpecifyKind(exc.Fecha, DateTimeKind.Utc), Trabaja = exc.Trabaja };
                if (exc.Trabaja)
                {
                    if (TimeSpan.TryParse(exc.HoraDesde, out var desde)) excepcion.HoraDesde = desde;
                    if (TimeSpan.TryParse(exc.HoraHasta, out var hasta)) excepcion.HoraHasta = hasta;
                    excepcion.Capacidad = exc.Capacidad ?? 1;
                }
                _context.HorariosExcepciones.Add(excepcion);
            }

            await _context.SaveChangesAsync();
            return (true, "Agenda guardada correctamente.");
        }
    }
}