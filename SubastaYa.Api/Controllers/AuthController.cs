using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubastaYa.Core.DTOs.Auth;
using SubastaYa.Core.Interfaces;
using SubastaYa.Infrastructure.Data;
using System.Security.Claims;

namespace SubastaYa.Api.Controllers
{    
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtProvider _jwtProvider;

        public AuthController(ApplicationDbContext context, IJwtProvider jwtProvider)
        {
            _context = context;
            _jwtProvider = jwtProvider;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {            
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (usuario is null || !BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash))
            {
                return Unauthorized(new { mensaje = "Credenciales inválidas" });
            }

            var token = _jwtProvider.GenerarToken(usuario);

            return Ok(new AuthResponseDto(token, usuario.Email));
        }

        [Authorize]
        [HttpGet("prueba-protegida")]
        public IActionResult ProbarAutorizacion()
        {
            // Extrae el Id del usuario a partir del claim "sub" guardado en el JWT
            var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst("sub")?.Value;

            // Extrae el correo a partir del claim de email
            var usuarioEmail = User.FindFirst(ClaimTypes.Email)?.Value
                               ?? User.FindFirst("email")?.Value;

            return Ok(new
            {
                Mensaje = "¡Acceso concedido! Tu token JWT es 100% válido.",
                IdRecuperado = usuarioId,
                EmailRecuperado = usuarioEmail,
                FechaConsulta = DateTime.UtcNow
            });
        }
    }
}
