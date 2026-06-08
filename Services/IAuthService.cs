using SistemaSaludGoya.Models;
using SistemaSaludGoya.ViewModel;

namespace SistemaSaludGoya.Services;

public interface IAuthService
{
    Task<Usuario?> LoginAsync(string email, string password);

    Task<bool> RegistrarPacienteAsync(RegisterPacienteVM model);
}