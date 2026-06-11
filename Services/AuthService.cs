using Microsoft.EntityFrameworkCore;
using SistemaSaludGoya.Data;
using SistemaSaludGoya.Models;
using SistemaSaludGoya.ViewModel;

namespace SistemaSaludGoya.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;

    public AuthService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> LoginAsync(string email, string password)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Paciente)
            .Include(u => u.UsuarioRoles).ThenInclude(ur => ur.Rol)
            .FirstOrDefaultAsync(u => u.Email == email && u.Activo);

        if (usuario == null) return null;

        return BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash) ? usuario : null;
    }

    public async Task<bool> RegistrarPacienteAsync(RegisterPacienteVM model)
    {
        bool existe = await _context.Usuarios.AnyAsync(u => u.Email == model.Email);
        if (existe) return false;

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

        var paciente = new Paciente
        {
            IdUsuario       = usuario.IdUsuario,
            Dni             = model.Dni,
            FechaNacimiento = model.FechaNacimiento,
            Telefono        = model.Telefono,
            Direccion       = model.Direccion
        };
        _context.Pacientes.Add(paciente);

        // Asignar rol Paciente
        var rolPaciente = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == "Paciente");
        if (rolPaciente != null)
        {
            _context.UsuariosRoles.Add(new UsuarioRol { IdUsuario = usuario.IdUsuario, IdRol = rolPaciente.IdRol });
        }

        await _context.SaveChangesAsync();
        return true;
    }
}
