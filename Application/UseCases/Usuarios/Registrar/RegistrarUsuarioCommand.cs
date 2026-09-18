using Application.DTOs.Auth;

namespace Application.UseCases.Usuarios.Registrar
{
    public record RegistrarUsuarioCommand(string Email, string Nombre, string Password) : ICommand<AuthResponseDto>;
}
