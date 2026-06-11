using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaSaludGoya.Data;
using SistemaSaludGoya.Models;
using SistemaSaludGoya.Services;
using SistemaSaludGoya.ViewModel;
using System.Security.Claims;

namespace SistemaSaludGoya.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly AppDbContext _context;

        public AuthController(IAuthService authService, AppDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var usuario = await _authService.LoginAsync(model.Email, model.Password);
            if (usuario == null)
            {
                ModelState.AddModelError("", "Email o contraseña incorrectos");
                return View(model);
            }

            // Verificar si tiene rol asignado
            var usuarioConRol = await _context.UsuariosRoles
                .Include(ur => ur.Rol)
                .FirstOrDefaultAsync(ur => ur.IdUsuario == usuario.IdUsuario);

            string rolNombre;

            if (usuarioConRol == null)
            {
                // Recuperación: si tiene registro de Paciente pero no tiene rol asignado
                // (puede pasar si se registró antes de que existiera el seed de roles)
                var tienePaciente = await _context.Pacientes
                    .AnyAsync(p => p.IdUsuario == usuario.IdUsuario);

                if (tienePaciente)
                {
                    // Auto-asignar rol Paciente
                    var rolPaciente = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == "Paciente");
                    if (rolPaciente != null)
                    {
                        _context.UsuariosRoles.Add(new UsuarioRol { IdUsuario = usuario.IdUsuario, IdRol = rolPaciente.IdRol });
                        await _context.SaveChangesAsync();
                        rolNombre = "Paciente";
                    }
                    else
                    {
                        ModelState.AddModelError("", "Error interno: el rol Paciente no existe. Contactá al administrador.");
                        return View(model);
                    }
                }
                else
                {
                    // Usuario sin rol y sin paciente → médico/recepcionista pendiente de aprobación
                    ModelState.AddModelError("", "Tu cuenta está pendiente de aprobación por un administrador.");
                    return View(model);
                }
            }
            else
            {
                rolNombre = usuarioConRol.Rol.Nombre;
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, $"{usuario.Nombre} {usuario.Apellido}"),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, rolNombre),
                new Claim("IdPaciente", usuario.Paciente?.IdPaciente.ToString() ?? "0"),
                new Claim("Rol", rolNombre)
            };

            var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return rolNombre switch
            {
                "Medico"          => RedirectToAction("Dashboard", "Medico"),
                "Recepcionista"   => RedirectToAction("Dashboard", "Recepcionista"),
                "Administrador"   => RedirectToAction("Dashboard", "Admin"),
                _                 => RedirectToAction("Index", "Home")
            };
        }

        // ── Registro Paciente ──────────────────────────────────────
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterPacienteVM model)
        {
            if (!ModelState.IsValid) return View(model);

            bool registrado = await _authService.RegistrarPacienteAsync(model);
            if (!registrado)
            {
                ModelState.AddModelError("", "Ya existe un usuario con ese email");
                return View(model);
            }

            TempData["Success"] = "Cuenta creada correctamente. Ya podés iniciar sesión.";
            return RedirectToAction(nameof(Login));
        }

        // ── Registro Médico (pendiente de aprobación admin) ────────
        [HttpGet]
        public IActionResult RegisterMedico() => View();

        [HttpPost]
        public async Task<IActionResult> RegisterMedico(RegisterMedicoVM model)
        {
            if (!ModelState.IsValid) return View(model);

            bool existe = await _context.Usuarios.AnyAsync(u => u.Email == model.Email);
            if (existe)
            {
                ModelState.AddModelError("", "Ya existe un usuario con ese email.");
                return View(model);
            }

            // Crear usuario SIN rol → queda pendiente de aprobación del admin
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

            TempData["Success"] = "Solicitud enviada. Un administrador revisará tu cuenta y te asignará el rol de Médico.";
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult AccessDenied() => View();
    }
}
