using Application.DTOs.Auth;
using Application.Interfaces;
using Application.UseCases.Usuarios.Login;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{    
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ICommandHandler<LoginCommand, AuthResponseDto> _loginHandler;

        public AuthController(ICommandHandler<LoginCommand, AuthResponseDto> loginHandler)
        {
            _loginHandler = loginHandler;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken ct)
        {            
            var command = new LoginCommand(request.Email, request.Password);
            var resultado = await _loginHandler.HandleAsync(command, ct);
            return Ok(resultado);
        }        
    }
}
