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

            // 1. Evitar que EL MISMO paciente saque dos lugares en el mismo bloque exacto
            bool pacienteYaTieneTurno = await _context.Turnos.AnyAsync(t =>
                t.IdPaciente == idPaciente &&
                t.IdMedico == idMedico &&
                t.FechaHora == fechaHoraLocal &&
                t.Estado != EstadoTurno.Cancelado &&
                t.Estado != EstadoTurno.Atendido);

            if (pacienteYaTieneTurno)
                return (false, "Ya tenés un turno solicitado en este exacto horario.");

            // 2. Averiguar la CAPACIDAD máxima de ese bloque horario
            int capacidadBloque = 1; // Por defecto 1

            var excepcion = await _context.HorariosExcepciones
                .FirstOrDefaultAsync(e => e.IdMedico == idMedico && e.Fecha.Date == fecha.Date && e.Trabaja && e.HoraDesde == hora);

            if (excepcion != null)
            {
                capacidadBloque = excepcion.Capacidad ?? 1;
            }
            else
            {
                var horarioDefecto = await _context.HorariosAtencion
                    .FirstOrDefaultAsync(h => h.IdMedico == idMedico && h.DiaSemana == fecha.DayOfWeek && h.HoraDesde == hora);

                if (horarioDefecto != null) capacidadBloque = horarioDefecto.Capacidad;
            }

            // 3. Contar cuántos turnos ya están ocupados en ese bloque
            int turnosOcupados = await _context.Turnos.CountAsync(t =>
                t.IdMedico == idMedico &&
                t.FechaHora == fechaHoraLocal &&
                t.Estado != EstadoTurno.Cancelado &&
                t.Estado != EstadoTurno.Atendido);

            if (turnosOcupados >= capacidadBloque)
                return (false, "El cupo para este horario ya está completo.");

            // 4. Si pasó las pruebas, creamos el turno
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

                var excepciones = await _context.HorariosExcepciones
                    .Where(e => e.IdMedico == medicoId && e.Fecha.Date == fechaDate.Date)
                    .ToListAsync();

                var bloquesDelDia = new List<(TimeSpan Desde, TimeSpan Hasta, int Capacidad)>();

                if (excepciones.Any())
                {
                    if (excepciones.Any(e => !e.Trabaja))
                        return new { disponible = false, mensaje = "El profesional no atiende en esta fecha (Día libre)." };

                    foreach (var exc in excepciones.Where(e => e.Trabaja))
                        bloquesDelDia.Add((exc.HoraDesde ?? TimeSpan.Zero, exc.HoraHasta ?? TimeSpan.Zero, exc.Capacidad ?? 1));
                }
                else
                {
                    var diaSemana = fechaDate.DayOfWeek;
                    var horariosDefecto = await _context.HorariosAtencion
                        .Where(h => h.IdMedico == medicoId && h.DiaSemana == diaSemana)
                        .ToListAsync();

                    if (!horariosDefecto.Any())
                        return new { disponible = false, mensaje = "El profesional no atiende en este día de la semana." };

                    foreach (var h in horariosDefecto)
                        bloquesDelDia.Add((h.HoraDesde, h.HoraHasta, h.Capacidad));
                }

                // Buscamos los turnos ocupados
                var turnosOcupadosTotales = await _context.Turnos
                    .Where(t => t.IdMedico == medicoId && t.FechaHora.Date == fechaDate.Date &&
                                t.Estado != EstadoTurno.Cancelado && t.Estado != EstadoTurno.Atendido)
                    .Select(t => t.FechaHora.TimeOfDay)
                    .ToListAsync();

                var slots = new List<object>();
                bool hayAlgunLugar = false;

                // Generamos los botones separados por cada bloque usando estrictamente formato 24hs (Hours:D2)
                foreach (var bloque in bloquesDelDia.OrderBy(b => b.Desde))
                {
                    int ocupados = turnosOcupadosTotales.Count(t => t == bloque.Desde);
                    bool hayLugar = ocupados < bloque.Capacidad;
                    if (hayLugar) hayAlgunLugar = true;

                    slots.Add(new
                    {
                        hora = $"{bloque.Desde.Hours:D2}:{bloque.Desde.Minutes:D2}",
                        texto = $"{bloque.Desde.Hours:D2}:{bloque.Desde.Minutes:D2} a {bloque.Hasta.Hours:D2}:{bloque.Hasta.Minutes:D2} ({ocupados}/{bloque.Capacidad} ocupados)",
                        ocupado = !hayLugar
                    });
                }

                return new { disponible = hayAlgunLugar, slots = slots };
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