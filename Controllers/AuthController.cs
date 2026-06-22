using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SistemaSaludGoya.Services;
using SistemaSaludGoya.ViewModel;
using System.Security.Claims;

namespace SistemaSaludGoya.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var resultado = await _authService.ProcesarLoginAsync(model.Email, model.Password);

            if (resultado.Error != null || resultado.Usuario == null)
            {
                ModelState.AddModelError("", resultado.Error ?? "Error de autenticación");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, resultado.Usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, $"{resultado.Usuario.Nombre} {resultado.Usuario.Apellido}"),
                new Claim(ClaimTypes.Email, resultado.Usuario.Email),
                new Claim(ClaimTypes.Role, resultado.RolNombre ?? ""),
                new Claim("IdPaciente", resultado.Usuario.Paciente?.IdPaciente.ToString() ?? "0"),
                new Claim("Rol", resultado.RolNombre ?? "")
            };

            var usuarioRol = resultado.Usuario.UsuarioRoles.FirstOrDefault();

            if (usuarioRol != null && usuarioRol.Rol.RolPermisos != null)
            {
                foreach (var rp in usuarioRol.Rol.RolPermisos)
                {
                    claims.Add(new Claim("Permiso", rp.Permiso.Nombre));
                }
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return resultado.RolNombre switch
            {
                "medico" => RedirectToAction("Dashboard", "Medico"),
                "recepcionista" => RedirectToAction("Dashboard", "Recepcionista"),
                "administrador" => RedirectToAction("Dashboard", "Admin"),
                _ => RedirectToAction("Index", "Home")
            };
        }

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

        [HttpGet]
        public IActionResult RegisterMedico() => View();

        [HttpPost]
        public async Task<IActionResult> RegisterMedico(RegisterMedicoVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var resultado = await _authService.RegistrarMedicoAsync(model);

            if (!resultado.Ok)
            {
                ModelState.AddModelError("", resultado.Mensaje);
                return View(model);
            }

            TempData["Success"] = resultado.Mensaje;
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