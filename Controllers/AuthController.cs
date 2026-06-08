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
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var usuario = await _authService.LoginAsync(
                model.Email,
                model.Password);

            if (usuario == null)
            {
                ModelState.AddModelError(
                    "",
                    "Email o contraseña incorrectos");

                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    usuario.IdUsuario.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    $"{usuario.Nombre} {usuario.Apellido}"),

                new Claim(
                    ClaimTypes.Email,
                    usuario.Email)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            return RedirectToAction(
                "Index",
                "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(
            RegisterPacienteVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            bool registrado =
                await _authService.RegistrarPacienteAsync(model);

            if (!registrado)
            {
                ModelState.AddModelError(
                    "",
                    "Ya existe un usuario con ese email");

                return View(model);
            }

            TempData["Success"] =
                "Usuario registrado correctamente";

            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
