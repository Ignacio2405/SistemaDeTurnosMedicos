using Microsoft.EntityFrameworkCore;
using SistemaSaludGoya.Data;
using SistemaSaludGoya.Models;
using SistemaSaludGoya.ViewModel;

namespace SistemaSaludGoya.Services
{
    public class MedicoService : IMedicoService
    {
        private readonly AppDbContext _context;

        public MedicoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int?> GetIdMedicoAsync( int idUsuario, bool esAdministrador, int? medicoIdQuery)
        {
            if (esAdministrador && medicoIdQuery.HasValue)
                return medicoIdQuery.Value;

            return await _context.Medicos
                .Where(m => m.IdUsuario == idUsuario)
                .Select(m => (int?)m.IdMedico)
                .FirstOrDefaultAsync();
        }

        public async Task<MedicoDashboardVM?> ObtenerDashboardAsync( int idMedico, bool esMedico)
        {
            var medico = await _context.Medicos
                .Include(m => m.Usuario)
                .Include(m => m.Horarios)
                .FirstOrDefaultAsync(m => m.IdMedico == idMedico);

            if (medico == null)
                return null;

            var turnosSinEstado = await _context.Turnos
                .Where(t => t.IdMedico == idMedico &&
                            (int)(object)t.Estado == 0)
                .ToListAsync();

            if (turnosSinEstado.Any())
            {
                turnosSinEstado.ForEach(t =>
                    t.Estado = EstadoTurno.Solicitado);

                await _context.SaveChangesAsync();
            }

            var hoy = DateTime.Today;
            var manana = hoy.AddDays(1);

            var turnosHoy = await _context.Turnos
                .Where(t => t.IdMedico == idMedico
                         && t.FechaHora >= hoy
                         && t.FechaHora < manana)
                .Include(t => t.Paciente)
                    .ThenInclude(p => p.Usuario)
                .Include(t => t.Consulta)
                .OrderBy(t => t.FechaHora)
                .ToListAsync();

            var proximos = await _context.Turnos
                .Where(t => t.IdMedico == idMedico
                         && t.FechaHora >= manana
                         && t.Estado != EstadoTurno.Cancelado)
                .Include(t => t.Paciente)
                    .ThenInclude(p => p.Usuario)
                .OrderBy(t => t.FechaHora)
                .Take(15)
                .ToListAsync();

            return new MedicoDashboardVM
            {
                IdMedico = medico.IdMedico,
                NombreCompleto = $"Dr/a. {medico.Usuario.Nombre} {medico.Usuario.Apellido}",
                Matricula = medico.Matricula,

                TurnosHoy = turnosHoy.Count(t =>
                    t.Estado != EstadoTurno.Cancelado),

                TurnosPendientes = turnosHoy.Count(t =>
                    t.Estado == EstadoTurno.Solicitado),

                TurnosConfirmados = turnosHoy.Count(t =>
                    t.Estado == EstadoTurno.Confirmado),

                TurnosAtendidos = turnosHoy.Count(t =>
                    t.Estado == EstadoTurno.Atendido),

                TurnosDelDia = turnosHoy.Select(t =>
                    new TurnoResumenVM
                    {
                        IdTurno = t.IdTurno,
                        IdPaciente = t.IdPaciente,
                        FechaHora = t.FechaHora,
                        PacienteNombre =
                            $"{t.Paciente.Usuario.Nombre} {t.Paciente.Usuario.Apellido}",
                        PacienteDni = t.Paciente.Dni,
                        Estado = t.Estado.ToString(),
                        Motivo = t.MotivoConsulta,
                        TieneConsulta = t.Consulta != null
                    }).ToList(),

                ProximosTurnos = proximos.Select(t =>
                    new TurnoResumenVM
                    {
                        IdTurno = t.IdTurno,
                        IdPaciente = t.IdPaciente,
                        FechaHora = t.FechaHora,
                        PacienteNombre =
                            $"{t.Paciente.Usuario.Nombre} {t.Paciente.Usuario.Apellido}",
                        PacienteDni = t.Paciente.Dni,
                        Estado = t.Estado.ToString(),
                        Motivo = t.MotivoConsulta
                    }).ToList()
            };
        }

