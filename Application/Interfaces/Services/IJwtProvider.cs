using Domain.Entities;

namespace Application.Interfaces.Services
{
    public interface IJwtProvider
    {
        string GenerarToken(Usuario usuario);
    }
}
