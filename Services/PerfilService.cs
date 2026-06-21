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
                    var h = usuario.Medico?.Horarios.FirstOrDefault(x => x.DiaSemana == d.Dia);
                    return new HorarioDiaVM
                    {
                        DiaSemana = d.Dia,
                        NombreDia = d.Nombre,
                        Activo = h != null,
                        HoraDesde = h != null ? $"{h.HoraDesde.Hours:D2}:{h.HoraDesde.Minutes:D2}" : "08:00",
                        HoraHasta = h != null ? $"{h.HoraHasta.Hours:D2}:{h.HoraHasta.Minutes:D2}" : "13:00",
                    };
                }).ToList()
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

        public async Task<(bool Ok, string Mensaje)> GuardarHorariosAsync(int idUsuario, List<bool> diasActivos, List<string> horasDesde, List<string> horasHasta)
        {
            var medico = await _context.Medicos
                .Include(m => m.Horarios)
                .FirstOrDefaultAsync(m => m.IdUsuario == idUsuario);

            if (medico == null) return (false, "Médico no encontrado.");

            _context.HorariosAtencion.RemoveRange(medico.Horarios);
            await _context.SaveChangesAsync();

            for (int i = 0; i < DiasOrdenados.Length; i++)
            {
                if (i < diasActivos.Count && diasActivos[i])
                {
                    var desdeStr = i < horasDesde.Count ? horasDesde[i] : "08:00";
                    var hastaStr = i < horasHasta.Count ? horasHasta[i] : "13:00";
                    if (TimeSpan.TryParse(desdeStr, out var desde) && TimeSpan.TryParse(hastaStr, out var hasta) && desde < hasta)
                    {
                        _context.HorariosAtencion.Add(new HorarioAtencion
                        {
                            IdMedico = medico.IdMedico,
                            DiaSemana = DiasOrdenados[i].Dia,
                            HoraDesde = desde,
                            HoraHasta = hasta
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
            return (true, "Horarios actualizados correctamente.");
        }
    }
}