        public async Task<bool> ConfirmarTurnoAsync( int idTurno, int idMedico)
        {
            var turno = await _context.Turnos
                .FirstOrDefaultAsync(t =>
                    t.IdTurno == idTurno &&
                    t.IdMedico == idMedico);

            if (turno == null)
                return false;

            turno.Estado = EstadoTurno.Confirmado;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<(bool Ok, string Mensaje)> CancelarTurnoAsync( int idTurno, int idMedico, string motivo)
        {
            if (string.IsNullOrWhiteSpace(motivo))
                return (false,
                    "Debés indicar el motivo de cancelación.");

            var turno = await _context.Turnos
                .FirstOrDefaultAsync(t =>
                    t.IdTurno == idTurno &&
                    t.IdMedico == idMedico);

            if (turno == null)
                return (false, "Turno no encontrado.");

            turno.Estado = EstadoTurno.Cancelado;
            turno.MotivoConsulta =
                $"[CANCELADO POR MÉDICO] {motivo}";

            await _context.SaveChangesAsync();

            return (true, "Turno cancelado.");
        }

        public async Task<ConsultaVM?> ObtenerTurnoParaAtenderAsync( int idTurno, int idMedico)
        {
            var turno = await _context.Turnos
                .Include(t => t.Paciente)
                    .ThenInclude(p => p.Usuario)
                .Include(t => t.Consulta)
                .FirstOrDefaultAsync(t =>
                    t.IdTurno == idTurno &&
                    t.IdMedico == idMedico);

            if (turno == null ||
                turno.Estado == EstadoTurno.Cancelado ||
                turno.Consulta != null)
                return null;

            return new ConsultaVM
            {
                IdTurno = turno.IdTurno,
                PacienteNombre =
                    $"{turno.Paciente.Usuario.Nombre} {turno.Paciente.Usuario.Apellido}",
                PacienteDni = turno.Paciente.Dni,
                FechaHora = turno.FechaHora,
                MotivoConsulta = turno.MotivoConsulta
            };
        }

        public async Task<bool> GuardarConsultaAsync( ConsultaVM model, int idMedico)
        {
            var turno = await _context.Turnos
                .FirstOrDefaultAsync(t =>
                    t.IdTurno == model.IdTurno &&
                    t.IdMedico == idMedico);

            if (turno == null)
                return false;

            _context.Consultas.Add(new Consulta
            {
                IdTurno = model.IdTurno,
                Diagnostico = model.Diagnostico,
                Observaciones = model.Observaciones,
                Indicaciones = model.Indicaciones,
                FechaConsulta = DateTime.Now
            });

            turno.Estado = EstadoTurno.Atendido;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<HistorialPacienteVM?> ObtenerHistorialPacienteAsync( int idPaciente)
        {
            var paciente = await _context.Pacientes
                .Include(p => p.Usuario)
                .FirstOrDefaultAsync(p =>
                    p.IdPaciente == idPaciente);

            if (paciente == null)
                return null;

            var consultas = await _context.Consultas
                .Include(c => c.Turno)
                    .ThenInclude(t => t.Medico)
                    .ThenInclude(m => m.Usuario)
                .Where(c => c.Turno.IdPaciente == idPaciente)
                .OrderByDescending(c => c.FechaConsulta)
                .ToListAsync();

            return new HistorialPacienteVM
            {
                IdPaciente = idPaciente,
                NombreCompleto =
                    $"{paciente.Usuario.Nombre} {paciente.Usuario.Apellido}",
                Dni = paciente.Dni,
                Telefono = paciente.Telefono,
                FechaNacimiento = paciente.FechaNacimiento,

                Consultas = consultas.Select(c =>
                    new ConsultaHistorialVM
                    {
                        IdConsulta = c.IdConsulta,
                        FechaConsulta = c.FechaConsulta,
                        MedicoNombre =
                            $"Dr/a. {c.Turno.Medico.Usuario.Nombre} {c.Turno.Medico.Usuario.Apellido}",
                        Diagnostico = c.Diagnostico,
                        Observaciones = c.Observaciones,
                        Indicaciones = c.Indicaciones
                    }).ToList()
            };
        }
    }
}
