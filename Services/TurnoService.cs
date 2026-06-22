using Microsoft.EntityFrameworkCore;
using SistemaSaludGoya.Data;
using SistemaSaludGoya.Models;

namespace SistemaSaludGoya.Services
{
    public class TurnoService : ITurnoService
    {
        private readonly AppDbContext _context;

        public TurnoService(AppDbContext context) { _context = context; }

        public async Task<List<Turno>> ObtenerTodosAsync()
        {
            return await _context.Turnos
                .Include(t => t.Medico).ThenInclude(m => m.Usuario)
                .Include(t => t.Paciente).ThenInclude(p => p.Usuario)
                .ToListAsync();
        }

        public async Task<Turno?> ObtenerPorIdAsync(int id)
        {
            return await _context.Turnos
                .Include(t => t.Medico).ThenInclude(m => m.Usuario)
                .Include(t => t.Paciente).ThenInclude(p => p.Usuario)
                .FirstOrDefaultAsync(x => x.IdTurno == id);
        }

        public async Task CrearAsync(Turno turno) { _context.Turnos.Add(turno); await _context.SaveChangesAsync(); }
        public async Task ActualizarAsync(Turno turno) { _context.Turnos.Update(turno); await _context.SaveChangesAsync(); }
        public async Task EliminarAsync(int id) { var t = await _context.Turnos.FindAsync(id); if (t != null) { _context.Turnos.Remove(t); await _context.SaveChangesAsync(); } }

        public async Task<List<Especialidad>> ObtenerEspecialidadesAsync() => await _context.Especialidades.ToListAsync();

        public async Task<List<Turno>> ObtenerTurnosPacienteAsync(int idPaciente)
        {
            return await _context.Turnos
                .Where(t => t.IdPaciente == idPaciente)
                .Include(t => t.Medico).ThenInclude(m => m.Usuario)
                .OrderByDescending(t => t.FechaHora)
                .ToListAsync();
        }

        public async Task<(bool Ok, string Mensaje)> CrearTurnoAsync(int idMedico, int idPaciente, DateTime fecha, TimeSpan hora, string motivo)
        {
            var fechaHoraLocal = DateTime.SpecifyKind(fecha.Date.Add(hora), DateTimeKind.Local);

            bool turnoOcupado = await _context.Turnos.AnyAsync(t =>
                t.IdMedico == idMedico &&
                t.FechaHora == fechaHoraLocal &&
                t.Estado != EstadoTurno.Cancelado &&
                t.Estado != EstadoTurno.Atendido);

            if (turnoOcupado) return (false, "El profesional ya tiene un turno en ese horario.");

            _context.Turnos.Add(new Turno
            {
                IdMedico = idMedico,
                IdPaciente = idPaciente,
                FechaHora = fechaHoraLocal,
                MotivoConsulta = motivo,
                Estado = EstadoTurno.Solicitado,
                FechaCreacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local)
            });
            await _context.SaveChangesAsync();
            return (true, "Turno creado correctamente.");
        }

        public async Task<(bool Ok, string Mensaje)> CancelarTurnoPacienteAsync(int idTurno, int idPaciente, string motivo)
        {
            var turno = await _context.Turnos.FirstOrDefaultAsync(t => t.IdTurno == idTurno && t.IdPaciente == idPaciente);
            if (turno == null) return (false, "Turno no encontrado.");

            turno.Estado = EstadoTurno.Cancelado;
            turno.MotivoConsulta = $"[CANCELADO] {motivo}";
            await _context.SaveChangesAsync();
            return (true, "Turno cancelado correctamente.");
        }

        public async Task<object> ObtenerMedicosPorEspecialidadAsync(int idEspecialidad)
        {
            return await _context.MedicosEspecialidades
                .Where(me => me.IdEspecialidad == idEspecialidad)
                .Select(me => new { idMedico = me.IdMedico, nombre = me.Medico.Usuario.Nombre + " " + me.Medico.Usuario.Apellido })
                .ToListAsync();
        }

        public async Task<object> ObtenerHorariosOcupadosAsync(int medicoId, string fecha)
        {
            try
            {
                if (!DateTime.TryParse(fecha, out DateTime fechaDate))
                    return new { error = "Fecha inválida", slots = new List<object>() };

                var diaSemana = fechaDate.DayOfWeek;
                var todosHorarios = await _context.HorariosAtencion
                    .Where(h => h.IdMedico == medicoId && h.DiaSemana == diaSemana).ToListAsync();

                var turnosDelDia = await _context.Turnos
                    .Where(t => t.IdMedico == medicoId && t.FechaHora.Date == fechaDate.Date && t.Estado != EstadoTurno.Cancelado && t.Estado != EstadoTurno.Atendido)
                    .ToListAsync();

                var turnosOcupados = turnosDelDia.ToDictionary(t => t.FechaHora.ToString("HH:mm"), t => t.Estado.ToString());

                List<string> slots;
                if (todosHorarios.Any())
                {
                    slots = new List<string>();
                    foreach (var h in todosHorarios)
                    {
                        var actual = h.HoraDesde;
                        while (actual.Add(TimeSpan.FromMinutes(30)) <= h.HoraHasta)
                        {
                            slots.Add(actual.Hours.ToString("D2") + ":" + actual.Minutes.ToString("D2"));
                            actual = actual.Add(TimeSpan.FromMinutes(30));
                        }
                    }
                    slots = slots.Distinct().OrderBy(s => s).ToList();
                }
                else
                {
                    slots = new List<string> { "08:00", "08:30", "09:00", "09:30", "10:00", "10:30", "11:00", "11:30", "12:00", "14:00", "14:30", "15:00", "15:30", "16:00", "16:30", "17:00" };
                }

                var resultado = slots.Select(s => new
                {
                    hora = s,
                    ocupado = turnosOcupados.ContainsKey(s),
                    estado = turnosOcupados.ContainsKey(s) ? turnosOcupados[s] : null
                }).ToList();
                return new { disponible = true, slots = resultado };
            }
            catch (Exception ex) { return new { error = ex.Message, slots = new List<object>() }; }
        }
        public async Task<List<int>> ObtenerDiasHabilesAsync(int medicoId)
        {
            return await _context.HorariosAtencion
                .Where(h => h.IdMedico == medicoId)
                .Select(h => (int)h.DiaSemana)
                .Distinct()
                .ToListAsync();
        }
    }
}