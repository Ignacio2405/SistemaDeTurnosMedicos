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

    public async Task<(Usuario? Usuario, string? RolNombre, string? Error)> ProcesarLoginAsync(string email, string password)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Paciente)
            .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
                    .ThenInclude(r => r.RolPermisos)
                        .ThenInclude(rp => rp.Permiso)
            .FirstOrDefaultAsync(u => u.Email == email && u.Activo);

        if (usuario == null || !BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash))
            return (null, null, "Email o contraseña incorrectos");

        var usuarioRol = usuario.UsuarioRoles.FirstOrDefault();

        if (usuarioRol != null)
            return (usuario, usuarioRol.Rol.Nombre, null);

        var tienePaciente = await _context.Pacientes.AnyAsync(p => p.IdUsuario == usuario.IdUsuario);

        if (tienePaciente)
        {
            var rolPaciente = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == "paciente");
            if (rolPaciente != null)
            {
                _context.UsuariosRoles.Add(new UsuarioRol { IdUsuario = usuario.IdUsuario, IdRol = rolPaciente.IdRol });
                await _context.SaveChangesAsync();

                usuario = await _context.Usuarios
                    .Include(u => u.UsuarioRoles)
                        .ThenInclude(ur => ur.Rol)
                            .ThenInclude(r => r.RolPermisos)
                                .ThenInclude(rp => rp.Permiso)
                    .FirstOrDefaultAsync(u => u.IdUsuario == usuario.IdUsuario);

                return (usuario, "paciente", null);
            }
            return (null, null, "Error interno: el rol paciente no existe.");
        }

        return (null, null, "Tu cuenta está pendiente de aprobación por un administrador.");
    }

    public async Task<(bool Ok, string Mensaje)> RegistrarPacienteAsync(RegisterPacienteVM model)
    {
        bool emailExiste = await _context.Usuarios.AnyAsync(u => u.Email == model.Email);
        if (emailExiste) return (false, "Ya existe un usuario registrado con este correo electrónico.");

        bool dniExiste = await _context.Pacientes.AnyAsync(p => p.Dni == model.Dni);
        if (dniExiste) return (false, "El DNI ingresado ya se encuentra registrado en el sistema.");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
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
                FechaNacimiento = model.FechaNacimiento.ToUniversalTime(),
                Telefono = model.Telefono,
                Direccion = model.Direccion
            };
            _context.Pacientes.Add(paciente);

            var rolPaciente = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == "paciente");
            if (rolPaciente != null)
            {
                _context.UsuariosRoles.Add(new UsuarioRol { IdUsuario = usuario.IdUsuario, IdRol = rolPaciente.IdRol });
            }

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            return (true, "Registro exitoso.");
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return (false, "Ocurrió un error inesperado al crear la cuenta. Por favor, intentá nuevamente.");
        }
    }

    public async Task<(bool Ok, string Mensaje)> RegistrarMedicoAsync(RegisterMedicoVM model)
    {
        bool existe = await _context.Usuarios.AnyAsync(u => u.Email == model.Email);
        if (existe)
            return (false, "Ya existe un usuario con ese email.");

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

        return (true, "Solicitud enviada. Un administrador revisará tu cuenta.");
    }
}