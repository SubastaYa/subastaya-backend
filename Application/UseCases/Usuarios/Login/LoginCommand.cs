using Application.DTOs.Auth;

namespace Application.UseCases.Usuarios.Login
{
    public record LoginCommand(string Email, string Password) : ICommand<AuthResponseDto>;
}
