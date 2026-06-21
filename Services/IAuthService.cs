using SistemaSaludGoya.Models;
using SistemaSaludGoya.ViewModel;

namespace SistemaSaludGoya.Services;

public interface IAuthService
{
    Task<(Usuario? Usuario, string? RolNombre, string? Error)> ProcesarLoginAsync(string email, string password);

    Task<bool> RegistrarPacienteAsync(RegisterPacienteVM model);

    Task<(bool Ok, string Mensaje)> RegistrarMedicoAsync(RegisterMedicoVM model);
}