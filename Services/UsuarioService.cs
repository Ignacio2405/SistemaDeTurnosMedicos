using Microsoft.EntityFrameworkCore;
using SistemaSaludGoya.Data;
using SistemaSaludGoya.Models;

namespace SistemaSaludGoya.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly AppDbContext _context;

        public UsuarioService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Usuario>> ObtenerTodosAsync()
        {
            return await _context.Usuarios
                .AsNoTracking()
                .OrderBy(u => u.Apellido)
                .ThenBy(u => u.Nombre)
                .ToListAsync();
        }

        public async Task<Usuario?> ObtenerPorIdAsync(int id)
        {
            return await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u =>
                    u.IdUsuario == id);
        }

        public async Task ActualizarAsync(Usuario usuario)
        {
            var usuarioDb = await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.IdUsuario == usuario.IdUsuario);

            if (usuarioDb == null)
                throw new Exception("Usuario no encontrado");

            usuarioDb.Nombre = usuario.Nombre;
            usuarioDb.Apellido = usuario.Apellido;
            usuarioDb.Email = usuario.Email;
            usuarioDb.Activo = usuario.Activo;

            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.IdUsuario == id);

            if (usuario == null)
                return;

            _context.Usuarios.Remove(usuario);

            await _context.SaveChangesAsync();
        }
    }
}