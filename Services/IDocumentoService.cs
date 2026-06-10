using SistemaSaludGoya.Models;

namespace SistemaSaludGoya.Services
{
    public interface IDocumentoService
    {
        Task<List<Documento>> ObtenerTodosPorConsultaAsync(int idConsulta);
        Task<Documento?> ObtenerPorIdAsync(int id);
        Task SubirDocumentoAsync(Documento documento);
        Task EliminarAsync(int id);
    }
}
