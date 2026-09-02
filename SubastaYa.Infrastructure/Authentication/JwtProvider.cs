using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SubastaYa.Core.Entities;
using SubastaYa.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Infrastructure.Authentication
{
    public class JwtProvider : IJwtProvider
    {
        private readonly IConfiguration _configuration;

        public JwtProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerarToken(Usuario usuario)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, usuario.Email),                
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var duracion = int.Parse(jwtSettings["DuracionMinutos"] ?? "120");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(duracion),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
