using Application.UseCases.Usuarios.Commands.Login;
using Microsoft.AspNetCore.Mvc;

namespace SubastaYa.Api.Controllers
{    
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly LoginCommandHandler _loginHandler;

        public AuthController(LoginCommandHandler loginHandler)
        {
            _loginHandler = loginHandler;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
        {            
            var resultado = await _loginHandler.HandleAsync(command, ct);
            return Ok(resultado);
        }        
    }
}
