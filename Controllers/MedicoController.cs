using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaSaludGoya.Data;
using SistemaSaludGoya.Models;
using SistemaSaludGoya.ViewModel;
using System.Security.Claims;

namespace SistemaSaludGoya.Controllers
{
    [Authorize(Roles = "Medico,Administrador")]
    public class MedicoController : Controller
    {
        private readonly AppDbContext _context;
        public MedicoController(AppDbContext context) { _context = context; }

        private int? GetIdMedico()
        {
            var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            if (User.IsInRole("Administrador"))
            {
                // Admin puede pasar idMedico por query
                if (Request.Query.TryGetValue("medicoId", out var v) && int.TryParse(v, out int mid))
                    return mid;
            }
            return _context.Medicos.Where(m => m.IdUsuario == idUsuario).Select(m => (int?)m.IdMedico).FirstOrDefault();
        }

        public async Task<IActionResult> Dashboard()
        {
            var idMedico = GetIdMedico();
            if (idMedico == null) return RedirectToAction("Login", "Auth");

            var medico = await _context.Medicos
                .Include(m => m.Usuario)
                .Include(m => m.Horarios)
                .FirstOrDefaultAsync(m => m.IdMedico == idMedico);
            if (medico == null) return RedirectToAction("Login", "Auth");

            // PRIMER LOGIN: si el médico no tiene horarios configurados, redirigir obligatoriamente al perfil
            // (Solo aplica a médicos reales, no al admin navegando como médico)
            if (User.IsInRole("Medico") && !medico.Horarios.Any())
            {
                TempData["AlertInfo"] = "👋 ¡Bienvenido! Antes de continuar, configurá tus horarios de atención.";
                return RedirectToAction("Editar", "Perfil");
            }

            // Reparar turnos con Estado=0 (bug de turnos creados sin setear Estado)
            var turnosSinEstado = await _context.Turnos
                .Where(t => t.IdMedico == idMedico && (int)(object)t.Estado == 0)
                .ToListAsync();
            if (turnosSinEstado.Any())
            {
                turnosSinEstado.ForEach(t => t.Estado = EstadoTurno.Solicitado);
                await _context.SaveChangesAsync();
            }

            var hoy = DateTime.Today;
            var manana = hoy.AddDays(1);

            var turnosHoy = await _context.Turnos
                .Where(t => t.IdMedico == idMedico && t.FechaHora >= hoy && t.FechaHora < manana)
                .Include(t => t.Paciente).ThenInclude(p => p.Usuario)
                .Include(t => t.Consulta)
                .OrderBy(t => t.FechaHora)
                .ToListAsync();

            var proximos = await _context.Turnos
                .Where(t => t.IdMedico == idMedico && t.FechaHora >= manana
                         && t.Estado != EstadoTurno.Cancelado)
                .Include(t => t.Paciente).ThenInclude(p => p.Usuario)
                .OrderBy(t => t.FechaHora)
                .Take(15)
                .ToListAsync();

            var vm = new MedicoDashboardVM
            {
                IdMedico = idMedico.Value,
                NombreCompleto = $"Dr/a. {medico.Usuario.Nombre} {medico.Usuario.Apellido}",
                Matricula = medico.Matricula,
                TurnosHoy = turnosHoy.Count(t => t.Estado != EstadoTurno.Cancelado),
                TurnosPendientes = turnosHoy.Count(t => t.Estado == EstadoTurno.Solicitado),
                TurnosConfirmados = turnosHoy.Count(t => t.Estado == EstadoTurno.Confirmado),
                TurnosAtendidos = turnosHoy.Count(t => t.Estado == EstadoTurno.Atendido),
                TurnosDelDia = turnosHoy.Select(t => new TurnoResumenVM
                {
                    IdTurno = t.IdTurno,
                    IdPaciente = t.IdPaciente,
                    FechaHora = t.FechaHora,
                    PacienteNombre = $"{t.Paciente.Usuario.Nombre} {t.Paciente.Usuario.Apellido}",
                    PacienteDni = t.Paciente.Dni,
                    Estado = t.Estado.ToString(),
                    Motivo = t.MotivoConsulta,
                    TieneConsulta = t.Consulta != null
                }).ToList(),
                ProximosTurnos = proximos.Select(t => new TurnoResumenVM
                {
                    IdTurno = t.IdTurno,
                    IdPaciente = t.IdPaciente,
                    FechaHora = t.FechaHora,
                    PacienteNombre = $"{t.Paciente.Usuario.Nombre} {t.Paciente.Usuario.Apellido}",
                    PacienteDni = t.Paciente.Dni,
                    Estado = t.Estado.ToString(),
                    Motivo = t.MotivoConsulta
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmarTurno(int id)
        {
            var idMedico = GetIdMedico();
            var turno = await _context.Turnos.FirstOrDefaultAsync(t => t.IdTurno == id && t.IdMedico == idMedico);
            if (turno == null) return RedirectToAction("Dashboard");

            turno.Estado = EstadoTurno.Confirmado;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Turno confirmado correctamente.";
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> CancelarTurno(int id, string motivo)
        {
            if (string.IsNullOrWhiteSpace(motivo))
            {
                TempData["Error"] = "Debés indicar el motivo de cancelación.";
                return RedirectToAction("Dashboard");
            }

            var idMedico = GetIdMedico();
            var turno = await _context.Turnos.FirstOrDefaultAsync(t => t.IdTurno == id && t.IdMedico == idMedico);
            if (turno == null) return RedirectToAction("Dashboard");

            turno.Estado = EstadoTurno.Cancelado;
            turno.MotivoConsulta = $"[CANCELADO POR MÉDICO] {motivo}";
            await _context.SaveChangesAsync();
            TempData["Success"] = "Turno cancelado.";
            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public async Task<IActionResult> AtenderTurno(int id)
        {
            var idMedico = GetIdMedico();
            var turno = await _context.Turnos
                .Include(t => t.Paciente).ThenInclude(p => p.Usuario)
                .Include(t => t.Consulta)
                .FirstOrDefaultAsync(t => t.IdTurno == id && t.IdMedico == idMedico);

            if (turno == null || turno.Estado == EstadoTurno.Cancelado)
                return RedirectToAction("Dashboard");

            // Si ya tiene consulta, redirigir al historial
            if (turno.Consulta != null)
                return RedirectToAction("HistorialPaciente", new { idPaciente = turno.IdPaciente });

            var vm = new ConsultaVM
            {
                IdTurno = turno.IdTurno,
                PacienteNombre = $"{turno.Paciente.Usuario.Nombre} {turno.Paciente.Usuario.Apellido}",
                PacienteDni = turno.Paciente.Dni,
                FechaHora = turno.FechaHora,
                MotivoConsulta = turno.MotivoConsulta
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> GuardarConsulta(ConsultaVM model)
        {
            if (!ModelState.IsValid) return View("AtenderTurno", model);

            var idMedico = GetIdMedico();
            var turno = await _context.Turnos.FirstOrDefaultAsync(t => t.IdTurno == model.IdTurno && t.IdMedico == idMedico);
            if (turno == null) return RedirectToAction("Dashboard");

            var consulta = new Consulta
            {
                IdTurno = model.IdTurno,
                Diagnostico = model.Diagnostico,
                Observaciones = model.Observaciones,
                Indicaciones = model.Indicaciones,
                FechaConsulta = DateTime.Now
            };
            _context.Consultas.Add(consulta);
            turno.Estado = EstadoTurno.Atendido;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Consulta guardada correctamente.";
            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public async Task<IActionResult> HistorialPaciente(int idPaciente)
        {
            var paciente = await _context.Pacientes
                .Include(p => p.Usuario)
                .FirstOrDefaultAsync(p => p.IdPaciente == idPaciente);
            if (paciente == null) return RedirectToAction("Dashboard");

            var consultas = await _context.Consultas
                .Include(c => c.Turno).ThenInclude(t => t.Medico).ThenInclude(m => m.Usuario)
                .Where(c => c.Turno.IdPaciente == idPaciente)
                .OrderByDescending(c => c.FechaConsulta)
                .ToListAsync();

            var vm = new HistorialPacienteVM
            {
                IdPaciente = idPaciente,
                NombreCompleto = $"{paciente.Usuario.Nombre} {paciente.Usuario.Apellido}",
                Dni = paciente.Dni,
                Telefono = paciente.Telefono,
                FechaNacimiento = paciente.FechaNacimiento,
                Consultas = consultas.Select(c => new ConsultaHistorialVM
                {
                    IdConsulta = c.IdConsulta,
                    FechaConsulta = c.FechaConsulta,
                    MedicoNombre = $"Dr/a. {c.Turno.Medico.Usuario.Nombre} {c.Turno.Medico.Usuario.Apellido}",
                    Diagnostico = c.Diagnostico,
                    Observaciones = c.Observaciones,
                    Indicaciones = c.Indicaciones
                }).ToList()
            };

            return View(vm);
        }
    }
}
﻿
