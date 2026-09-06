using Application.DTOs.Auth;
using Application.Interfaces;

namespace Application.UseCases.Usuarios.Commands.Login
{
    public record LoginCommand(string Email, string Password) : ICommand<AuthResponseDto>;
}
