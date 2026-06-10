using Microsoft.EntityFrameworkCore;
using SistemaSaludGoya.Data;
using SistemaSaludGoya.Models;

namespace SistemaSaludGoya.Services
{
    public class DocumentoService : IDocumentoService
    {
        private readonly AppDbContext _context;
        public DocumentoService(AppDbContext context) => _context = context;

        public async Task<List<Documento>> ObtenerTodosPorConsultaAsync(int idConsulta)
        {
            return await _context.Documentos
                .Where(d => d.IdConsulta == idConsulta)
                .ToListAsync();
        }

        public async Task<Documento?> ObtenerPorIdAsync(int id)
        {
            return await _context.Documentos.FirstOrDefaultAsync(d => d.IdDocumento == id);
        }

        public async Task SubirDocumentoAsync(Documento documento)
        {
            _context.Documentos.Add(documento);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id)
        {
            var doc = await _context.Documentos.FindAsync(id);
            if (doc != null) { _context.Documentos.Remove(doc); await _context.SaveChangesAsync(); }
        }
    }
}