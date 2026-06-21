using Microsoft.EntityFrameworkCore;
using SistemaSaludGoya.Data;
using SistemaSaludGoya.Models;
using SistemaSaludGoya.ViewModel;

namespace SistemaSaludGoya.Services
{
    public class HomeService : IHomeService
    {
        private readonly AppDbContext _context;
        public HomeService(AppDbContext context) { _context = context; }

        public async Task<HomeDashboardVM> ObtenerDashboardPacienteAsync(int idUsuario, int idPaciente)
        {
            var usuario = await _context.Usuarios.Include(u => u.Paciente).FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

            var turnos = await _context.Turnos
                .Where(t => t.IdPaciente == idPaciente)
                .Include(t => t.Medico).ThenInclude(m => m.Usuario)
                .ToListAsync();

            var proximo = turnos
                .Where(t => t.FechaHora > DateTime.Now && t.Estado != EstadoTurno.Cancelado && t.Estado != EstadoTurno.Atendido)
                .OrderBy(t => t.FechaHora).FirstOrDefault();

            return new HomeDashboardVM
            {
                IdUsuario = idUsuario,
                IdPaciente = idPaciente,
                NombreCompleto = $"{usuario?.Nombre} {usuario?.Apellido}",
                Email = usuario?.Email ?? "",
                Telefono = usuario?.Paciente?.Telefono,
                Direccion = usuario?.Paciente?.Direccion,
                Dni = usuario?.Paciente?.Dni,
                FechaNacimiento = usuario?.Paciente?.FechaNacimiento,
                TurnosPendientes = turnos.Count(t => t.Estado == EstadoTurno.Solicitado),
                TurnosConfirmados = turnos.Count(t => t.Estado == EstadoTurno.Confirmado),
                TurnosAtendidos = turnos.Count(t => t.Estado == EstadoTurno.Atendido),
                TurnosCancelados = turnos.Count(t => t.Estado == EstadoTurno.Cancelado),
                ProximoTurno = proximo?.FechaHora,
                ProximoTurnoMedico = proximo?.Medico?.Usuario != null ? $"Dr/a. {proximo.Medico.Usuario.Nombre} {proximo.Medico.Usuario.Apellido}" : null
            };
        }
    }
}