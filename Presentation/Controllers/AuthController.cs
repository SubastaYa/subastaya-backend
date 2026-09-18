using Application.DTOs.Auth;
using Application.UseCases.Usuarios.Login;
using Application.UseCases.Usuarios.Registrar;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ICommandHandler<LoginCommand, AuthResponseDto> _loginHandler;
        private readonly ICommandHandler<RegistrarUsuarioCommand, AuthResponseDto> _registerHandler;

        public AuthController(
            ICommandHandler<LoginCommand, AuthResponseDto> loginHandler,
            ICommandHandler<RegistrarUsuarioCommand, AuthResponseDto> registerHandler)
        {
            _loginHandler = loginHandler;
            _registerHandler = registerHandler;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken ct)
        {
            var command = new LoginCommand(request.Email, request.Password);
            var resultado = await _loginHandler.HandleAsync(command, ct);
            return Ok(resultado);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request, CancellationToken ct)
        {
            var command = new RegistrarUsuarioCommand(request.Email, request.Nombre, request.Password);
            var resultado = await _registerHandler.HandleAsync(command, ct);
            return Ok(resultado);
        }
    }
}
