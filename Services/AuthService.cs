using SistemaSaludGoya.Data;
using SistemaSaludGoya.Models;
using SistemaSaludGoya.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace SistemaSaludGoya.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;

    public AuthService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> LoginAsync(string email, string password) { 
    
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == email);

        if (usuario == null) {
            return null;
        }

        bool passwordValida = BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash);

        return passwordValida ? usuario : null;
    }

    public async Task<bool> RegistrarPacienteAsync(RegisterPacienteVM model) { 
    
        bool exite = await _context.Usuarios.AnyAsync(u => u.Email == model.Email);

        if (exite) {
            return false;
        }

        var usuario = new Usuario
        {
            Nombre = model.Nombre,
            Apellido = model.Apellido,
            Email = model.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
            Activo = true
        };

        _context.Usuarios.Add(usuario);

        await _context.SaveChangesAsync();

        var paciente = new Paciente
        {
            IdUsuario = usuario.IdUsuario,
            Dni = model.Dni,
            FechaNacimiento = model.FechaNacimiento,
            Telefono = model.Telefono,
            Direccion = model.Direccion
        };

        _context.Pacientes.Add(paciente);

        await _context.SaveChangesAsync();

        return true;
    }
}
