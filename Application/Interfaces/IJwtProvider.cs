using Domain.Entities;

namespace Application.Interfaces
{
    public interface IJwtProvider
    {
        string GenerarToken(Usuario usuario);
    }
}
