using Application.DTOs.Auth;
using Application.Interfaces;
using Domain.Exceptions;

namespace Application.UseCases.Usuarios.Login
{
    public class LoginCommandHandler : ICommandHandler<LoginCommand, AuthResponseDto>
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtProvider _jwtProvider;

        public LoginCommandHandler(
            IUsuarioRepository usuarioRepository,
            IPasswordHasher passwordHasher,
            IJwtProvider jwtProvider)
        {
            _usuarioRepository = usuarioRepository;
            _passwordHasher = passwordHasher;
            _jwtProvider = jwtProvider;
        }

        public async Task<AuthResponseDto> HandleAsync(LoginCommand command, CancellationToken ct = default)
        {
            var usuario = await _usuarioRepository.ObtenerPorEmailAsync(command.Email, ct);

            if (usuario is null || !_passwordHasher.Verificar(command.Password, usuario.PasswordHash))
            {
                throw new AuthenticationFailedException("Credenciales inválidas");
            }

            var token = _jwtProvider.GenerarToken(usuario);

            return new AuthResponseDto(token, usuario.Email);
        }
    }
}
